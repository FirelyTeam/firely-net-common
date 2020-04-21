/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Introspection;

namespace Hl7.Fhir.Tests.Serialization
{
    [TestClass]
	public class SinceFhirVersionTests
    {
        [TestMethod]
        public void TestSinceParameterOnAttribute()
        {
            var fea = new FhirElementAttribute("test") { Since = 3 };          
            Assert.IsFalse(fea.AppliesToVersion(1));
            Assert.IsFalse(fea.AppliesToVersion(2));
            Assert.IsTrue(fea.AppliesToVersion(3));
            Assert.IsTrue(fea.AppliesToVersion(4));
            Assert.IsTrue(fea.AppliesToVersion(int.MaxValue));

            fea = new FhirElementAttribute("test2") {  };
            Assert.IsTrue(fea.AppliesToVersion(1));
            Assert.IsTrue(fea.AppliesToVersion(2));
            Assert.IsTrue(fea.AppliesToVersion(3));
            Assert.IsTrue(fea.AppliesToVersion(int.MaxValue));

            var fra = new ReferencesAttribute() { Since = 3 };
            Assert.IsFalse(fra.AppliesToVersion(1));
            Assert.IsFalse(fra.AppliesToVersion(2));
            Assert.IsTrue(fra.AppliesToVersion(3)); 
            Assert.IsTrue(fra.AppliesToVersion(4));
            Assert.IsTrue(fra.AppliesToVersion(int.MaxValue));
        }
    }
}
