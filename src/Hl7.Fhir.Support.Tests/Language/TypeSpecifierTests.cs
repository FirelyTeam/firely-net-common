using Hl7.Fhir.Language;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hl7.Fhir.Support.Tests.Language
{
    [TestClass]
    public class TypeSpecifierTests
    {
        [TestMethod]
        public void TestConstruction()
        {
            var sysT = NamedTypeSpecifier.GetByName("Concept");
            Assert.AreEqual("System", sysT.Namespace);
            Assert.AreEqual("Concept", sysT.Name);
            Assert.IsTrue(Object.ReferenceEquals(sysT, TypeSpecifier.Concept));

            sysT = NamedTypeSpecifier.GetByName("AsYetUnknown");
            Assert.AreEqual("System", sysT.Namespace);
            Assert.AreEqual("AsYetUnknown", sysT.Name);

            var cusT = NamedTypeSpecifier.GetByName("MySpace", "MyType");
            Assert.AreEqual("MySpace", cusT.Namespace);
            Assert.AreEqual("MyType", cusT.Name);
        }

        [TestMethod]
        public void TestEquality()
        {
            Assert.AreEqual(TypeSpecifier.Concept, TypeSpecifier.Concept);
            Assert.AreNotEqual(TypeSpecifier.Concept, TypeSpecifier.Code);
#pragma warning disable CS1718 // Comparison made to same variable - we're testing the '==' operator here
            Assert.IsTrue(TypeSpecifier.Concept == TypeSpecifier.Concept);
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.IsTrue(TypeSpecifier.Concept != TypeSpecifier.Code);
            Assert.AreEqual(TypeSpecifier.Concept, NamedTypeSpecifier.GetByName("Concept"));
            Assert.AreEqual(TypeSpecifier.Concept, NamedTypeSpecifier.GetByName("System","Concept"));

            Assert.AreNotEqual(TypeSpecifier.Concept, NamedTypeSpecifier.GetByName("System", "concept"));
            Assert.AreNotEqual(NamedTypeSpecifier.GetByName("System", "Concept"), NamedTypeSpecifier.GetByName("System", "concept"));
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("System.Concept", TypeSpecifier.Concept.ToString());
            Assert.AreEqual("`dot.net`.String", NamedTypeSpecifier.GetByName("dot.net", "String").ToString());
            Assert.AreEqual("DotNet.`System.Guid`", NamedTypeSpecifier.GetByName("DotNet", "System.Guid").ToString());
            Assert.AreEqual(@"`dot.\`net\``.String", NamedTypeSpecifier.GetByName("dot.`net`", "String").ToString());
            Assert.AreEqual(@"`dot\`net`.String", NamedTypeSpecifier.GetByName("dot`net", "String").ToString());
        }

        [TestMethod]
        public void TestForNativeType()
        {
            Assert.AreEqual(TypeSpecifier.Boolean,NamedTypeSpecifier.ForNativeType(typeof(bool)));
            Assert.AreEqual(TypeSpecifier.DateTime, NamedTypeSpecifier.ForNativeType(typeof(PartialDateTime)));
            Assert.AreEqual(TypeSpecifier.Concept, NamedTypeSpecifier.ForNativeType(typeof(Concept)));
            Assert.AreEqual(TypeSpecifier.Any, NamedTypeSpecifier.ForNativeType(typeof(object)));
            Assert.AreEqual(NamedTypeSpecifier.GetByName("DotNet", "System.Guid"), NamedTypeSpecifier.ForNativeType(typeof(Guid)));
            Assert.AreEqual(NamedTypeSpecifier.GetByName("DotNet", "System.Collections.Generic.IEnumerable`1[System.Guid]"), 
                NamedTypeSpecifier.ForNativeType(typeof(IEnumerable<Guid>)));
        }

        [TestMethod]
        public void TestGetNativeType()
        {
            Assert.AreEqual(typeof(string), TypeSpecifier.String.GetNativeType());
            Assert.AreEqual(typeof(Quantity), TypeSpecifier.Quantity.GetNativeType());
            Assert.AreEqual(typeof(object), TypeSpecifier.Any.GetNativeType());
            Assert.AreEqual(typeof(PartialTime), TypeSpecifier.Time.GetNativeType());
            Assert.AreEqual(typeof(Guid), NamedTypeSpecifier.ForNativeType(typeof(Guid)).GetNativeType());

#if !NET40
            Assert.ThrowsException<NotSupportedException>(() => NamedTypeSpecifier.GetByName("DotNet", "NoSuchType").GetNativeType());
            Assert.ThrowsException<NotSupportedException>(() => NamedTypeSpecifier.GetByName("Internal", "NoSuchType").GetNativeType());
#endif
        }
    }
}
