using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

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

            var result = JsonDynamicDeserializer.DeserializePrimitiveValue(ref reader, expected, allowNull:false);

            if (code is not null)
                result.Exception.Should().BeOfType<JsonFhirException>().Which.ErrorCode.Should().Be(code);
            else
                result.Exception.Should().BeNull();

            if (expected == typeof(byte[]))
            {
                if (result.Exception is null)
                    Convert.ToBase64String((byte[])result.PartialResult).Should().Be((string)data);
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
            else if (code == JsonSerializerErrors.JSON105.ErrorCode)
#pragma warning disable CS0642 // Possible mistaken empty statement
                ; // nothing to check
#pragma warning restore CS0642 // Possible mistaken empty statement
            else
                result.PartialResult.Should().Be(data);
               
            reader.TokenType.Should().Be(JsonTokenType.PropertyName, because: "reader should have moved past the prop value.");
        }
    }

}
