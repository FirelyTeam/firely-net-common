using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text.Json;

namespace Hl7.Fhir.Support.Poco.Tests
{
    [TestClass]
    public class SummaryFilterTests
    {
        [TestMethod]
        public void BundleFilter()
        {
            var epf = new ElementPrefixFilter("elem");
            var bf = new BundleFilter(epf);

            var inspector = ModelInspector.ForAssembly(typeof(TestBundle).Assembly);

            bf.EnterObject(null, inspector.FindClassMapping("Bundle"));
            bf.TryEnterMember("bundleElement", null, null).Should().BeTrue();
            bf.TryEnterMember("bundleElement2", null, null).Should().BeTrue();

            bf.EnterObject(null, inspector.FindClassMapping("Organization"));
            epf.EnteredObjects.Should().Be(1);

            bf.TryEnterMember("bundleElement", null, null).Should().BeFalse();
            bf.TryEnterMember("elemX", null, null).Should().BeTrue();
            bf.EnterObject(null, null);
            epf.EnteredObjects.Should().Be(2);
            bf.LeaveObject(null, null);
            epf.EnteredObjects.Should().Be(1);

            bf.TryEnterMember("bundleElement", null, null).Should().BeFalse();
            bf.TryEnterMember("elemX", null, null).Should().BeTrue();
            bf.LeaveMember("elemX", null, null);
            bf.LeaveMember("elemX", null, null);
            bf.LeaveObject(null, inspector.FindClassMapping("Organization"));
            epf.EnteredObjects.Should().Be(0);

            bf.LeaveMember("bundleElement2", null, null);

            bf.TryEnterMember("bundleElement", null, null).Should().BeTrue();
            epf.EnteredObjects.Should().Be(0);

            bf.LeaveMember("bundleElement", null, null);
            bf.LeaveMember("bundleElement", null, null);

            bf.LeaveObject(null, inspector.FindClassMapping("Bundle"));
        }

        [TestMethod]
        public void TopLevelWithChildFilter()
        {
            var tlf = new TopLevelFilter(new ElementPrefixFilter("top"), new ElementPrefixFilter("child"));

            tlf.TryEnterMember("no-top", null, null).Should().BeFalse();
            tlf.TryEnterMember("child", null, null).Should().BeFalse();

            tlf.TryEnterMember("top", null, null).Should().BeTrue();
            tlf.TryEnterMember("top", null, null).Should().BeFalse("top should not pass child filter");
            tlf.TryEnterMember("xop", null, null).Should().BeFalse("xop should not pass child filter");

            tlf.TryEnterMember("child", null, null).Should().BeTrue();
            tlf.TryEnterMember("top", null, null).Should().BeFalse("top should not pass child filter");
            tlf.TryEnterMember("xop", null, null).Should().BeFalse("xop should not pass child filter");

            tlf.TryEnterMember("child2", null, null).Should().BeTrue("child2 should pass child filter");
            tlf.LeaveMember("child2", null, null);

            tlf.TryEnterMember("top", null, null).Should().BeFalse("top should not pass child filter");
            tlf.TryEnterMember("xop", null, null).Should().BeFalse("xop should not pass child filter");

            tlf.LeaveMember("child", null, null);

            tlf.TryEnterMember("top", null, null).Should().BeFalse("top should not pass child filter");
            tlf.TryEnterMember("xop", null, null).Should().BeFalse("xop should not pass child filter");

            tlf.LeaveMember("top", null, null);

            tlf.TryEnterMember("no-top", null, null).Should().BeFalse();
            tlf.TryEnterMember("child", null, null).Should().BeFalse();

            tlf.TryEnterMember("top", null, null).Should().BeTrue();
            tlf.LeaveMember("top", null, null);
        }

        [TestMethod]
        public void TopLevelFilter()
        {
            var tlf = new TopLevelFilter(new ElementPrefixFilter("top"));

            tlf.TryEnterMember("no-top", null, null).Should().BeFalse();

            tlf.TryEnterMember("top", null, null).Should().BeTrue();

            tlf.TryEnterMember("top", null, null).Should().BeTrue("top should pass - no child filter");
            tlf.LeaveMember("top", null, null);

            tlf.TryEnterMember("xop", null, null).Should().BeTrue("xop should not pass - no child filter");
            tlf.LeaveMember("xop", null, null);

            tlf.LeaveMember("top", null, null);

            tlf.TryEnterMember("no-top", null, null).Should().BeFalse();

            tlf.TryEnterMember("top", null, null).Should().BeTrue();
            tlf.LeaveMember("top", null, null);
        }

