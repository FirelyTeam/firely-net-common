using BenchmarkDotNet.Attributes;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace Firely.Sdk.Benchmarks.Common
{
    [MemoryDiagnoser]
    public class DeserializationBenchmarks
    {
        internal string JsonData;
        internal string XmlData;
        internal FhirXmlPocoDeserializer XmlDeserializer;
        internal FhirJsonPocoDeserializer JsonDeserializer;
        internal XmlReader xmlreader;
        byte[] JsonBytes;

        [GlobalSetup]
        public void BenchmarkSetup()
        {
            var jsonFileName = Path.Combine("TestData", "fp-test-patient.json");
            JsonData = File.ReadAllText(jsonFileName);

            var xmlFileName = Path.Combine("TestData", "fp-test-patient.xml");
            XmlData = File.ReadAllText(xmlFileName);

            xmlreader = XmlReader.Create(new StringReader(XmlData));
            XmlDeserializer = new FhirXmlPocoDeserializer(typeof(TestPatient).Assembly);

            JsonDeserializer = new FhirJsonPocoDeserializer(typeof(TestPatient).Assembly);
            JsonBytes = JsonSerializer.SerializeToUtf8Bytes<string>(JsonData);
        }

        [Benchmark]
        public Resource JsonDictionaryDeserializer()
        {
            var reader = new Utf8JsonReader(JsonBytes);
            try
            {
                return JsonDeserializer.DeserializeResource(ref reader);
            }
            catch (DeserializationFailedException ex)
            {
                return (Resource)ex.PartialResult;
            }
            // return new FhirJsonPocoDeserializer(typeof(TestPatient).Assembly).DeserializeResource(ref reader);
        }

        [Benchmark]
        public Resource XmlDictionaryDeserializer()
        {
            try
            {
                return XmlDeserializer.DeserializeResource(xmlreader);
            }
            catch (DeserializationFailedException ex)
            {
                return (Resource)ex.PartialResult;
            }
        }


        [Benchmark]
        public TestPatient TypedElementDeserializerJson()
        {
            return TypedSerialization.ToPoco<TestPatient>(FhirJsonNode.Parse(JsonData));
        }

        [Benchmark]
        public Resource TypedElementDeserializerXml()
        {
            return TypedSerialization.ToPoco<TestPatient>(FhirXmlNode.Parse(XmlData));
        }
    }
}
