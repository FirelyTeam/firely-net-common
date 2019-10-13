#if REWRITTEN
using Hl7.Fhir.Language;
using Hl7.Fhir.Model.System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Hl7.Fhir.Support.Tests.Model
{
    [TestClass]
    public class PrimitiveTests
    {
        [TestMethod]
        public void GetPrimitiveName()
        {
            Dictionary<Type, string> data = new Dictionary<Type, string>
            {
                { typeof(PartialDateTime), "dateTime" },
                { typeof(UInt16), "integer" },
                { typeof(Boolean), "boolean" }
            };

            foreach (var pair in data)
            {
                var native = pair.Key;
                var expected = pair.Value;

                Assert.IsTrue(TypeSpecifier.TryGetPrimitiveTypeName(native, out var actual));
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void CatchesUnknownPrimitiveName()
        {
            Assert.IsFalse(TypeSpecifier.TryGetPrimitiveTypeName(typeof(PrimitiveTests), out _));
        }

        [TestMethod]
        public void ConvertPrimitiveValue()
        {
            var tests = new (object,object)[] { ("a string", "a string"),
                            (new DateTimeOffset(2019, 6, 20, 13, 48, 0, TimeSpan.Zero), PartialDateTime.Parse("2019-06-20T13:48:00Z")),
                            (3u, 3L) };

            foreach (var test in tests)
            {
                Assert.IsTrue(TypeSpecifier.TryConvertToPrimitiveValue(test.Item1, out var actual));
                Assert.AreEqual(test.Item2, actual);
            }
        }


        [TestMethod]
        public void GetNativeRepresentation()
        {
            Dictionary<string, Type> data = new Dictionary<string, Type>
            {
                { "decimal", typeof(decimal) },
                { "url", typeof(string) },
                { "string", typeof(string) },
                { "time", typeof(PartialTime) },
                { "positiveInt", typeof(long) }
            };

            foreach(var pair in data)
            {
                var fhirType = pair.Key;
                var native = pair.Value;

                Assert.IsTrue(TypeSpecifier.TryGetNativeRepresentation(fhirType, out var actual));
                Assert.AreEqual(native, actual);
            }
        }

        [TestMethod]
        public void CatchesUnknownNativeRepresentation()
        {
            Assert.IsFalse(TypeSpecifier.TryGetNativeRepresentation("Patient", out _));
        }

    }
}
#endif