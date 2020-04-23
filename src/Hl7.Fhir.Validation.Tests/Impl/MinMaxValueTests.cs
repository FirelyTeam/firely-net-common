using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Validation.Impl;
using Hl7.Fhir.Validation.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Tests.Impl
{
    [TestClass]
    public class MinMaxValueTests// : SimpleAssertionTests
    {
        //public MinMaxValueTests() : base(new MinMaxValue("MinMaxValueTests", null, MinMax.MinValue))
        //{

        //}

        [TestMethod]
        public async Task Foo()
        {
            var validatable = new MinMaxValue("MinMaxValueTests", ElementNode.ForPrimitive(4), MinMax.MinValue);

            var result = await validatable.Validate(ElementNode.ForPrimitive("a string"), new ValidationContext());

            Assert.IsFalse(result.Result.IsSuccessful);

        }

    }
}
