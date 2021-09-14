/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Hl7.Fhir.Support.Tests
{
    [TestClass]
    public class FhirPathTests
    {
        [TestMethod]
        public void ResolveOnEmptyTest()
        {
            // resolve should handle an empty collection as input
            var symbolTable = new SymbolTable();
            symbolTable.AddStandardFP();
            symbolTable.AddFhirExtensions();
            var compiler = new FhirPathCompiler(symbolTable);
            var evaluator = compiler.Compile("{}.resolve()");

            var result = evaluator(null, FhirEvaluationContext.CreateDefault());

            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void NavigateWithChoiceTypes()
        {
            var xml = "<Parameters xmlns=\"http://hl7.org/fhir\"><parameter><name value=\"item\" /><valueString value=\"test\"/></parameter></Parameters>";
            var sourceNode = FhirXmlNode.Parse(xml);
            var typedElement = sourceNode.ToTypedElement(ModelInspector.ForAssembly(typeof(Resource).Assembly));

            // expression with TypedElement
            typedElement.Predicate("parameter[0].value.exists()").Should().BeTrue();
            typedElement.Predicate("parameter[0].valueString.exists()").Should().BeFalse();
            typedElement.Scalar("parameter[0].value").Should().Be("test");
            typedElement.Scalar("parameter[0].valueString").Should().BeNull();
        }
    }
}
