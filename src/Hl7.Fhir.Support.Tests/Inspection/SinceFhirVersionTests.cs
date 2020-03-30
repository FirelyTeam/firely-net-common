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
            var fea = new FhirElementAttribute("test") { Since = "3.2.0" };          
            Assert.IsFalse(fea.AppliesToVersion("1.0"));
            Assert.IsFalse(fea.AppliesToVersion("3.0.1"));
            Assert.IsTrue(fea.AppliesToVersion("3.2"));
            Assert.IsTrue(fea.AppliesToVersion("3.2.0"));
            Assert.IsTrue(fea.AppliesToVersion("3.2.1"));
            Assert.IsTrue(fea.AppliesToVersion("4.0.2"));
            Assert.IsFalse(fea.AppliesToVersion(""));
            Assert.IsFalse(fea.AppliesToVersion("ewout"));
            Assert.IsTrue(fea.AppliesToVersion(null));

            fea = new FhirElementAttribute("test2") {  };
            Assert.IsTrue(fea.AppliesToVersion("1.0"));
            Assert.IsTrue(fea.AppliesToVersion("3.2.0"));
            Assert.IsTrue(fea.AppliesToVersion("4.0.2"));
            Assert.IsTrue(fea.AppliesToVersion(""));
            Assert.IsTrue(fea.AppliesToVersion("ewout"));
            Assert.IsTrue(fea.AppliesToVersion(null));

            var fra = new ReferencesAttribute() { Since = "4.0.1" };
            Assert.IsFalse(fra.AppliesToVersion("1.0"));
            Assert.IsFalse(fra.AppliesToVersion("3.0.1"));
            Assert.IsFalse(fra.AppliesToVersion("4.0")); 
            Assert.IsTrue(fra.AppliesToVersion("4.0.2"));
            Assert.IsTrue(fra.AppliesToVersion("4.1"));
            Assert.IsFalse(fra.AppliesToVersion(""));
            Assert.IsFalse(fra.AppliesToVersion("ewout"));
            Assert.IsTrue(fra.AppliesToVersion(null));
        }
    }
}
