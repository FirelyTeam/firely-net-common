/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class ConceptTests
    {
        [TestMethod]
        public void ConceptConstructor()
        {
            var someCodings = new[] { new Code("http://system1", "codeA"), new Code("http://system2", "codeB") };
            var sameCodings = new[] { new Code("http://system1", "codeA"), new Code("http://system2", "codeB") };
            var someOtherCodings = new[] { new Code("http://system1", "codeB"), new Code("http://system2", "codeC") };

            var newCds = new Concept(someCodings);

            Assert.AreEqual(newCds, new Concept(someCodings));
            Assert.AreEqual(newCds, new Concept(sameCodings));
            Assert.AreNotEqual(newCds, new Concept(someOtherCodings));
            Assert.AreNotEqual(newCds, new Concept(someCodings, "bla"));
        }
    }
}