        [TestMethod]
        public void TopLevelEnterLeave()
        {
            var f1 = new ElementPrefixFilter("top");
            var f2 = new ElementPrefixFilter("child");
            var tlf = new TopLevelFilter(f1, f2);

            tlf.EnterObject(null, null);
            f1.EnteredObjects.Should().Be(1);
            f2.EnteredObjects.Should().Be(0);

            tlf.TryEnterMember("top", null, null).Should().BeTrue();
            tlf.EnterObject(null, null);
            f1.EnteredObjects.Should().Be(1);
            f2.EnteredObjects.Should().Be(1);

            tlf.TryEnterMember("child", null, null).Should().BeTrue();
            tlf.EnterObject(null, null);
            f1.EnteredObjects.Should().Be(1);
            f2.EnteredObjects.Should().Be(2);

            tlf.LeaveObject(null, null);
            f1.EnteredObjects.Should().Be(1);
            f2.EnteredObjects.Should().Be(1);
            tlf.LeaveMember("child", null, null);

            tlf.LeaveObject(null, null);
            f1.EnteredObjects.Should().Be(1);
            f2.EnteredObjects.Should().Be(0);
            tlf.LeaveMember("top", null, null);

            tlf.LeaveObject(null, null);
            f1.EnteredObjects.Should().Be(0);
            f2.EnteredObjects.Should().Be(0);
        }


        [TestMethod]
        public void FilterIntegrationTest()
        {
            // This bundle should get through unfiltered
            TestBundle b = new()
            {
                Identifier = new Identifier("http://nu.nl", "abc"),
                Type = TestBundle.BundleType.Batch,
                Total = 1000
            };

            // This organization will have only its "identifier" pass the filter
            TestPatient p = new()
            {
                Active = true,
                MaritalStatus = new CodeableConcept("http://nu.nl", "123"),
            };

            p.Identifier.Add(new Identifier("http://nu.nl", "abc"));
            p.Communication.Add(new TestPatient.CommunicationComponent { Language = new CodeableConcept("x", "nl-nl"), Preferred = true });

            // This nested bundle also will have only its "identifier" pass the filter
            TestBundle nestedB = new()
            {
                Identifier = new Identifier("http://nu.nl", "abc"),
                Type = TestBundle.BundleType.Collection
            };

            b.Entry.Add(new TestBundle.EntryComponent { Resource = p });
            b.Entry.Add(new TestBundle.EntryComponent { Resource = nestedB });

            var filter = new BundleFilter(new TopLevelFilter(
                new ElementMetadataFilter
                {
                    IncludeNames = new[] { "communication", "type" },
                },
                new ElementMetadataFilter
                {
                    IncludeMandatory = true,
                    IncludeInSummary = true,
                }
                ));
            var options = new JsonSerializerOptions().ForFhir(typeof(TestPatient).Assembly, filter).Pretty();
            string actual = JsonSerializer.Serialize(b, options);

            // Root bundle should not have been filtered at all
            var bp = TypedSerialization.ToPoco<TestBundle>(FhirJsonNode.Parse(actual));
            assertIdentifier(bp.Identifier);
            bp.Type.Value.Should().Be(TestBundle.BundleType.Batch);
            bp.Count().Should().Be(4);

            // The nested Patient should only its "communication" element included
            var pat = bp.Entry[0].Resource as TestPatient;
            pat.Count().Should().Be(1);
            pat.Communication.Should().NotBeNull();
            var communication = pat.Communication.Single();

            // Communication should just have its mandatory "language" set.
            communication.Count().Should().Be(1);

            // Communication.language is a CodeableConcept, all of its field are in summary...
            communication.Language.Should().BeEquivalentTo(new CodeableConcept("x", "nl-nl"));

            // The nested Bundle should only its "type" present
            var nb = bp.Entry[1].Resource as TestBundle;
            nb.Count().Should().Be(1);
            nb.Type.Should().NotBeNull();

            // Non-bundle root resources should be filtered normally too 
            actual = JsonSerializer.Serialize(p, options);
            pat = TypedSerialization.ToPoco<TestPatient>(FhirJsonNode.Parse(actual));
            pat.Count().Should().Be(1);
            pat.Communication.Should().NotBeNull();

            static void assertIdentifier(Identifier ide)
            {
                ide.Should().NotBeNull();
                ide.System.Should().Be("http://nu.nl");
                ide.Value.Should().Be("abc");
                ide.Count().Should().Be(2);
            }
        }


        internal class ElementPrefixFilter : SerializationFilter
        {
            public ElementPrefixFilter(string prefix)
            {
                Prefix = prefix;
            }

            public int EnteredObjects { get; private set; }
            public string Prefix { get; }

            public override void EnterObject(object value, ClassMapping mapping) { EnteredObjects += 1; }
            public override void LeaveMember(string name, object value, PropertyMapping mapping) { }
            public override void LeaveObject(object value, ClassMapping mapping) { EnteredObjects -= 1; }
            public override bool TryEnterMember(string name, object value, PropertyMapping mapping) => name.StartsWith(Prefix);
        }
    }

}
