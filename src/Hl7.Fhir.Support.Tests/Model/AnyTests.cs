/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

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
            IEnumerable<(string value, System.Type to, bool success, object expected)> tests()
            {
                yield return ("true", typeof(Boolean), true, true);
                yield return ("True", typeof(Boolean), false, default);
                yield return ("tru", typeof(Boolean), false, default);
                yield return ("1", typeof(Boolean), false, default);
                yield return ("false", typeof(Boolean), true, false);
                yield return ("False", typeof(Boolean), false, default);
                yield return ("fal", typeof(Boolean), false, default);
                yield return ("0", typeof(Boolean), false, default);

                yield return ("2018-01", typeof(PartialDate), true, PartialDate.Parse("2018-01"));
                yield return ("hallo", typeof(PartialDate), false, default);

                yield return ("2018-01-04T12:00:00Z", typeof(PartialDateTime), true, PartialDateTime.Parse("2018-01-04T12:00:00Z"));
                yield return ("hallo", typeof(PartialDateTime), false, default);

                yield return ("12:00:00Z", typeof(PartialTime), true, PartialTime.Parse("12:00:00Z"));
                yield return ("hallo", typeof(PartialTime), false, default);

                yield return ("hallo", typeof(String), true, "hallo");

                yield return ("34", typeof(Integer), true, 34);
                yield return ("-34", typeof(Integer), true, -34);
                yield return ("+34", typeof(Integer), true, 34);
                yield return ("34.5", typeof(Integer), false, default);

                yield return ("64", typeof(Long), true, 64L);
                yield return ("-64", typeof(Long), true, -64L);
                yield return ("+64", typeof(Long), true, 64L);
                yield return ("64.5", typeof(Integer), false, default);


                yield return ("34", typeof(Decimal), true, 34m);
                yield return ("0034", typeof(Decimal), true, 34m);
                yield return ("34.0", typeof(Decimal), true, 34.0m);
                yield return ("-34", typeof(Decimal), true, -34m);
                yield return ("3e+4", typeof(Decimal), true, 3e+4m);
                yield return ("+34", typeof(Decimal), false, default);
                yield return ("34.", typeof(Decimal), false, default);

                yield return ("hallo", typeof(PartialDateTime), false, default);
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