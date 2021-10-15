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
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Hl7.Fhir.Support.Tests
{
    [TestClass]
    public class FhirPathTests
    {
        private static FhirPathCompiler _compiler;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            var symbolTable = new SymbolTable();
            symbolTable.AddStandardFP();
            symbolTable.AddFhirExtensions();
            _compiler = new FhirPathCompiler(symbolTable);
        }

        [TestMethod]
        public void ResolveOnEmptyTest()
        {
            // resolve should handle an empty collection as input
            var evaluator = _compiler.Compile("{}.resolve()");
            var result = evaluator(null, FhirEvaluationContext.CreateDefault());

            Assert.IsFalse(result.Any());
        }

        [DataTestMethod]
        [DataRow("<div>Not empty</div>", false, "no XHTML namespace")]
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\"> </div>", false, "containing only whitespace")] 
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\">\t\n</div>", false, "containing only whitespace")] 
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\"></div>", false, "empty div element")] 
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\">Not empty</div>", true, "non empty div element with XHTML namespace")]
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\"><img src=\"fhir.gif\" alt=\"Fhir gif\"></img></div>", true, "containing an image element")]
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\"><p><b><i> </i></b></p></div>", false, "no text")]
        [DataRow("<div xmlns=\"http://www.w3.org/1999/xhtml\"><p><b><i> </i></b><img src=\"fhir.gif\" alt=\"Fhir gif\"></img></p></div>", true, "containing an image element")]
        public void HtmlChecks(string xml, bool expected, string because)
        {
            var evaluator = _compiler.Compile("htmlChecks()");
            evaluator.Predicate(ElementNode.ForPrimitive(xml), FhirEvaluationContext.CreateDefault()).Should().Be(expected, because);
        }
    }
}
