﻿using BenchmarkDotNet.Attributes;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
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
        internal JsonSerializerOptions options;

        [GlobalSetup]
        public void BenchmarkSetup()
        {
            var jsonFileName = Path.Combine("TestData", "fp-test-patient.json");
            JsonData = File.ReadAllText(jsonFileName);

            var xmlFileName = Path.Combine("TestData", "fp-test-patient.xml");
            XmlData = File.ReadAllText(xmlFileName);

            XmlDeserializer = new FhirXmlPocoDeserializer(typeof(TestPatient).Assembly);
            JsonDeserializer = new FhirJsonPocoDeserializer(typeof(TestPatient).Assembly);

            options = new JsonSerializerOptions().ForFhir(typeof(TestPatient).Assembly);
        }

        [Benchmark]
        public Resource JsonDictionaryDeserializer()
        {
            try
            {
                return JsonSerializer.Deserialize<TestPatient>(JsonData, options);
            }
            catch (DeserializationFailedException e)
            {
                return (Resource)e.PartialResult;
            }

        }

        [Benchmark]
        public Resource XmlDictionaryDeserializer()
        {
            xmlreader = XmlReader.Create(new StringReader(XmlData));
            try
            {
                return XmlDeserializer.DeserializeResource(xmlreader);
            }
            catch (DeserializationFailedException e)
            {
                return (Resource)e.PartialResult;
            }
        }


        [Benchmark]
        public TestPatient TypedElementDeserializerJson()
        {
            return FhirJsonNode.Parse(JsonData).ToPoco<TestPatient>(ModelInspector.ForType<TestPatient>());
        }

        [Benchmark]
        public Resource TypedElementDeserializerXml()
        {
            return FhirXmlNode.Parse(XmlData).ToPoco<TestPatient>(ModelInspector.ForType<TestPatient>());
        }
    }
}
