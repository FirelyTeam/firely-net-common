using Hl7.Fhir.Language;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Hl7.Fhir.Support.Tests.Language
{
    [TestClass]
    public class CastTests
    {
        [TestMethod]
        public void TestCast()
        {
            // try Integer -> List<Decimal?>
            var from = TypeSpecifier.Integer;
            var to = new ListTypeSpecifier(new OptionTypeSpecifier(TypeSpecifier.Decimal));

            Console.WriteLine($"Casting '{from}' to '{to}'");

            var result = CastCollection.AllCasts.TryCast(null, from, to);

        }

        [TestMethod]
        public void TestCast2()
        {
            // try List<Integer> -> List<Any>
            var from = new ListTypeSpecifier(TypeSpecifier.Integer);
            var to = new ListTypeSpecifier(TypeSpecifier.Any);

            Console.WriteLine($"Casting '{from}' to '{to}'");

            var result = CastCollection.AllCasts.TryCast(null, from, to);

        }

    }
}
