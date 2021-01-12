/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class ConceptTests
    {
        [TestMethod]
        public void ConceptConstructor()
        {
            var someCodings = new[] { new P.Code("http://system1", "codeA"), new P.Code("http://system2", "codeB") };
            var sameCodings = new[] { new P.Code("http://system1", "codeA"), new P.Code("http://system2", "codeB") };
            var someOtherCodings = new[] { new P.Code("http://system1", "codeB"), new P.Code("http://system2", "codeC") };

            var newCds = new P.Concept(someCodings);

            Assert.AreEqual(newCds, new P.Concept(someCodings));
            Assert.AreEqual(newCds, new P.Concept(sameCodings));
            Assert.AreNotEqual(newCds, new P.Concept(someOtherCodings));
            Assert.AreNotEqual(newCds, new P.Concept(someCodings, "bla"));
        }
    }
}