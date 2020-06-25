using Hl7.Fhir.ElementModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Schema.Tests
{
    [TestClass]
    public class ChildrenTests
    {
        [TestMethod]
        public async Task ValidateCorrectNumberOfChildren()
        {
            var assertion = new Children(createTuples(new[] { "child1", "child2" }), false);
            var input = createNode(new[] { "child1", "child2" });

            var result = await assertion.Validate(input, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("child1", getElementAt(result, 0));
            Assert.AreEqual("child2", getElementAt(result, 1));
        }

        [TestMethod]
        public async Task ValidateMoreChildrenThenDefined()
        {
            var assertion = new Children(createTuples(new[] { "child1", "child2" }), false);
            var input = createNode(new[] { "child1", "child2", "child3" });

            var result = await assertion.Validate(input, null);

            Assert.IsFalse(result.Result.IsSuccessful);
            Assert.AreEqual(Issue.CONTENT_ELEMENT_HAS_UNKNOWN_CHILDREN.IssueNumber, getEvidence(result)?.IssueNumber);
            Assert.AreEqual(2, result.OfType<Visited>().Count(), "matched 2 children");
            Assert.AreEqual("child1", getElementAt(result, 0));
            Assert.AreEqual("child2", getElementAt(result, 1));
        }

        [TestMethod]
        public async Task ValidateLessChildrenThenDefined()
        {
            var assertion = new Children(createTuples(new[] { "child1", "child2" }), false);
            var input = createNode(new[] { "child1" });

            var result = await assertion.Validate(input, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("child1", getElementAt(result, 0));

        }

        [TestMethod]
        public async Task ValidateOtherChildrenThenDefined()
        {
            var assertion = new Children(createTuples(new[] { "child1", "child2" }), false);
            var input = createNode(new[] { "childA", "childB" });


            var result = await assertion.Validate(input, null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.OfType<Visited>().Count());
            Assert.IsFalse(result.Result.IsSuccessful);
            Assert.AreEqual(Issue.CONTENT_ELEMENT_HAS_UNKNOWN_CHILDREN.IssueNumber, getEvidence(result)?.IssueNumber);
        }

        [TestMethod]
        public async Task ValidateEmptyChildren()
        {
            var assertion = new Children(false);
            var input = createNode(new[] { "child1", "child2" });

            var result = await assertion.Validate(input, null);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.OfType<Visited>().Any());
            Assert.IsFalse(result.Result.IsSuccessful);
            Assert.AreEqual(Issue.CONTENT_ELEMENT_HAS_UNKNOWN_CHILDREN.IssueNumber, getEvidence(result)?.IssueNumber);
        }

        [TestMethod]
        public async Task ValidateNoChildrenThenDefined()
        {
            var assertion = new Children(createTuples(new[] { "child1", "child2" }), false);
            var input = createNode(new string[] { });

            var result = await assertion.Validate(input, null);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.OfType<Visited>().Any());
            Assert.IsFalse(result.Result.IsSuccessful);
            Assert.AreEqual(Issue.CONTENT_ELEMENT_MUST_HAVE_VALUE_OR_CHILDREN.IssueNumber, getEvidence(result)?.IssueNumber);
        }

        [TestMethod]
        public void MergeTest()
        {
            var assertion1 = new Children(createTuples(new[] { "child1", "child2" }), false);
            var assertion2 = new Children(createTuples(new[] { "child3", "child1" }), false);

            var result = assertion1.Merge(assertion2) as Children;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.ChildList.Count);
            Assert.AreEqual(3, result.ChildList.Keys.Intersect(new[] { "child1", "child2", "child3" }).Count());
        }

        private static IssueAssertion getEvidence(Assertions assertions)
            => assertions.Result.Evidence.OfType<IssueAssertion>().FirstOrDefault();

        private static ElementNode createNode(string[] childNames)
        {
            var result = ElementNode.Root("root");
            foreach (var childName in childNames)
            {
                result.Add(childName);
            }
            return result;
        }

        private static string getElementAt(Assertions assertions, int index)
            => assertions[index] is Visited v ? v.ChildName : null;

        private static IReadOnlyDictionary<string, IAssertion> createTuples(string[] childNames)
        {
            var result = new Dictionary<string, IAssertion>();
            foreach (var childName in childNames)
            {
                result.Add(childName, new VisitorAssertion(childName));
            }
            return result;
        }
    }


    class VisitorAssertion : IValidatable
    {
        private readonly IAssertion _visited;

        public VisitorAssertion(string childName)
        {
            _visited = new Visited { ChildName = childName };
        }

        public Task<Assertions> Validate(ITypedElement input, ValidationContext vc)
        {
            return Task.FromResult(new Assertions(_visited));
        }

        public JToken ToJson()
        {
            throw new System.NotImplementedException();
        }
    }

    class Visited : IAssertion
    {
        public JToken ToJson()
        {
            throw new System.NotImplementedException();
        }

        public string ChildName { get; set; }
    }
}