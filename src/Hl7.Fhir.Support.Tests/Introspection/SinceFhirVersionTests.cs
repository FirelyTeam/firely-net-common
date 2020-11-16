/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Tests.Serialization
{
    [TestClass]
	public class SinceFhirVersionTests
    {
        [TestMethod]
        public void TestSinceParameterOnAttribute()
        {
            var fea = new FhirElementAttribute("test") { Since = FhirRelease.STU3 };          
            Assert.IsFalse(fea.AppliesToVersion(FhirRelease.DSTU1));
            Assert.IsFalse(fea.AppliesToVersion(FhirRelease.DSTU2));
            Assert.IsTrue(fea.AppliesToVersion(FhirRelease.STU3));
            Assert.IsTrue(fea.AppliesToVersion(FhirRelease.R4));
            Assert.IsTrue(fea.AppliesToVersion((FhirRelease)int.MaxValue));

            fea = new FhirElementAttribute("test2") {  };
            Assert.IsTrue(fea.AppliesToVersion(FhirRelease.DSTU1));
            Assert.IsTrue(fea.AppliesToVersion(FhirRelease.DSTU2));
            Assert.IsTrue(fea.AppliesToVersion(FhirRelease.STU3));
            Assert.IsTrue(fea.AppliesToVersion((FhirRelease)int.MaxValue));

            var fra = new ReferencesAttribute() { Since = FhirRelease.STU3 };
            Assert.IsFalse(fra.AppliesToVersion(FhirRelease.DSTU1));
            Assert.IsFalse(fra.AppliesToVersion(FhirRelease.DSTU2));
            Assert.IsTrue(fra.AppliesToVersion(FhirRelease.STU3)); 
            Assert.IsTrue(fra.AppliesToVersion(FhirRelease.R4));
            Assert.IsTrue(fra.AppliesToVersion((FhirRelease)int.MaxValue));
        }
    }
}
