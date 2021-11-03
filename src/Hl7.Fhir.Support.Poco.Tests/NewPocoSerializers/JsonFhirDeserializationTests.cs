using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ERR = Hl7.Fhir.Serialization.JsonFhirException;

#nullable enable

namespace Hl7.Fhir.Support.Poco.Tests
{
    [TestClass]
    public class JsonFhirDeserializationTests
    {
        private const string NUMBER_CANNOT_BE_PARSED = "JSON108";

        [DataTestMethod]
        [DataRow(null, typeof(decimal), "JSON109")]
        [DataRow(new[] { 1, 2 }, typeof(decimal), "JSON105")]

        [DataRow("hi!", typeof(string), null)]
        [DataRow("SGkh", typeof(byte[]), null)]
        [DataRow("hi!", typeof(byte[]), "JSON106")]
        [DataRow("hi!", typeof(DateTimeOffset), "JSON107")]
        [DataRow("2007-02-03", typeof(DateTimeOffset), null)]
        [DataRow("enumvalue", typeof(UriFormat), null)]
        [DataRow(true, typeof(Enum), "JSON110")]
        [DataRow("hi!", typeof(int), "JSON110")]

        [DataRow(3, typeof(decimal), null)]
        [DataRow(3, typeof(uint), null)]
        [DataRow(3, typeof(long), null)]
        [DataRow(3, typeof(ulong), null)]
        [DataRow(3.14, typeof(decimal), null)]
        [DataRow(double.MaxValue, typeof(decimal), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(3.14, typeof(int), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(3.14, typeof(uint), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(3.14, typeof(long), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(-3, typeof(ulong), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(long.MaxValue, typeof(uint), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(long.MaxValue, typeof(int), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(long.MaxValue, typeof(decimal), null)]
        [DataRow(5, typeof(float), null)]
        [DataRow(double.MaxValue, typeof(float), NUMBER_CANNOT_BE_PARSED)]
        [DataRow(6.14, typeof(double), null)]
        [DataRow(314, typeof(int), null)]
        [DataRow(314, typeof(decimal), null)]
        [DataRow(3.14, typeof(bool), "JSON110")]

        [DataRow(true, typeof(bool), null)]
        [DataRow(true, typeof(string), "JSON110")]
        public void TryDeserializePrimitiveValue(object data, Type expected, string code)
        {
            var reader = constructReader(data);
            reader.Read();

            var (result,error) = JsonDynamicDeserializer.DeserializePrimitiveValue(ref reader, expected);

            if (code is not null)
                error?.ErrorCode.Should().Be(code);
            else
                error.Should().BeNull();

            if (expected == typeof(byte[]))
            {
                if (error is null)
                    Convert.ToBase64String((byte[])result!).Should().Be((string)data);
                else
                    result.Should().Be(data);
            }
            else if (expected == typeof(DateTimeOffset))
            {
                if (error is null)
                    result.Should().BeOfType<DateTimeOffset>().Which.ToFhirDate().Should().Be((string)data);
                else
                    result.Should().Be(data);
            }
            else if (code == ERR.EXPECTED_PRIMITIVE_NOT_ARRAY.ErrorCode ||
                code == ERR.EXPECTED_PRIMITIVE_NOT_OBJECT.ErrorCode)
#pragma warning disable CS0642 // Possible mistaken empty statement
                ; // nothing to check
#pragma warning restore CS0642 // Possible mistaken empty statement
            else
                result.Should().Be(data);
        }

        [TestMethod]
        public void PrimitiveValueCannotBeComplex()
        {
            TryDeserializePrimitiveValue(new { bla = 4 }, typeof(int), ERR.EXPECTED_PRIMITIVE_NOT_OBJECT.ErrorCode);
        }

        [DataTestMethod]
        [DataRow("OperationOutcome", null)]
        [DataRow("OperationOutcomeX", "JSON116")]
        [DataRow("Meta", null)]
        [DataRow(4, "JSON102")]
        [DataRow(null, "JSON103")]
        public void DeriveClassMapping(object typename, string errorcode, bool checkPartial = false)
        {
            var (result,error) = test(typename);
            if (errorcode is null)
                error.Should().BeNull();
            else
                error?.ErrorCode.Should().Be(errorcode);

            if (errorcode is null || checkPartial)
                result!.Name.Should().Be((string)typename);

            static (ClassMapping?, JsonFhirException?) test(object typename)
            {
                var inspector = ModelInspector.ForAssembly(typeof(Resource).Assembly);

                var jsonBytes = typename != null
                    ? JsonSerializer.SerializeToUtf8Bytes(new { resourceType = typename })
                    : JsonSerializer.SerializeToUtf8Bytes(new { resourceTypeX = "wrong" });
                var reader = new Utf8JsonReader(jsonBytes);
                reader.Read();

                return JsonDynamicDeserializer.DetermineClassMappingFromInstance(ref reader, inspector);
            }
        }

        [DataTestMethod]
        [DataRow(null, typeof(FhirString), "JSON109")]
        [DataRow(new[] { 1, 2 }, typeof(FhirString), "JSON105")]
        [DataRow("SGkh", typeof(FhirString), null, "SGkh")]

        [DataRow("SGkh", typeof(Base64Binary), null, new byte[] { 72, 105, 33 })]
        [DataRow("hi!", typeof(Base64Binary), "JSON106", "hi!")]
        [DataRow(4, typeof(Base64Binary), "JSON110", 4)]

        [DataRow("2007-", typeof(FhirDateTime), null, "2007-")]
        [DataRow(4.45, typeof(FhirDateTime), "JSON110", 4.45)]

        [DataRow("female", typeof(Code), null, "female")]
        [DataRow("is-a", typeof(Code<FilterOperator>), null, "is-a")]
        [DataRow("female", typeof(Code<FilterOperator>), null, "female")]
        [DataRow(true, typeof(Code), "JSON110", true)]

        [DataRow("hi!", typeof(Instant), "JSON107")]
        [DataRow("2007-02-03", typeof(Instant), null, 2007)]
        public void ParsePrimitiveValue(object value, Type targetType, string errorcode, object? expectedObjectValue = null)
        {
            ExceptionAggregator aggregator = new();

            PrimitiveType test()
            {
                var inspector = ModelInspector.ForAssembly(typeof(TestPatient).Assembly);
                var deserializer = new JsonDynamicDeserializer(typeof(TestPatient).Assembly);
                var mapping = inspector.ImportType(targetType)!;

                var reader = constructReader(value);
                reader.Read();

                return deserializer.DeserializeFhirPrimitive(null, "dummy", mapping, ref reader, aggregator);
            }

            var result = test();

            if (aggregator.HasExceptions)
            {
                if (errorcode is not null)
                    aggregator.Single().Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(errorcode);
                else
                    throw aggregator.Single();
            }

            if (expectedObjectValue is not null)
            {
                if (targetType != typeof(Instant))
                    result.ObjectValue.Should().BeEquivalentTo(expectedObjectValue);
                else
                    result.ObjectValue.Should().BeOfType<DateTimeOffset>().Which.Year.Should().Be((int)expectedObjectValue!);
            }
        }

        private static Base deserializeComplex(Type objectType, object testObject, out Utf8JsonReader readerState, ExceptionAggregator aggregator)
        {
            var inspector = ModelInspector.ForAssembly(typeof(TestPatient).Assembly);
            var deserializer = new JsonDynamicDeserializer(typeof(TestPatient).Assembly);
            var mapping = inspector.ImportType(objectType)!;

            Utf8JsonReader reader = constructReader(testObject);
            reader.Read();

            Base newObject = (Base)mapping.Factory();

            deserializer.DeserializeObjectInto(newObject, mapping, ref reader, inResource: false, aggregator);
            readerState = reader; // copy

            return newObject;
        }

        private static Utf8JsonReader constructReader(object testObject)
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(testObject);
            var reader = new Utf8JsonReader(jsonBytes);
            return reader;
        }

        private static void assertErrors(IEnumerable<JsonFhirException> actual, JsonFhirException[] expected)
        {
            if (expected.Length > 0)
            {
                actual.Should().NotBeEmpty();

                string why = $"Not the same: actual - {string.Join(",", actual.Select(a => a.ErrorCode))} and expected {string.Join(";", expected.Select(a => a.ErrorCode) )}";
                _ = actual.Zip(expected, (a, e) => a.ErrorCode.Should().Be(e.ErrorCode, because: why)).ToList();
                actual.Count().Should().Be(expected.Length, because: why);
                Console.WriteLine($"Found {string.Join(", ", actual.Select(a => a.Message))}");
            }
            else
            {
                actual.Should().BeEmpty();
            }
        }

        [TestMethod]
        [DynamicData(nameof(TestDeserializeResourceData))]
        [DynamicData(nameof(TestDeserializeNestedResource))]
        public void TestDeserializeResource(object testObject, JsonTokenType tokenAfterParsing, params JsonFhirException[] errors)
        {
            var reader = constructReader(testObject);
            reader.Read();

            var deserializer = new JsonDynamicDeserializer(typeof(TestPatient).Assembly);
            var aggregator = new ExceptionAggregator();
            _ = deserializer.DeserializeResourceInternal(ref reader, aggregator);
            assertErrors(aggregator, errors);
            reader.TokenType.Should().Be(tokenAfterParsing);
        }

        public static IEnumerable<object[]> TestDeserializeResourceData
        {
            get
            {
                yield return new object[] { 5, JsonTokenType.Number, ERR.EXPECTED_START_OF_OBJECT };
                yield return new object[] { new { }, JsonTokenType.EndObject, ERR.NO_RESOURCETYPE_PROPERTY };
                yield return new object[] { new { resourceType = 4, crap = 4 }, JsonTokenType.EndObject, ERR.RESOURCETYPE_SHOULD_BE_STRING };
                yield return new object[] { new { resourceType = "Doesnotexist", crap = 5 }, JsonTokenType.EndObject, ERR.UNKNOWN_RESOURCE_TYPE };
                yield return new object[] { new { resourceType = nameof(OperationOutcome), crap = 5 }, JsonTokenType.EndObject, ERR.UNKNOWN_PROPERTY_FOUND };
                yield return new object[] { new { resourceType = nameof(Meta) },
                    JsonTokenType.EndObject, ERR.OBJECTS_CANNOT_BE_EMPTY, ERR.RESOURCE_TYPE_NOT_A_RESOURCE };
            }

        }

        public static IEnumerable<object[]> TestDeserializeNestedResource
        {
            get
            {
                yield return new object[]
                { 
                    new
                    {
                        resourceType = "Parameters",
                        parameter = new[] 
                        { 
                            new 
                            { name = "a", resource = new
                                {
                                    resourceType = "Patient",
                                    active = true
                                } 
                            }
                        }
                    },
                    JsonTokenType.EndObject
                };                
            }
        }

        [TestMethod]
        [DynamicData(nameof(TestPrimitiveArrayData), DynamicDataSourceType.Method)]
        [DynamicData(nameof(CatchesIncorrectlyStructuredComplexData), DynamicDataSourceType.Method)]
        [DynamicData(nameof(TestNormalArrayData), DynamicDataSourceType.Method)]
        [DynamicData(nameof(TestPrimitiveData), DynamicDataSourceType.Method)]        
        public void TestData(Type t, object testObject, JsonTokenType token, Action<object>? verify, params JsonFhirException[] errors)
        {
            var aggregator = new ExceptionAggregator();
            var result = deserializeComplex(t, testObject, out var readerState, aggregator);
            assertErrors(aggregator, errors);
            readerState.TokenType.Should().Be(token);
            var cdResult = result.Should().BeOfType(t);
            verify?.Invoke(result);
        }

        private static object?[] data<T>(object data, Action<object> verifier, params object[] args) =>
            new[] { typeof(T), data, JsonTokenType.EndObject, verifier }.Concat(args).ToArray();

        private static object?[] data<T>(object data, JsonTokenType token, params object[] args) =>
            new[] { typeof(T), data, token, default(Action<object>) }.Concat(args).ToArray();

        private static object?[] data<T>(object data, params object[] args) =>
            new[] { typeof(T), data, JsonTokenType.EndObject, null }.Concat(args).ToArray();


        public static IEnumerable<object?[]> CatchesIncorrectlyStructuredComplexData()
        {
            yield return new object?[] { typeof(Extension), 5, JsonTokenType.Number, default(Action<object>), ERR.EXPECTED_START_OF_OBJECT };
            yield return data<Extension>(5, JsonTokenType.Number, ERR.EXPECTED_START_OF_OBJECT);
            yield return data<Extension>(new[] { 2, 3 }, JsonTokenType.EndArray, ERR.EXPECTED_START_OF_OBJECT);
            yield return data<Extension>(new { }, ERR.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { resourceType = "Whatever" },
                ERR.RESOURCETYPE_UNEXPECTED, ERR.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { }, ERR.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { unknown = "test" }, ERR.UNKNOWN_PROPERTY_FOUND);
            yield return data<Extension>(new { url = "test" });
            yield return data<Extension>(new { _url = "test" }, ERR.USE_OF_UNDERSCORE_ILLEGAL);
            yield return data<Extension>(new { resourceType = "whatever", unknown = "test", url = "test" },
                    ERR.RESOURCETYPE_UNEXPECTED, ERR.UNKNOWN_PROPERTY_FOUND);
            yield return data<Extension>(new { value = "no type suffix" }, ERR.CHOICE_ELEMENT_HAS_NO_TYPE);
            yield return data<Extension>(new { valueUnknown = "incorrect type suffix" }, ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE);
            yield return data<Extension>(new { valueBoolean = true }, JsonTokenType.EndObject);
            yield return data<Extension>(new { valueUnknown = "incorrect type suffix", unknown = "unknown" },
                    ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE, ERR.UNKNOWN_PROPERTY_FOUND);
        }

        public static IEnumerable<object?[]> TestNormalArrayData()
        {
            yield return data<ContactDetail>(new { name = "Ewout", telecom = 4 }, checkName, ERR.EXPECTED_START_OF_ARRAY, ERR.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { name = "Ewout", telecom = Array.Empty<object>() }, checkName,
                ERR.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<ContactDetail>(new { name = "Ewout", telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                    checkData, ERR.UNKNOWN_PROPERTY_FOUND);
            yield return data<ContactDetail>(new { name = "Ewout", _telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                 checkData, ERR.USE_OF_UNDERSCORE_ILLEGAL, ERR.UNKNOWN_PROPERTY_FOUND);
            yield return data<ContactDetail>(new { name = new[] { "Ewout" } }, ERR.EXPECTED_PRIMITIVE_NOT_ARRAY);

            static void checkName(object parsed) => parsed.Should().BeOfType<ContactDetail>().Which.Name.Should().Be("Ewout");

            static void checkData(object parsedObject)
            {
                checkName(parsedObject);

                var parsed = parsedObject.Should().BeOfType<ContactDetail>().Subject;
                parsed.Telecom.Count.Should().Be(2);
                parsed.Telecom[0].System.Should().Be(ContactPoint.ContactPointSystem.Phone);
                parsed.Telecom[1].Count().Should().Be(0);
            }
        }

        public static IEnumerable<object?[]> TestPrimitiveData()
        {
            yield return data<ContactDetail>(new { name = new[] { "Ewout" } }, ERR.EXPECTED_PRIMITIVE_NOT_ARRAY);
            yield return data<ContactDetail>(new { name = new { dummy = "Ewout" } }, ERR.EXPECTED_PRIMITIVE_NOT_OBJECT);
            yield return data<ContactDetail>(new { _name = new[] { "Ewout" } }, ERR.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { _name = "Ewout" }, ERR.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { name = "Ewout" }, checkName);
            yield return data<ContactDetail>(new { _name = new { id = "12345" } }, checkId);
            yield return data<ContactDetail>(new { _name = new { id = true } }, ERR.INCOMPATIBLE_SIMPLE_VALUE);
            yield return data<ContactDetail>(new { name = "Ewout", _name = new { id = "12345" } }, checkAll);

            static void checkName(object parsed) => parsed.Should().BeOfType<ContactDetail>().Which.NameElement.Value.Should().Be("Ewout");
            static void checkId(object parsed) => parsed.Should().BeOfType<ContactDetail>().Which.NameElement.ElementId.Should().Be("12345");
            static void checkAll(object parsed)
            {
                checkName(parsed);
                checkId(parsed);
            }
        }

        public static IEnumerable<object?[]> TestPrimitiveArrayData()
        {
            yield return data<TestAddress>(new { line = "hi!" }, ERR.EXPECTED_START_OF_ARRAY);
            yield return data<TestAddress>(new { line = Array.Empty<string>() }, ERR.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<TestAddress>(new { line = Array.Empty<string>(), _line = Array.Empty<string>() }, ERR.ARRAYS_CANNOT_BE_EMPTY, ERR.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<TestAddress>(new { line = Array.Empty<string>(), _line = new string?[] { null } }, ERR.ARRAYS_CANNOT_BE_EMPTY, ERR.PRIMITIVE_ARRAYS_ONLY_NULL);
            yield return data<TestAddress>(new { line = new string?[] { null }, _line = new[] { new { id = "1" } } }, ERR.PRIMITIVE_ARRAYS_ONLY_NULL );
            yield return data<TestAddress>(new { line = new[] { "Ewout" }, _line = new string?[] { null } }, ERR.PRIMITIVE_ARRAYS_ONLY_NULL);
            yield return data<TestAddress>(new { line = new string?[] { null }, _line = new string?[] { null } }, ERR.PRIMITIVE_ARRAYS_ONLY_NULL, ERR.PRIMITIVE_ARRAYS_BOTH_NULL, ERR.PRIMITIVE_ARRAYS_ONLY_NULL);
            yield return data<TestAddress>(new { line = new string?[] { null }, _line = new string?[] { null, null } }, ERR.PRIMITIVE_ARRAYS_ONLY_NULL, ERR.PRIMITIVE_ARRAYS_BOTH_NULL, ERR.PRIMITIVE_ARRAYS_ONLY_NULL, ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new string?[] { null, null }, _line = new string?[] { null } }, ERR.PRIMITIVE_ARRAYS_ONLY_NULL, ERR.PRIMITIVE_ARRAYS_BOTH_NULL, ERR.PRIMITIVE_ARRAYS_ONLY_NULL, ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" } }, checkName);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" } } }, checkId1AndName, ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" }, null } }, checkId1AndName); 
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" }, new { id = "2" } } }, checkAll);
            yield return data<TestAddress>(new { line = new[] { "Ewout", null }, _line = new[] { null, new { id = "2" } } });
            yield return data<TestAddress>(new { line = new[] { "Ewout", null }, _line = new[] { new { id = "1" }, null } }, checkId1, ERR.PRIMITIVE_ARRAYS_BOTH_NULL);
            yield return data<TestAddress>(new { _line = new[] { new { id = "1" }, null } }, checkId1, ERR.PRIMITIVE_ARRAYS_LONELY_NULL);
            yield return data<TestAddress>(new { _line = new[] { new { id = "1" }, new { id = "2" } } }, checkIds);

            static void checkName(object parsed) => parsed.Should().BeOfType<TestAddress>().Which.Line.Should().BeEquivalentTo("Ewout", "Wouter");
            static void checkIds(object parsed) => 
                parsed.Should().BeOfType<TestAddress>().Which.LineElement.Select(le => le.ElementId).Should().BeEquivalentTo("1","2");
            static void checkId1(object parsed) =>
                parsed.Should().BeOfType<TestAddress>().Which.LineElement.Select(le => le.ElementId).Should().BeEquivalentTo("1", null);
            static void checkId1AndName(object parsed)
            {
                checkName(parsed);
                checkId1(parsed);
            }
            static void checkAll(object parsed)
            {
                checkName(parsed);
                checkIds(parsed);
            }

        }

        [TestMethod]
        public void TestParseResourcePublicMethod()
        {
            var deserializer = new JsonDynamicDeserializer(typeof(Resource).Assembly);
            var reader = constructReader(
                    new
                    {
                        resourceType = "Parameters",
                        parameter = new[]
                        {
                            new { name = "a" }
                        }
                    });

            deserializer.DeserializeResource(ref reader).Should().NotBeNull();

            reader = constructReader(
                    new
                    {
                        resourceType = "ParametersX",
                    });

            try
            {
                deserializer.DeserializeResource(ref reader);
                Assert.Fail();
            }
            catch(DeserializationFailedException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void TestParseObjectPublicMethod()
        {
            var deserializer = new JsonDynamicDeserializer(typeof(Resource).Assembly);
            var reader = constructReader(
                    new
                    {
                        name = "Ewout"
                    });

            deserializer.DeserializeObject<ContactDetail>(ref reader).Should().NotBeNull();

            reader = constructReader(
                    new
                    {
                        nameX = "Ewout",
                    });

            try
            {
                deserializer.DeserializeObject<ContactDetail>(ref reader);
                Assert.Fail();
            }
            catch (DeserializationFailedException)
            {
                // ok!
            }

            try
            {
                deserializer.DeserializeObject(typeof(JsonFhirDeserializationTests), ref reader);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // ok!
            }
        }

        [TestMethod]
        public void TestRecovery()
        {
            var filename = Path.Combine("TestData", "fp-test-patient-errors.json");
            var jsonInput = File.ReadAllText(filename);

            // For now, deserialize with the existing deserializer, until we have completed
            // the dynamicserializer too.
            var options = new JsonSerializerOptions().ForFhir(typeof(TestPatient).Assembly);

            try
            {
                var actual = JsonSerializer.Deserialize<TestPatient>(jsonInput, options);
                Assert.Fail("Should have encountered errors.");
            }
            catch(DeserializationFailedException dfe)
            {
                Console.WriteLine(dfe.Message);
                var recoveredActual = JsonSerializer.Serialize(dfe.PartialResult, options);
                Console.WriteLine(recoveredActual);

                var recoveredFilename = Path.Combine("TestData", "fp-test-patient-errors-recovered.json");
                var recoveredExpected = File.ReadAllText(recoveredFilename);

                List<string> errors = new();
                JsonAssert.AreSame("fp-test-patient-json-errors/recovery", recoveredExpected, recoveredActual, errors);
                errors.Should().BeEmpty();

                assertErrors(dfe.InnerExceptions.Cast<JsonFhirException>(), new[] 
                {
                    ERR.STRING_ISNOTAN_INSTANT,
                    ERR.RESOURCETYPE_UNEXPECTED,
                    ERR.UNKNOWN_RESOURCE_TYPE,
                    ERR.RESOURCE_TYPE_NOT_A_RESOURCE,
                    ERR.RESOURCETYPE_SHOULD_BE_STRING,
                    ERR.NO_RESOURCETYPE_PROPERTY,
                    ERR.INCOMPATIBLE_SIMPLE_VALUE,
                    ERR.EXPECTED_START_OF_ARRAY,
                    ERR.UNKNOWN_PROPERTY_FOUND, // mother is not a property of HumanName
                    ERR.EXPECTED_PRIMITIVE_NOT_ARRAY, // family is not an array,
                    ERR.PRIMITIVE_ARRAYS_INCOMPAT_SIZE, // given and _given not the same length
                    ERR.EXPECTED_PRIMITIVE_NOT_NULL, // telecom use cannot be null
                    ERR.EXPECTED_PRIMITIVE_NOT_OBJECT, // address.use is not an object
                    ERR.PRIMITIVE_ARRAYS_BOTH_NULL, // address.line should not have a null at the same position in both arrays
                    ERR.PRIMITIVE_ARRAYS_ONLY_NULL, // Questionnaire._subjectType cannot be just null
                    ERR.EXPECTED_START_OF_OBJECT, // item.code is a complex object, not a boolean
                    ERR.PRIMITIVE_ARRAYS_LONELY_NULL, // given cannot be the only array with a null
                    ERR.UNEXPECTED_JSON_TOKEN, // telecom.rank should be a number, not a boolean
                    ERR.USE_OF_UNDERSCORE_ILLEGAL, // should be extension.url, not extension._url
                    ERR.UNEXPECTED_JSON_TOKEN, // gender.extension.valueCode should be a string, not a number
                    ERR.CHOICE_ELEMENT_HAS_NO_TYPE, // extension.value is incorrect
                    ERR.CHOICE_ELEMENT_HAS_UNKOWN_TYPE, // extension.valueSuperDecimal is incorrect
                    ERR.UNEXPECTED_JSON_TOKEN, // deceasedBoolean should be a boolean not a string
                    ERR.NUMBER_CANNOT_BE_PARSED, // multipleBirthInteger should not be a float (3.14)
                    ERR.INCORRECT_BASE64_DATA,
                    ERR.ARRAYS_CANNOT_BE_EMPTY,
                    ERR.OBJECTS_CANNOT_BE_EMPTY
                });                
            }
            
        }
    }

}
#nullable restore