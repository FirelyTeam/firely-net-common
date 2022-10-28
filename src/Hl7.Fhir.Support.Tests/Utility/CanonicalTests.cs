using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Hl7.Fhir.Utility.Result;

namespace Hl7.Fhir.Utility.Tests
{
    [TestClass]
    public class CanonicalTests
    {
        [TestMethod]
        public void CanonicalTest()
        {
            var r = Ok(4);
            Assert.AreEqual(10, r + 6);
            Canonical c1 = new Canonical("http://example.org/StrucutureDefinition/Condition", null);
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition", c1.Value);
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition", c1.CanonicalUrl);
            Assert.IsNull(c1.CanonicalVersion);
            Assert.IsNull(c1.Fragment);

            Canonical c2 = new Canonical("http://example.org/StrucutureDefinition/Condition", "2022-10");
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition|2022-10", c2.Value);
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition", c2.CanonicalUrl);
            Assert.AreEqual("2022-10", c2.CanonicalVersion);
            Assert.IsNull(c2.Fragment);

            Canonical c3 = new Canonical("http://example.org/StrucutureDefinition/Condition", "2022-10", "code");
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition|2022-10#code", c3.Value);
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition", c3.CanonicalUrl);
            Assert.AreEqual("2022-10", c3.CanonicalVersion);
            Assert.AreEqual("code", c3.Fragment);

            Canonical c4 = new Canonical("http://example.org/StrucutureDefinition/Condition", null, "code");
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition#code", c4.Value);
            Assert.AreEqual("http://example.org/StrucutureDefinition/Condition", c4.CanonicalUrl);
            Assert.IsNull(c4.CanonicalVersion);
            Assert.AreEqual("code", c4.Fragment);

            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition|", null));
            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition#", null));

            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition", "#bla"));
            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition", "|ba"));

            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition", null, "#bla"));
            Assert.ThrowsException<ArgumentException>(() => new Canonical("http://example.org/StrucutureDefinition/Condition", null, "|ba"));
        }
    }
}