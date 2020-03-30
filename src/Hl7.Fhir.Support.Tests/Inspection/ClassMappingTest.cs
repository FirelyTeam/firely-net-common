/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Introspection;

namespace Hl7.Fhir.Tests.Introspection
{
    [TestClass]
    public class ClassMappingTest
    {
        [TestMethod]
        public void TestResourceMappingCreation()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way), out var mapping));
            Assert.IsTrue(mapping.IsResource);
            Assert.AreEqual("Way", mapping.Name);
            Assert.AreEqual(typeof(Way), mapping.NativeType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out mapping));
            Assert.IsTrue(mapping.IsResource);
            Assert.AreEqual("Way2", mapping.Name);
            Assert.AreEqual(typeof(Way2), mapping.NativeType);

            Assert.IsFalse(ClassMapping.TryCreate(typeof(Way2), out _, "1.0.0"));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out _, "2.0.0"));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(Way2), out _, "3.0.0"));
        }


        [TestMethod]
        public void TestDatatypeMappingCreation()
        {
            Assert.IsTrue(ClassMapping.TryCreate(typeof(AnimalName), out var mapping, "10.0.0"));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("AnimalName", mapping.Name);
            Assert.AreEqual(typeof(AnimalName), mapping.NativeType);

            Assert.IsTrue(ClassMapping.TryCreate(typeof(NewAnimalName), out mapping));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("AnimalName", mapping.Name);
            Assert.AreEqual(typeof(NewAnimalName), mapping.NativeType);

            Assert.IsFalse(ClassMapping.TryCreate(typeof(ComplexNumber), out mapping, "1.0.0"));
            Assert.IsTrue(ClassMapping.TryCreate(typeof(ComplexNumber), out mapping, "5.0.0"));
            Assert.IsFalse(mapping.IsResource);
            Assert.AreEqual("Complex", mapping.Name);
            Assert.AreEqual(typeof(ComplexNumber), mapping.NativeType);
        }
    }


    /*
     * Resource classes for tests 
     */
    [FhirType("Way")]
    public class Way : Resource
    {
        public override IDeepCopyable DeepCopy() => throw new NotImplementedException(); 
    }

    [FhirType("Way2", Since = "2.0.0")]
    public class Way2 : Resource 
    {
        public override IDeepCopyable DeepCopy() { throw new NotImplementedException(); } 
    }

    /* 
     * Datatype classes for tests
     */
    [FhirType("AnimalName")]
    public class AnimalName  { }

    [FhirType("AnimalName")]
    public class NewAnimalName : AnimalName { }

    [FhirType("Complex", Since="2.0.0")]
    public class ComplexNumber { }
}
