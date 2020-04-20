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
            IEnumerable<(string value, NamedTypeSpecifier to, bool success, object expected)> tests()
            {
                yield return ("true", TypeSpecifier.Boolean, true, true);
                yield return ("True", TypeSpecifier.Boolean, false, default);
                yield return ("tru", TypeSpecifier.Boolean, false, default);
                yield return ("1", TypeSpecifier.Boolean, false, default);
                yield return ("false", TypeSpecifier.Boolean, true, false);
                yield return ("False", TypeSpecifier.Boolean, false, default);
                yield return ("fal", TypeSpecifier.Boolean, false, default);
                yield return ("0", TypeSpecifier.Boolean, false, default);

                yield return ("2018-01", TypeSpecifier.Date, true, PartialDate.Parse("2018-01"));
                yield return ("hallo", TypeSpecifier.Date, false, default);

                yield return ("2018-01-04T12:00:00Z", TypeSpecifier.DateTime, true, PartialDateTime.Parse("2018-01-04T12:00:00Z"));
                yield return ("hallo", TypeSpecifier.DateTime, false, default);

                yield return ("12:00:00Z", TypeSpecifier.Time, true, PartialTime.Parse("12:00:00Z"));
                yield return ("hallo", TypeSpecifier.Time, false, default);

                yield return ("hallo", TypeSpecifier.String, true, "hallo");

                yield return ("34", TypeSpecifier.Integer, true, 34);
                yield return ("-34", TypeSpecifier.Integer, true, -34);
                yield return ("+34", TypeSpecifier.Integer, true, 34);
                yield return ("34.5", TypeSpecifier.Integer, false, default);

                yield return ("64", TypeSpecifier.Integer64, true, 64L);
                yield return ("-64", TypeSpecifier.Integer64, true, -64L);
                yield return ("+64", TypeSpecifier.Integer64, true, 64L);
                yield return ("64.5", TypeSpecifier.Integer, false, default);


                yield return ("34", TypeSpecifier.Decimal, true, 34m);
                yield return ("0034", TypeSpecifier.Decimal, true, 34m);
                yield return ("34.0", TypeSpecifier.Decimal, true, 34.0m);
                yield return ("-34", TypeSpecifier.Decimal, true, -34m);
                yield return ("3e+4", TypeSpecifier.Decimal, true, 3e+4m);
                yield return ("+34", TypeSpecifier.Decimal, false, default);
                yield return ("34.", TypeSpecifier.Decimal, false, default);

                yield return ("hallo", TypeSpecifier.DateTime, false, default);
            };

            foreach (var (value, to, success, expected) in tests())
            {
                Assert.AreEqual(success, Any.TryParse(value, to, out var parsed), $"While parsing {value} for type {to}");

                if (success)
                    Assert.AreEqual(expected, parsed);
            }
        }
    }
}