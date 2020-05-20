using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Schema
{

    [TestClass]
    public class TestSerialization
    {
        [TestMethod]
        public void SerializeSchema()
        {
            var sub = new ElementSchema("#sub", new Trace("In a subschema"));

            var main = new ElementSchema("http://root.nl/schema1",
                 new Definitions(sub),
                // new Fail(),
                new ElementSchema("#nested", new Trace("nested")),
                new ReferenceAssertion(sub),
                /*
                 * new SliceAssertion(false,
                    @default: new Trace("this is the default"),
                    new SliceAssertion.Slice("und", new Undecided(), new Trace("I really don't know")),
                    new SliceAssertion.Slice("fail", new Fail(), new Trace("This always fails"))
                    ),
                */
                new Children(
                    ("child1", new ElementSchema(new Trace("in child 1"))),
                    ("child2", new Trace("in child 2")))
                );

            var result = main.ToJson().ToString();
        }

        [TestMethod]
        public async Task ValidateSchema()
        {
            var stringSchema = new ElementSchema("#string",
                new MaxLength("TestSerialization.ValidateSchema", 50)
                );

            var familySchema = new ElementSchema("#myHumanName.family",
                new Assertions(
                    new ReferenceAssertion(stringSchema),
                    new CardinalityAssertion(0, "1", "myHumanName.family"),
                    new MaxLength("TestSerialization.ValidateSchema", 40),
                    new Fixed("TestSerialization.ValidateSchema", "Brown")
                )
            );

            var givenSchema = new ElementSchema("#myHumanName.given",
                new Assertions(
                    new ReferenceAssertion(stringSchema),
                    new CardinalityAssertion(0, "*", "myHumanName.given"),
                    new MaxLength("TestSerialization.ValidateSchema", 40)
                )
            );

            var myHumanNameSchema = new ElementSchema("http://example.com/myHumanNameSchema",
                new Definitions(stringSchema),
                new Children(
                    ("family", familySchema),
                    ("given", givenSchema)
                )
            );

            var humanName = ElementNode.Root("HumanName");
            humanName.Add("family", "Brown", "string");
            humanName.Add("family", "Brown2", "string");
            humanName.Add("given", "Joe", "string");
            humanName.Add("given", "Patrick", "string");

            var result = myHumanNameSchema.ToJson().ToString();

            var vc = new ValidationContext() { ValidateAssertions = new[] { typeof(CardinalityAssertion) } };

            var validationResults = await myHumanNameSchema.Validate(new[] { humanName }, vc);

            var issues = validationResults.OfType<IssueAssertion>();

        }

        [TestMethod]
        public async Task ValidateBloodPressureSchema()
        {
            var bpComponentSchema = new ElementSchema("#bpComponentSchema",
                new Assertions(
                    new CardinalityAssertion(1, "1"),
                    new Children(
                        ("code", new CardinalityAssertion(1, "*")),
                        ("value[x]", new AllAssertion(new CardinalityAssertion(1, "*"), new FhirTypeLabel("Quantity", "TODO")))
                    )
                )
            ); ;

            ElementNode BuildCodeableConcept(string system, string code)
            {
                var coding = ElementNode.Root("Coding");
                coding.Add("system", system, "string");
                coding.Add("code", code, "string");

                var result = ElementNode.Root("CodeableConcept");
                result.Add(coding, "coding");
                return result;
            }

            var systolicSlice = new SliceAssertion.Slice("systolic",
                new AllAssertion(
                    new FhirPathAssertion("path", "code"),
                    new PathSelectorAssertion("code", new Fixed("TODO", BuildCodeableConcept("http://loinc.org", "8480-6")))
                ),
                bpComponentSchema
            );

            var dystolicSlice = new SliceAssertion.Slice("dystolic",
                new AllAssertion(
                    new FhirPathAssertion("path", "code"),
                    new PathSelectorAssertion("code", new Fixed("TODO", BuildCodeableConcept("http://loinc.org", "8462-4")))
                ),
                bpComponentSchema
            );


            var componentSchema = new ElementSchema("#ComponentSlicing",
                new Assertions(
                    new CardinalityAssertion(2, "*"),
                    new SliceAssertion(false, new[] { systolicSlice, dystolicSlice })
                    )
            );

            var bloodPressureSchema = new ElementSchema("http://example.com/bloodPressureSchema",
                new Children(
                    ("status", new CardinalityAssertion(1, "*")),
                    ("component", componentSchema)
                )
            );

            ElementNode buildBpComponent(string system, string code, string value)
            {
                var result = ElementNode.Root("Component");
                result.Add(BuildCodeableConcept(system, code), "code");
                result.Add("value", value, "Quantity");
                return result;
            }



            var bloodPressure = ElementNode.Root("Observation");
            bloodPressure.Add("status", "final", "string");
            bloodPressure.Add(buildBpComponent("http://loinc.org", "8480-6", "120"), "component");
            bloodPressure.Add(buildBpComponent("http://loinc.org", "8462-4", "80"), "component");


            var json = bloodPressureSchema.ToJson().ToString();

            var vc = new ValidationContext();

            var validationResults = await bloodPressureSchema.Validate(new[] { bloodPressure }, vc);


            var issues = validationResults.OfType<IssueAssertion>().Concat(validationResults.Result.Evidence.OfType<IssueAssertion>());

            Assert.IsTrue(validationResults.Result.IsSuccessful);



        }
    }
}
