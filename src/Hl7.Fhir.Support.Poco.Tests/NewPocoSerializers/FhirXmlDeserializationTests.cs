using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace Hl7.Fhir.Support.Poco.Tests.NewPocoSerializers
{
    [TestClass]
    public class FhirXmlDeserializationTests
    {
        [TestMethod]
        public void TryDeserializePrimitiveValue()
        {

            var content = "<active value=\"true\"/>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatype(typeof(FhirBoolean), reader);

            datatype.Should().BeOfType<FhirBoolean>();
            datatype.As<FhirBoolean>().Value.Should().Be(true);
        }

        [TestMethod]
        public void TryDeserializeResourceSinglePrimitive()
        {
            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><active value=\"true\"/></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);
        }


        [TestMethod]
        public void TryDeserializeResourceMultiplePrimitives()
        {

            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><active value=\"true\"/><gender value=\"female\"/></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);
            resource.As<TestPatient>().Gender.Value.Should().Be(TestAdministrativeGender.Female);
        }

        [TestMethod]
        public void TryDeserializeComplexResource()
        {

            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><active value=\"true\"/><name id=\"1337\"><given value=\"foo\"/><given value=\"bar\"/></name><name><given value=\"foo2\"/><given value=\"bar2\"/></name></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);

            resource.As<TestPatient>().Name.Should().HaveCount(2);
            resource.As<TestPatient>().Name[0].ElementId.Should().Be("1337");
            resource.As<TestPatient>().Name[0].Given.Should().Equal("foo", "bar");
            resource.As<TestPatient>().Name[1].Given.Should().Equal("foo2", "bar2");
        }


        [TestMethod]
        public void TryDeserializeListValue()
        {
            var content = "<name><given value=\"foo\"/><given value=\"bar\"/></name>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatype(typeof(TestHumanName), reader);

            datatype.Should().BeOfType<TestHumanName>();
            datatype.As<TestHumanName>().Given.Should().HaveCount(2);
        }

        [TestMethod]
        public void TryDeserializeWrongListValue()
        {
            var content = "<name><given value=\"foo\"/><family value=\"oof\"/><given value=\"bar\"/></name>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatype(typeof(TestHumanName), reader);


            datatype.Should().BeOfType<TestHumanName>();
            datatype.As<TestHumanName>().Given.Should().HaveCount(2);
            datatype.As<TestHumanName>().Family.Should().Be("oof");
        }


        private static XmlReader constructReader(string xml)
        {
            var stringReader = new StringReader(xml);
            var reader = XmlReader.Create(stringReader);
            return reader;
        }

        private static FhirXmlPocoDeserializer getTestDeserializer(FhirXmlPocoDeserializerSettings settings) =>
                new(typeof(TestPatient).Assembly, settings);
    }
}
