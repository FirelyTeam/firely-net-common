/* 
 * Copyright (c) 2015, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using P = Hl7.Fhir.ElementModel.Types;

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
                yield return ("true", typeof(P.Boolean), true, true);
                yield return ("True", typeof(P.Boolean), false, default);
                yield return ("tru", typeof(P.Boolean), false, default);
                yield return ("1", typeof(P.Boolean), false, default);
                yield return ("false", typeof(P.Boolean), true, false);
                yield return ("False", typeof(P.Boolean), false, default);
                yield return ("fal", typeof(P.Boolean), false, default);
                yield return ("0", typeof(P.Boolean), false, default);

                yield return ("2018-01", typeof(P.PartialDate), true, P.PartialDate.Parse("2018-01"));
                yield return ("hallo", typeof(P.PartialDate), false, default);

                yield return ("2018-01-04T12:00:00Z", typeof(P.PartialDateTime), true, P.PartialDateTime.Parse("2018-01-04T12:00:00Z"));
                yield return ("hallo", typeof(P.PartialDateTime), false, default);

                yield return ("12:00:00Z", typeof(P.PartialTime), true, P.PartialTime.Parse("12:00:00Z"));
                yield return ("hallo", typeof(P.PartialTime), false, default);

                yield return ("hallo", typeof(P.String), true, "hallo");

                yield return ("34", typeof(P.Integer), true, 34);
                yield return ("-34", typeof(P.Integer), true, -34);
                yield return ("+34", typeof(P.Integer), true, 34);
                yield return ("34.5", typeof(P.Integer), false, default);

                yield return ("64", typeof(P.Long), true, 64L);
                yield return ("-64", typeof(P.Long), true, -64L);
                yield return ("+64", typeof(P.Long), true, 64L);
                yield return ("64.5", typeof(P.Integer), false, default);


                yield return ("34", typeof(P.Decimal), true, 34m);
                yield return ("0034", typeof(P.Decimal), true, 34m);
                yield return ("34.0", typeof(P.Decimal), true, 34.0m);
                yield return ("-34", typeof(P.Decimal), true, -34m);
                yield return ("3e+4", typeof(P.Decimal), true, 3e+4m);
                yield return ("+34", typeof(P.Decimal), false, default);
                yield return ("34.", typeof(P.Decimal), false, default);

                yield return ("hallo", typeof(P.PartialDateTime), false, default);
            };

            foreach (var (value, to, success, expected) in tests())
            {
               // var anyExpected = Any.ConvertToAny(expected);

                Assert.AreEqual(success, P.Any.TryParse(value, to, out var parsed), $"While parsing {value} for type {to}");

                if (success)
                    Assert.AreEqual(expected, parsed);
            }
        }
    }
}