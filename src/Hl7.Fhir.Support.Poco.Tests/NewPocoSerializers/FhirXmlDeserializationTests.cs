using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml;
using ERR = Hl7.Fhir.Serialization.FhirXmlException;

namespace Hl7.Fhir.Support.Poco.Tests.NewPocoSerializers
{
    [TestClass]
    public class FhirXmlDeserializationTests
    {
        [DataTestMethod]
        [DataRow("<active value =\"true\"/>", typeof(FhirBoolean), true, null)]
        [DataRow("<multipleBirthInteger value =\"1\"/>", typeof(Integer), 1, null)]
        [DataRow("<Birthdate value =\"2000-01-01\"/>", typeof(FhirDateTime), "2000-01-01", null)]
        [TestMethod]
        public void TryDeserializePrimitives(string xmlPrimitive, Type expectedFhirType, object expectedValue, string error)
        {
            var reader = constructReader(xmlPrimitive);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatype(expectedFhirType, reader);

            datatype.Should().BeOfType(expectedFhirType);
            datatype.As<PrimitiveType>().ObjectValue.Should().Be(expectedValue);
        }

        [DataTestMethod]
        [DataRow("<foo value =\"true\"/>", typeof(bool), true, null, DisplayName = "XmlBool1")]
        [DataRow("<foo value =\"1\"/>", typeof(bool), "1", ERR.STRING_ISNOTA_BOOLEAN_CODE, DisplayName = "XmlBool2")]
        [DataRow("<foo value =\"2000-01-01\"/>", typeof(DateTimeOffset), "2000-01-01", null, DisplayName = "XmlInstant1")]
        [DataRow("<foo value =\"foo\"/>", typeof(DateTimeOffset), "foo", ERR.STRING_ISNOTAN_INSTANT_CODE, DisplayName = "XmlInstant2")]
        [DataRow("<foo value =\"foo\"/>", typeof(byte[]), "foo", ERR.INCORRECT_BASE64_DATA_CODE, DisplayName = "XmlByteArray")]
        [DataRow("<foo value =\"1\"/>", typeof(int), 1, null, DisplayName = "XmlInteger1")]
        [DataRow("<foo value =\"1.1\"/>", typeof(int), "1.1", ERR.STRING_ISNOTAN_INT_CODE, DisplayName = "XmlInteger2")]
        [DataRow("<foo value =\"1\"/>", typeof(long), 1, null, DisplayName = "XmlLong1")]
        [DataRow("<foo value =\"1.1\"/>", typeof(long), "1.1", ERR.STRING_ISNOTA_LONG_CODE, DisplayName = "XmlLong2")]
        [DataRow("<foo value =\"1\"/>", typeof(uint), 1, ERR.STRING_ISNOTAN_UINT_CODE, DisplayName = "XmlUint1")]
        [DataRow("<foo value =\"-1\"/>", typeof(uint), "-1", ERR.STRING_ISNOTAN_UINT_CODE, DisplayName = "XmlUint2")]
        [DataRow("<foo value =\"3.14\"/>", typeof(decimal), 3.14, null, DisplayName = "XmlDecimal1")]
        [DataRow("<foo value =\"3.14e2\"/>", typeof(decimal), 3.14e2, null, DisplayName = "XmlDecimal1")]
        [DataRow("<foo value =\"3.14e500\"/>", typeof(decimal), "3.14e500", ERR.STRING_ISNOTA_DECIMAL_CODE, DisplayName = "XmlDecimal2")]
        [DataRow("<foo value =\"3.14\"/>", typeof(double), 3.14, null, DisplayName = "XmlDouble1")]
        [DataRow("<foo value =\"1\"/>", typeof(ulong), 1, ERR.STRING_ISNOTAN_ULONG_CODE, DisplayName = "XmlUlong1")]
        [DataRow("<foo value =\"-1\"/>", typeof(ulong), "-1", ERR.STRING_ISNOTAN_ULONG_CODE, DisplayName = "XmlUlong2")]
        [DataRow("<foo value =\"1\"/>", typeof(float), 1, null, DisplayName = "XmlFloat1")]
        public void TryDeserializePrimitiveValue(string xmlPrimitive, Type implementingType, object expectedValue, string expectedErrorCode)
        {
            var reader = constructReader(xmlPrimitive);
            reader.MoveToContent();
            reader.MoveToFirstAttribute();

            var deserializer = getTestDeserializer(new());
            var (value, error) = deserializer.ParsePrimitiveValue(reader, implementingType);

            error?.ErrorCode.Should().Be(expectedErrorCode);

            if (implementingType == typeof(DateTimeOffset) && expectedErrorCode is null)
            {
                value.Should().BeOfType<DateTimeOffset>().Which.ToFhirDate().Should().Be((string)expectedValue);
            }
            else
            {
                value.Should().Be(expectedValue);
            }
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
        public void TryDeserializeNarrative()
        {
            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><text><status value=\"generated\"/><div xmlns=\"http://www.w3.org/1999/xhtml\">this is text</div></text><active value=\"true\"/></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.As<TestPatient>().Text.Status.Should().Be(Narrative.NarrativeStatus.Generated);
            resource.As<TestPatient>().Text.Div.Should().Be("this is text");
        }


        [TestMethod]
        public void TryDeserializeExtensions()
        {
            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><extension url=\"http://fire.ly/fhir/StructureDefinition/extension-test\"><valueString value =\"foo\"/></extension><active value=\"true\"/></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);
            resource.As<TestPatient>().Extension.Should().HaveCount(1);
            resource.As<TestPatient>().Extension[0].Url.Should().Be("http://fire.ly/fhir/StructureDefinition/extension-test");
            resource.As<TestPatient>().Extension[0].Value.As<FhirString>().Value.Should().Be("foo");
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
        public void TryDeserializeContainedResource()
        {
            var content = "<Patient xmlns=\"http://hl7.org/fhir\"><contained><Patient><multipleBirthBoolean value = \"true\"/></Patient></contained><contained><Patient><active value = \"true\"/></Patient></contained><active value=\"true\"/><gender value=\"female\"/></Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResource(reader);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);
            resource.As<TestPatient>().Gender.Value.Should().Be(TestAdministrativeGender.Female);
            resource.As<TestPatient>().Contained.Should().HaveCount(2);
            resource.As<TestPatient>().Contained[0].As<TestPatient>().MultipleBirth.As<FhirBoolean>().Value.Should().Be(true);
            resource.As<TestPatient>().Contained[1].As<TestPatient>().Active.Value.Should().Be(true);
        }

        [TestMethod]
        public void TryDeserializeIncorrectContainedResource()
        {
            var content = "<Patient xmlns=\"http://hl7.org/fhir\">" +
                             "<contained>" +
                                "<Patient>" +
                                    "<multipleBirthBoolean value = \"true\"/>" +
                                "</Patient>" +
                                "<Patient>" +
                                    "<birthdate value = \"2020-01-01\"/>" +
                                "</Patient>" +
                              "</contained>" +
                              "<contained>" +
                                "<Patient>" +
                                    "<active value = \"true\"/>" +
                                "</Patient>" +
                              "</contained>" +
                              "<active value=\"true\"/>" +
                              "<gender value=\"female\"/>" +
                          "</Patient>";

            var reader = constructReader(content);
            reader.Read();

            var deserializer = getTestDeserializer(new());
            var state = new FhirXmlPocoDeserializerState();
            var resource = deserializer.DeserializeResourceInternal(reader, state);

            state.Errors.Should().OnlyContain(ce => ce.ErrorCode == ERR.MULTIPLE_RESOURCES_IN_RESOURCE_CONTAINER_CODE);

            resource.Should().BeOfType<TestPatient>();
            resource.As<TestPatient>().Active.Value.Should().Be(true);
            resource.As<TestPatient>().Gender.Value.Should().Be(TestAdministrativeGender.Female);
            resource.As<TestPatient>().Contained.Should().HaveCount(3);
            resource.As<TestPatient>().Contained[0].As<TestPatient>().MultipleBirth.As<FhirBoolean>().Value.Should().Be(true);
            resource.As<TestPatient>().Contained[1].As<TestPatient>().BirthDate.Should().Be("2020-01-01");
            resource.As<TestPatient>().Contained[2].As<TestPatient>().Active.Value.Should().Be(true);
        }


        [TestMethod]
        public void TryDeserializeComplexResource()
        {
            var content =
            "<Patient xmlns=\"http://hl7.org/fhir\">" +
                "<active value=\"true\"/>" +
                "<name id=\"1337\">" +
                    "<given value=\"foo\"/>" +
                    "<given value=\"bar\"/>" +
                "</name>" +
                "<name>" +
                    "<given value=\"foo2\"/>" +
                    "<given value=\"bar2\"/>" +
                "</name>" +
             "</Patient>";

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
            var content = "<name>" +
                                "<family value=\"oof\"/>" +
                                "<given value=\"foo\"/>" +
                                "<given value=\"rab\"/>" +
                                "<prefix value=\"mr.\"/>" +
                                "<given value=\"bar\"/>" +
                          "</name>";

            var reader = constructReader(content);
            reader.Read();
            var state = new FhirXmlPocoDeserializerState();

            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatypeInternal(typeof(TestHumanName), reader, state);

            datatype.Should().BeOfType<TestHumanName>();
            datatype.As<TestHumanName>().Given.Should().HaveCount(3);
            datatype.As<TestHumanName>().Family.Should().Be("oof");

            state.Errors.Should().OnlyContain(ce => ce.ErrorCode == ERR.UNEXPECTED_ELEMENT_CODE);

        }


        [TestMethod]
        public void TryDeserializeUnknownElement()
        {
            var content = "<name><family value =\"oof\"/><foo value = \"bar\"/><given value=\"foo\"/></name>";

            var reader = constructReader(content);
            reader.Read();

            var state = new FhirXmlPocoDeserializerState();
            var deserializer = getTestDeserializer(new());
            var datatype = deserializer.DeserializeDatatypeInternal(typeof(TestHumanName), reader, state);

            datatype.Should().BeOfType<TestHumanName>();
            datatype.As<TestHumanName>().GivenElement[0].Value.Should().Be("foo");
            datatype.As<TestHumanName>().Family.Should().Be("oof");

            state.Errors.Should().OnlyContain(ce => ce.ErrorCode == ERR.UNKNOWN_ELEMENT_CODE);
        }

        [TestMethod]
        public void TryDeserializeRecursiveElements()
        {
            var content =
            "<CodeSystem>" +
                "<concept>" +
                    "<code value = \"foo\" />" +
                    "<concept>" +
                        "<code value = \"bar\" />" +
                    "</concept>" +
                "</concept>" +
            "</CodeSystem >";

            var reader = constructReader(content);
            reader.Read();

            var state = new FhirXmlPocoDeserializerState();
            var deserializer = getTestDeserializer(new());
            var resource = deserializer.DeserializeResourceInternal(reader, state);
            resource.Should().NotBeNull();

            resource.As<TestCodeSystem>().Concept[0].Code.Should().Be("foo");
            resource.As<TestCodeSystem>().Concept[0].Concept[0].Code.Should().Be("bar");
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
