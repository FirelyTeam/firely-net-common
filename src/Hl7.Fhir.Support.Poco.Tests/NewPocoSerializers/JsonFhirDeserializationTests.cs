using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

#nullable enable

namespace Hl7.Fhir.Support.Poco.Tests
{
    [TestClass]
    public class JsonFhirDeserializationTests
    {
        record TestContainer(object Data, string Next)
        {
        }

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

        [DataRow(3.14, typeof(decimal), null)]
        [DataRow(3.14, typeof(int), "JSON108")]
        [DataRow(long.MaxValue, typeof(int), "JSON108")]
        [DataRow(314, typeof(int), null)]
        [DataRow(314, typeof(decimal), null)]
        [DataRow(3.14, typeof(bool), "JSON110")]

        [DataRow(true, typeof(bool), null)]
        [DataRow(true, typeof(string), "JSON110")]
        public void TryDeserializePrimitiveValue(object data, Type expected, string code)
        {
            var t1 = new TestContainer(data, "Hi!");
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(t1);
            var reader = new Utf8JsonReader(jsonBytes);
            do { reader.Read(); } while (reader.TokenType != JsonTokenType.PropertyName);
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
            else if(expected == typeof(DateTimeOffset))
            {
                if (result.Exception is null)
                    result.PartialResult.Should().BeOfType<DateTimeOffset>().Which.ToFhirDate().Should().Be((string)data);
                else
                    result.PartialResult.Should().Be(data);
            }
            else if (code == JsonSerializerErrors.EXPECTED_PRIMITIVE_NOT_ARRAY.ErrorCode)
#pragma warning disable CS0642 // Possible mistaken empty statement
                ; // nothing to check
#pragma warning restore CS0642 // Possible mistaken empty statement
            else
                result.PartialResult.Should().Be(data);
               
            reader.TokenType.Should().Be(JsonTokenType.PropertyName, because: "reader should have moved past the prop value.");
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
            if(errorcode is null) 
                result.Success.Should().BeTrue();
            else
                result.Exception.Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(errorcode);
            
            if(errorcode is null || checkPartial)
                result.PartialResult!.Name.Should().Be((string)typename);

            static PartialDeserialization<ClassMapping> test(object typename)
            {
                var inspector = ModelInspector.ForAssembly(typeof(Resource).Assembly);

                var jsonBytes = typename != null 
                    ? JsonSerializer.SerializeToUtf8Bytes(new { resourceType = typename })
                    : JsonSerializer.SerializeToUtf8Bytes(new { resorceType = "wrong" });
                var reader = new Utf8JsonReader(jsonBytes);

                return JsonDynamicDeserializer.DetermineClassMappingFromInstance(ref reader, inspector);
            }
        }

        //TODO: test fhir primitive with id/extension

        [DataTestMethod]
        [DataRow(null, typeof(FhirString), "JSON109")]
        [DataRow(new[] { 1, 2 }, typeof(FhirString), "JSON105")]
        [DataRow("SGkh", typeof(FhirString), null, "SGkh")]

        [DataRow("SGkh", typeof(Base64Binary), null, new byte[] {72,105,33} )]
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

                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(value);
                var reader = new Utf8JsonReader(jsonBytes);
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
    }

}

#nullable restore