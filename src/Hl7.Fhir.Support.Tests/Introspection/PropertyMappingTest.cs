/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using Hl7.Fhir.Introspection;
using System.Linq;
using System.Diagnostics;
using Hl7.Fhir.Specification;

namespace Hl7.Fhir.Tests.Introspection
{
    [TestClass]
	public class VersionAwarePocoProvMembersTest
    {
        [TestMethod]
        public void TestPrimitiveDataTypeMapping()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Base64Binary), out var mapping));
            Assert.AreEqual("base64Binary", mapping.Name);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            var valueProp = mapping.PropertyMappings.SingleOrDefault(pm => pm.RepresentsValueElement);
            Assert.IsNotNull(valueProp);
            Assert.AreEqual("value", valueProp.Name);
            Assert.IsFalse(valueProp.IsCollection);     // don't see byte[] as a collection of byte in FHIR
            Assert.IsTrue(valueProp.RepresentsValueElement);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Code<Resource.ResourceValidationMode>), out mapping));
            Assert.AreEqual("codeOfT<Hl7.Fhir.Model.Resource+ResourceValidationMode>", mapping.Name);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value

            valueProp = mapping.PropertyMappings.SingleOrDefault(pm => pm.RepresentsValueElement);
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection);
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(Resource.ResourceValidationMode),valueProp.NativeType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(FhirUri), out mapping));
            Assert.AreEqual("uri", mapping.Name);
            Assert.AreEqual(3, mapping.PropertyMappings.Count); // id, extension, fhir_comments & value
            valueProp = mapping.PropertyMappings.SingleOrDefault(pm => pm.RepresentsValueElement);
            Assert.IsNotNull(valueProp);
            Assert.IsFalse(valueProp.IsCollection);
            Assert.IsTrue(valueProp.RepresentsValueElement);
            Assert.AreEqual(typeof(string), valueProp.NativeType);
        }
        
        [TestMethod]
        public void TestVersionSpecificMapping()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out var mapping, fhirVersion: 1));
            Assert.IsNull(mapping.FindMappedElementByName("source"));
            var profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(FhirUri), profile.FhirType.Single());

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Meta), out mapping, fhirVersion: 4));
            Assert.IsNotNull(mapping.FindMappedElementByName("source"));
            profile = mapping.FindMappedElementByName("profile");
            Assert.IsNotNull(profile);
            Assert.AreEqual(typeof(Canonical), profile.FhirType.Single());
        }

        [TestMethod]
        public void TestPropsWithRedirect()
        {
            _ = ClassMapping.TryCreate(typeof(TypeWithCodeOfT), out var mapping);

            var propMapping = mapping.FindMappedElementByName("type1");
            Assert.AreEqual(typeof(Code<Resource.ResourceValidationMode>), propMapping.NativeType);
            Assert.AreEqual(typeof(FhirString), propMapping.FhirType.Single());

            propMapping = mapping.FindMappedElementByName("type2");
            Assert.AreEqual(typeof(Code<Resource.ResourceValidationMode>), propMapping.NativeType);
            Assert.AreEqual(typeof(Code), propMapping.FhirType.Single());
        }


        [FhirType("TypeWithCodeOfT")]
        public class TypeWithCodeOfT
        {
            [FhirElement("type1")]
            [TypeRedirect(Type=typeof(FhirString))]
            public Code<Resource.ResourceValidationMode> Type1 { get; set; }

            [FhirElement("type2")]
            public Code<Resource.ResourceValidationMode> Type2 { get; set; }

        }

        [TestMethod]
        public void TestPerformanceOfMapping()
        {
            // just a random list of POCO types available in common
            var typesToTest = new Type[] { typeof(BackboneElement), typeof(BackboneType),
                typeof(Base64Binary), typeof(Canonical), typeof(Element), typeof(FhirString),
                typeof(Extension), typeof(Resource), typeof(Meta), typeof(XHtml) };


            var sw = new Stopwatch();
            sw.Start();
            for(int i = 0; i < 1000; i++)
                foreach (var testee in typesToTest)
                    createMapping(testee);
            sw.Stop();
            Console.WriteLine($"No props: {sw.ElapsedMilliseconds}");

            sw.Restart();
            for (int i = 0; i < 1000; i++)
                foreach (var testee in typesToTest)
                    createMapping(testee,touchProps: true);
            sw.Stop();
            Console.WriteLine($"With props: {sw.ElapsedMilliseconds}");

            int createMapping(Type t, bool touchProps = false)
            {
                ClassMapping.TryCreate(t, out var mapping);
                return touchProps ? mapping.PropertyMappings.Count : -1;
            }
        }
    }
}
