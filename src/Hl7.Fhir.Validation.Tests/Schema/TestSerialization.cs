using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
        public async void ValidateSchema()
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
    }
}
