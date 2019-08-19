/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Language;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class AnyTests
    {
        [TestMethod]
        public void AnyParse()
        {
            IEnumerable<(string value, TypeSpecifier to, bool success, object expected)> tests()
            {
                yield return ("true", TypeSpecifier.System.Boolean, true, true);
                yield return ("True", TypeSpecifier.System.Boolean, false, default);
                yield return ("tru", TypeSpecifier.System.Boolean, false, default);
                yield return ("1", TypeSpecifier.System.Boolean, false, default);
                yield return ("false", TypeSpecifier.System.Boolean, true, false);
                yield return ("False", TypeSpecifier.System.Boolean, false, default);
                yield return ("fal", TypeSpecifier.System.Boolean, false, default);
                yield return ("0", TypeSpecifier.System.Boolean, false, default);

                yield return ("2018-01", TypeSpecifier.System.Date, true, PartialDate.Parse("2018-01"));
                yield return ("hallo", TypeSpecifier.System.Date, false, default);

                yield return ("2018-01-04T12:00:00Z", TypeSpecifier.System.DateTime, true, PartialDateTime.Parse("2018-01-04T12:00:00Z"));
                yield return ("hallo", TypeSpecifier.System.DateTime, false, default);

                yield return ("12:00:00Z", TypeSpecifier.System.Time, true, PartialTime.Parse("12:00:00Z"));
                yield return ("hallo", TypeSpecifier.System.Time, false, default);

                yield return ("hallo", TypeSpecifier.System.String, true, "hallo");

                yield return ("34", TypeSpecifier.System.Integer, true, 34L);
                yield return ("-34", TypeSpecifier.System.Integer, true, -34L);
                yield return ("+34", TypeSpecifier.System.Integer, true, 34L);
                yield return ("34.5", TypeSpecifier.System.Integer, false, default);

                yield return ("34", TypeSpecifier.System.Decimal, true, 34m);
                yield return ("0034", TypeSpecifier.System.Decimal, true, 34m);
                yield return ("34.0", TypeSpecifier.System.Decimal, true, 34.0m);
                yield return ("-34", TypeSpecifier.System.Decimal, true, -34m);
                yield return ("3e+4", TypeSpecifier.System.Decimal, true, 3e+4m);
                yield return ("+34", TypeSpecifier.System.Decimal, false, default);
                yield return ("34.", TypeSpecifier.System.Decimal, false, default);

                yield return ("hallo", TypeSpecifier.System.DateTime, false, default);
            };

            foreach (var test in tests())
            {
                Assert.AreEqual(test.success, Any.TryParse(test.value, test.to, out var parsed), $"While parsing {test.value} for type {test.to}");

                if(test.success)
                    Assert.AreEqual(test.expected, parsed);
            }
        }
    }
}