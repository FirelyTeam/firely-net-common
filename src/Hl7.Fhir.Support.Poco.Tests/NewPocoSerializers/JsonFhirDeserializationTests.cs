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
                var inspector = ModelInspector.ForAssembly(typeof(Resource).Assembly);
                var deserializer = new JsonDynamicDeserializer(typeof(Resource).Assembly);
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

        internal static PartialDeserialization<Base> TestDeserializeComplex(Type objectType, object testObject, out Utf8JsonReader readerState)
        {
            var inspector = ModelInspector.ForAssembly(typeof(Resource).Assembly);
            var deserializer = new JsonDynamicDeserializer(typeof(Resource).Assembly);
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

                actual.Length.Should().Be(expected.Length);
                _ = actual.Zip(expected, (a, e) => a.ErrorCode.Should().Be(e.ErrorCode));
            }
            else
            {
                e.Should().BeNull();
            }
        }

        [TestMethod]
        [DynamicData(nameof(CatchesIncorrectlyStructuredComplexData))]
        public void CatchesIncorrectlyStructuredComplex(object testObject, JsonTokenType tokenAfterParsing, params JsonFhirException[] errors)
        {
            var result = TestDeserializeComplex(typeof(Extension), testObject, out var readerState);
            assertErrors(result.Exception, errors);
            readerState.TokenType.Should().Be(tokenAfterParsing);
        }

        public static IEnumerable<object[]> CatchesIncorrectlyStructuredComplexData
        {
            get
            {
                yield return new object[] { 5, JsonTokenType.Number, JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
                yield return new object[] { new[] { 2,3 }, JsonTokenType.EndArray, JsonSerializerErrors.START_OF_ARRAY_UNEXPECTED };
                yield return new object[] { new { }, JsonTokenType.EndObject, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY };
                yield return new object[] { new { resourceType = "Whatever" }, JsonTokenType.EndObject,
                    JsonSerializerErrors.RESOURCETYPE_UNEXPECTED_IN_DT, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY };
                yield return new object[] { new { }, JsonTokenType.EndObject, JsonSerializerErrors.OBJECTS_CANNOT_BE_EMPTY };
                yield return new object[] { new { unknown = "test" }, JsonTokenType.EndObject, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND };
                yield return new object[] { new { url = "test" }, JsonTokenType.EndObject };
                yield return new object[] { new { _url = "test" }, JsonTokenType.EndObject, JsonSerializerErrors.USE_OF_UNDERSCORE_ILLEGAL };
                yield return new object[] { new { resourceType = "whatever", unknown = "test", url = "test" }, JsonTokenType.EndObject,
                    JsonSerializerErrors.RESOURCETYPE_UNEXPECTED_IN_DT, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND };
                yield return new object[] { new { value = "no type suffix" }, JsonTokenType.EndObject,
                    JsonSerializerErrors.CHOICE_ELEMENT_HAS_NO_TYPE };
                yield return new object[] { new { valueUnknown = "incorrect type suffix" }, JsonTokenType.EndObject,
                    JsonSerializerErrors.CHOICE_ELEMENT_HAS_UNKOWN_TYPE };
                yield return new object[] { new { valueBoolean = true }, JsonTokenType.EndObject };
                yield return new object[] { new { valueUnknown = "incorrect type suffix", unknown = "unknown" }, JsonTokenType.EndObject,
                    JsonSerializerErrors.CHOICE_ELEMENT_HAS_UNKOWN_TYPE, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND };
            }
        }

        [TestMethod]
        [DynamicData(nameof(TestDeserializeResourceData))]
        public void TestDeserializeResource(object testObject, JsonTokenType tokenAfterParsing, params JsonFhirException[] errors)
        {
            var reader = constructReader(testObject);
            reader.Read();

            var deserializer = new JsonDynamicDeserializer(typeof(Resource).Assembly);
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

        [TestMethod]
        [DynamicData(nameof(TestNormalArrayData), DynamicDataSourceType.Method)]
        [DynamicData(nameof(TestPrimitiveData), DynamicDataSourceType.Method)]
        public void TestData(object testObject, Action<ContactDetail> verify, params JsonFhirException[] errors)
        {
            var result = TestDeserializeComplex(typeof(ContactDetail), testObject, out var readerState);
            assertErrors(result.Exception, errors);
            readerState.TokenType.Should().Be(JsonTokenType.EndObject);
            var cdResult = result.PartialResult.Should().BeOfType<ContactDetail>().Subject;
            verify(cdResult);         
        }

        public static IEnumerable<object[]> TestNormalArrayData()
        {
            yield return new object[] { new { name = "Ewout", telecom = 4 }, (Action<ContactDetail>)checkName, 
                JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
            yield return new object[] { new { name = "Ewout", telecom = Array.Empty<object>() }, (Action<ContactDetail>)checkName, 
                JsonSerializerErrors.ARRAYS_CANNOT_BE_EMPTY };
            yield return new object[] { new { name = "Ewout", telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                 (Action<ContactDetail>)checkData,
                JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND  };
            yield return new object[] { new { name = "Ewout", _telecom = new object[] { new { system = "phone" }, new { systemX = "b" } } },
                 (Action<ContactDetail>)checkData,
                JsonSerializerErrors.USE_OF_UNDERSCORE_ILLEGAL, JsonSerializerErrors.UNKNOWN_PROPERTY_FOUND };
            yield return new object[] { new { name = new[] { "Ewout" } }, (Action<ContactDetail>)(_ => { }), JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY };

            static void checkName(ContactDetail parsed) => parsed.Name.Should().Be("Ewout");

            static void checkData(ContactDetail parsed)
            {
                checkName(parsed);
                parsed.Telecom.Count.Should().Be(2);
                parsed.Telecom[0].System.Should().Be(ContactPoint.ContactPointSystem.Phone);
                parsed.Telecom[1].Count().Should().Be(0);
            }
        }

        public static IEnumerable<object[]> TestPrimitiveData()
        {
            yield return new object[] { new { name = new[] { "Ewout" } }, (Action<ContactDetail>)(_ => { }),
                JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY };
            yield return new object[] { new { name = new { dummy = "Ewout" } }, (Action<ContactDetail>)(_ => { }),
                JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_OBJECT };
            yield return new object[] { new { _name = new[] { "Ewout" } }, (Action<ContactDetail>)(_ => { }),
                JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
            yield return new object[] { new { _name = "Ewout" }, (Action<ContactDetail>)(_ => { }),
                JsonSerializerErrors.EXPECTED_START_OF_OBJECT };
            yield return new object[] { new { name = "Ewout" }, (Action<ContactDetail>)checkName };
            yield return new object[] { new { _name = new { id = "12345" } }, (Action<ContactDetail>)checkId };
            yield return new object[] { new { name = "Ewout", _name = new { id = "12345" } }, (Action<ContactDetail>)checkAll };

            static void checkName(ContactDetail parsed) => parsed.NameElement.Value.Should().Be("Ewout");
            static void checkId(ContactDetail parsed) => parsed.NameElement.ElementId.Should().Be("12345");
            static void checkAll(ContactDetail parsed)
            {
                checkName(parsed);
                checkId(parsed);
            }
        }

        [TestMethod]
        public void TestParseFullPrimitive()
        {

        }
        //TODO: test fhir primitive with id/extension
        // TODO: test recovery of object using a nested object + property after it.
    }
}
#nullable restore