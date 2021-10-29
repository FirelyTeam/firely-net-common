using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

            var result = JsonDynamicDeserializer.DeserializePrimitiveValue(ref reader, expected);

            if (code is not null)
                result.Exception.Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(code);
            else
                result.Exception.Should().BeNull();

            if (expected == typeof(byte[]))
            {
                if (result.Exception is null)
                    Convert.ToBase64String((byte[])result.PartialResult!).Should().Be((string)data);
                else
                    result.PartialResult.Should().Be(data);
            }
            else if (expected == typeof(DateTimeOffset))
            {
                if (result.Exception is null)
                    result.PartialResult.Should().BeOfType<DateTimeOffset>().Which.ToFhirDate().Should().Be((string)data);
                else
                    result.PartialResult.Should().Be(data);
            }
            else if (code == JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY.ErrorCode ||
                code == JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_OBJECT.ErrorCode)
#pragma warning disable CS0642 // Possible mistaken empty statement
                ; // nothing to check
#pragma warning restore CS0642 // Possible mistaken empty statement
            else
                result.PartialResult.Should().Be(data);
        }

        [TestMethod]
        public void PrimitiveValueCannotBeComplex()
        {
            TryDeserializePrimitiveValue(new { bla = 4 }, typeof(int), JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_OBJECT.ErrorCode);
        }

        [DataTestMethod]
        [DataRow("OperationOutcome", null)]
        [DataRow("OperationOutcomeX", "JSON116")]
        [DataRow("Meta", null)]
        [DataRow(4, "JSON102")]
        [DataRow(null, "JSON103")]
        public void DeriveClassMapping(object typename, string errorcode, bool checkPartial = false)
        {
            var result = test(typename);
            if (errorcode is null)
                result.Success.Should().BeTrue();
            else
                result.Exception.Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(errorcode);

            if (errorcode is null || checkPartial)
                result.PartialResult!.Name.Should().Be((string)typename);

            static PartialDeserialization<ClassMapping> test(object typename)
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

            PartialDeserialization<PrimitiveType> test()
            {
                var inspector = ModelInspector.ForAssembly(typeof(TestPatient).Assembly);
                var deserializer = new JsonDynamicDeserializer(typeof(TestPatient).Assembly);
                var mapping = inspector.ImportType(targetType)!;

                var reader = constructReader(value);
                reader.Read();

                return deserializer.DeserializeFhirPrimitive(null, "dummy", mapping, ref reader);
            }

            var result = test();

            if (result.Exception is not null)
            {
                if (errorcode is not null)
                    result.Exception.Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(errorcode);
                else
                    throw result.Exception;
            }

            if (expectedObjectValue is not null)
            {
                if (targetType != typeof(Instant))
                    result.PartialResult!.ObjectValue.Should().BeEquivalentTo(expectedObjectValue);
                else
                    result.PartialResult!.ObjectValue.Should().BeOfType<DateTimeOffset>().Which.Year.Should().Be((int)expectedObjectValue!);
            }
        }

        private static PartialDeserialization<Base> deserializeComplex(Type objectType, object testObject, out Utf8JsonReader readerState)
        {
            var inspector = ModelInspector.ForAssembly(typeof(TestPatient).Assembly);
            var deserializer = new JsonDynamicDeserializer(typeof(TestPatient).Assembly);
            var mapping = inspector.ImportType(objectType)!;

            Utf8JsonReader reader = constructReader(testObject);
            reader.Read();

            Base newObject = (Base)mapping.Factory();

            var result = deserializer.DeserializeObjectInto(newObject, mapping, ref reader, inResource: false);
            readerState = reader; // copy

            return result;
        }

        private static Utf8JsonReader constructReader(object testObject)
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(testObject);
            var reader = new Utf8JsonReader(jsonBytes);
            return reader;
        }

        private void assertErrors(Exception? e, JsonFhirException[] expected)
        {
            if (expected.Length > 0)
            {
                e.Should().NotBeNull();

                JsonFhirException[] actual = e switch
                {

                    JsonFhirException jfe => new[] { jfe },
                    AggregateException ae => ae.Flatten().InnerExceptions.Cast<JsonFhirException>().ToArray(),
                    _ => throw new InvalidOperationException("Wrong kind of error received.")
                };

                actual.Length.Should().Be(expected.Length, because: e.Message);
                _ = actual.Zip(expected, (a, e) => a.ErrorCode.Should().Be(e.ErrorCode));
            }
            else
            {
                e.Should().BeNull();
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
            var result = deserializer.DeserializeResourceInternal(ref reader);
            assertErrors(result.Exception, errors);
            reader.TokenType.Should().Be(tokenAfterParsing);
        }

        public static IEnumerable<object[]> TestDeserializeResourceData
        {
            get
            {
                yield return new object[] { 5, JsonTokenType.Number, JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
                yield return new object[] { new { }, JsonTokenType.EndObject, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY };
                yield return new object[] { new { resourceType = 4, crap = 4 }, JsonTokenType.EndObject, JsonSerializerErrors.RESOURCETYPE_SHOULD_BE_STRING };
                yield return new object[] { new { resourceType = "Doesnotexist", crap = 5 }, JsonTokenType.EndObject, JsonSerializerErrors.UNKNOWN_RESOURCE_TYPE };
                yield return new object[] { new { resourceType = nameof(OperationOutcome), crap = 5 }, JsonTokenType.EndObject, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND };
                yield return new object[] { new { resourceType = nameof(Meta) },
                    JsonTokenType.EndObject, JsonSerializerErrors.EXPECTED_A_RESOURCE_TYPE, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY };
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
            var result = deserializeComplex(t, testObject, out var readerState);
            assertErrors(result.Exception, errors);
            readerState.TokenType.Should().Be(token);
            var cdResult = result.PartialResult.Should().BeOfType(t);
            verify?.Invoke(result.PartialResult!);
        }

        private static object?[] data<T>(object data, Action<object> verifier, params object[] args) =>
            new[] { typeof(T), data, JsonTokenType.EndObject, verifier }.Concat(args).ToArray();

        private static object?[] data<T>(object data, JsonTokenType token, params object[] args) =>
            new[] { typeof(T), data, token, default(Action<object>) }.Concat(args).ToArray();

        private static object?[] data<T>(object data, params object[] args) =>
            new[] { typeof(T), data, JsonTokenType.EndObject, null }.Concat(args).ToArray();


        public static IEnumerable<object?[]> CatchesIncorrectlyStructuredComplexData()
        {
            yield return new object?[] { typeof(Extension), 5, JsonTokenType.Number, default(Action<object>), JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
            yield return data<Extension>(5, JsonTokenType.Number, JsonSerializerErrors.EXPECTED_START_OF_OBJECT);
            yield return data<Extension>(new[] { 2, 3 }, JsonTokenType.EndArray, JsonSerializerErrors.START_OF_ARRAY_UNEXPECTED);
            yield return data<Extension>(new { }, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { resourceType = "Whatever" },
                JsonSerializerErrors.RESOURCETYPE_UNEXPECTED_IN_DT, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { }, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY);
            yield return data<Extension>(new { unknown = "test" }, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND);
            yield return data<Extension>(new { url = "test" });
            yield return data<Extension>(new { _url = "test" }, JsonSerializerErrors.USE_OF_UNDERSCORE_ILLEGAL);
            yield return data<Extension>(new { resourceType = "whatever", unknown = "test", url = "test" },
                    JsonSerializerErrors.RESOURCETYPE_UNEXPECTED_IN_DT, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND);
            yield return data<Extension>(new { value = "no type suffix" }, JsonSerializerErrors.CHOICE_ELEMENT_HAS_NO_TYPE);
            yield return data<Extension>(new { valueUnknown = "incorrect type suffix" }, JsonSerializerErrors.CHOICE_ELEMENT_HAS_UNKOWN_TYPE);
            yield return data<Extension>(new { valueBoolean = true }, JsonTokenType.EndObject);
            yield return data<Extension>(new { valueUnknown = "incorrect type suffix", unknown = "unknown" },
                    JsonSerializerErrors.CHOICE_ELEMENT_HAS_UNKOWN_TYPE, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND);
        }

        public static IEnumerable<object?[]> TestNormalArrayData()
        {
            yield return data<ContactDetail>(new { name = "Ewout", telecom = 4 }, checkName, JsonSerializerErrors.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { name = "Ewout", telecom = Array.Empty<object>() }, checkName,
                JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<ContactDetail>(new { name = "Ewout", telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                    checkData, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND);
            yield return data<ContactDetail>(new { name = "Ewout", _telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                 checkData, JsonSerializerErrors.USE_OF_UNDERSCORE_ILLEGAL, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND);
            yield return data<ContactDetail>(new { name = new[] { "Ewout" } }, JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY);

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
            yield return data<ContactDetail>(new { name = new[] { "Ewout" } }, JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY);
            yield return data<ContactDetail>(new { name = new { dummy = "Ewout" } }, JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_OBJECT);
            yield return data<ContactDetail>(new { _name = new[] { "Ewout" } }, JsonSerializerErrors.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { _name = "Ewout" }, JsonSerializerErrors.EXPECTED_START_OF_OBJECT);
            yield return data<ContactDetail>(new { name = "Ewout" }, checkName);
            yield return data<ContactDetail>(new { _name = new { id = "12345" } }, checkId);
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
            yield return data<TestAddress>(new { line = default(string[]) }, JsonSerializerErrors.EXPECTED_START_OF_ARRAY);
            yield return data<TestAddress>(new { line = new string[0] }, JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<TestAddress>(new { line = new string[0], _line = new string[0] }, JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY, JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<TestAddress>(new { line = new string[0], _line = new string?[] { null } }, JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY);
            yield return data<TestAddress>(new { line = new string?[] { null }, _line = new string?[] { null } }, JsonSerializerErrors.PRIMITIVE_ARRAYS_BOTH_NULL);
            yield return data<TestAddress>(new { line = new string?[] { null }, _line = new string?[] { null, null } }, JsonSerializerErrors.PRIMITIVE_ARRAYS_BOTH_NULL, JsonSerializerErrors.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new string?[] { null, null }, _line = new string?[] { null } }, JsonSerializerErrors.PRIMITIVE_ARRAYS_BOTH_NULL, JsonSerializerErrors.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" } }, checkName);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" } } }, checkId1AndName, JsonSerializerErrors.PRIMITIVE_ARRAYS_INCOMPAT_SIZE);
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" }, null } }, checkId1AndName); 
            yield return data<TestAddress>(new { line = new[] { "Ewout", "Wouter" }, _line = new[] { new { id = "1" }, new { id = "2" } } }, checkAll);
            yield return data<TestAddress>(new { line = new[] { "Ewout", null }, _line = new[] { null, new { id = "2" } } });
            yield return data<TestAddress>(new { line = new[] { "Ewout", null }, _line = new[] { new { id = "1" }, null } }, checkId1, JsonSerializerErrors.PRIMITIVE_ARRAYS_BOTH_NULL);
            yield return data<TestAddress>(new { _line = new[] { new { id = "1" }, null } }, checkId1, JsonSerializerErrors.PRIMITIVE_ARRAYS_LONELY_NULL);
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
    }

    // TODO: test recovery
}
#nullable restore