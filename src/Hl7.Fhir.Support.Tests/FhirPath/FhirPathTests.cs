/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

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
    }
}
