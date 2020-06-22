/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

// To introduce the DSTU2 FHIR specification
//extern alias dstu2;

using System;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Utility;
using System.Linq;
using Hl7.Fhir.Serialization;
using System.IO;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Model;
using Hl7.Fhir.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class ElementNodeTests
    {
        [TestMethod]
        public void CreateFromPrimitive()
        {
            var node = ElementNode.ForPrimitive("hi!");
            Assert.AreEqual("hi!", node.Value);
            Assert.AreEqual("System.String", node.InstanceType);   // should really be System.String I think.

            node = ElementNode.ForPrimitive(TestAdministrativeGender.Female);
            Assert.AreEqual("female", node.Value);
            Assert.AreEqual("System.Code", node.InstanceType);

            node = ElementNode.ForPrimitive(AdHoc.Now);
            Assert.AreEqual("Now", node.Value);
            Assert.AreEqual("System.Code", node.InstanceType);
        }

        private enum AdHoc
        {
            Now,
            Spontaneous,
        }

        [FhirEnumeration("AdministrativeGender")]
        private enum TestAdministrativeGender
        {
            /// <summary>
            /// MISSING DESCRIPTION<br/>
            /// (system: http://hl7.org/fhir/administrative-gender)
            /// </summary>
            [EnumLiteral("male", "http://hl7.org/fhir/administrative-gender"), Hl7.Fhir.Utility.Description("Male")]
            Male,
            /// <summary>
            /// MISSING DESCRIPTION<br/>
            /// (system: http://hl7.org/fhir/administrative-gender)
            /// </summary>
            [EnumLiteral("female", "http://hl7.org/fhir/administrative-gender"), Hl7.Fhir.Utility.Description("Female")]
            Female,
        }
    }
}