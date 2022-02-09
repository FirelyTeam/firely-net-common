using FluentAssertions;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Specification.Source;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

#nullable enable

namespace Hl7.Fhir.Support.Poco.Tests
{

    [TestClass]
    public class FhirPackageResolverTests
    {
        private const string PACKAGENAME = "TestData/testPackage.tgz";
        //ModelInspector is needed for _resolver, but doesn't do anything in this case, since common doesn't have access to STU3 type information.
        private readonly FhirPackageResolver _resolver = new(new ModelInspector(FhirRelease.STU3), PACKAGENAME);


        [TestMethod]
        public async Task TestResolveByCanonicalUri()
        {
            //check StructureDefinitions
            var pat = await _resolver.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/StructureDefinition/Patient").ConfigureAwait(false);
            pat.Should().NotBeNull();
            pat.Should().Contain("\"url\":\"http://hl7.org/fhir/StructureDefinition/Patient\"");

            //check expansions
            var adm_gender = await _resolver.ResolveByCanonicalUriAsyncAsString("http://hl7.org/fhir/ValueSet/administrative-gender").ConfigureAwait(false);
            adm_gender.Should().NotBeNull();
            adm_gender.Should().Contain("\"url\":\"http://hl7.org/fhir/ValueSet/administrative-gender\"");
        }

        [TestMethod]
        public void TestListFileNames()
        {
            //check StructureDefinitions
            var names = _resolver.ListArtifactNames();
            names.Should().Contain("StructureDefinition-Patient.json");
        }

        [TestMethod]
        public void TestLoadArtifactByName()
        {

            //check StructureDefinitions
            var stream = _resolver.LoadArtifactByName("StructureDefinition-Patient.json");

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream!);
            var artifact = reader.ReadToEnd();

            artifact.Should().StartWith("{\"resourceType\":\"StructureDefinition\",\"id\":\"Patient\"");
        }

        [TestMethod]
        public void TestLoadArtifactByPath()
        {

            //check StructureDefinitions
            var stream = _resolver.LoadArtifactByPath("package/StructureDefinition-Patient.json");

            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream!);
            var artifact = reader.ReadToEnd();

            artifact.Should().StartWith("{\"resourceType\":\"StructureDefinition\",\"id\":\"Patient\"");
        }


        [TestMethod]
        public void TestListResourceUris()
        {
            //check StructureDefinitions
            var names = _resolver.ListResourceUris();
            names.Should().Contain("http://hl7.org/fhir/StructureDefinition/Patient");
            names.Should().Contain("http://hl7.org/fhir/ValueSet/administrative-gender");
        }

        [TestMethod]
        public async Task TestGetCodeSystemByValueSet()
        {
            var cs = await _resolver.FindCodeSystemByValueSetAsString("http://hl7.org/fhir/ValueSet/address-type").ConfigureAwait(false);
            cs.Should().NotBeNull();
            cs.Should().Contain("\"url\":\"http://hl7.org/fhir/address-type\"");
        }

        [TestMethod]
        public async Task TestGetConceptMap()
        {
            var cms = await _resolver.FindConceptMapsAsStrings(sourceUri: "http://hl7.org/fhir/ValueSet/data-absent-reason", targetUri: "http://hl7.org/fhir/ValueSet/v3-NullFlavor").ConfigureAwait(false);
            cms.Should().NotBeEmpty();
            cms.Should().Contain(c => c.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-data-absent-reason-v3\""));
            cms.Should().NotContain(c => c.Contains("\"url\":\"http://hl7.org/fhir/ConceptMap/cm-contact-point-use-v3\""));
        }

        [TestMethod]
        public async Task TestGetNamingSystem()
        {
            var ns = await _resolver.FindNamingSystemByUniqueIdAsString("http://snomed.info/sct").ConfigureAwait(false);
            ns.Should().NotBeNull();
            ns.Should().Contain("\"value\":\"http://snomed.info/sct\"");
            ns.Should().Contain("\"value\":\"2.16.840.1.113883.6.96\"");
        }

        [TestMethod]
        public async Task TestGetArtifactByUri()
        {
            var pat = await _resolver.ResolveByUriAsyncAsString("StructureDefinition/Patient").ConfigureAwait(false);
            pat.Should().NotBeNull();
            pat.Should().Contain("\"url\":\"http://hl7.org/fhir/StructureDefinition/Patient\"");
        }
    }
}
