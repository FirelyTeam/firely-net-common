/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model.Primitives;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class PartialTimeTest
    {
        [TestMethod]
        public void TimeConstructor()
        {
            accept("12:34:44.123456+02:00");
            accept("12:34:44.123+02:00");
            accept("12:34:44+02:00");
            accept("12:34:44Z");
            accept("12:34:44+00:00");
            accept("12:34:44");
            accept("12:34Z");
            accept("12:34");
            accept("12");
            accept("12-04:30");
            accept("+05:00");
            accept("Z");

            reject("");
            reject("Hi12:34:44");
            reject("12:34:44there");
            reject("12:34:44+A");
            reject("12:34:44+345:432");
            reject("92:34:44");
            reject("12:34:AM");

            void accept(string testValue)
            {
                Assert.IsTrue(PartialTime.TryParse(testValue, out PartialTime parsed));
                Assert.AreEqual(parsed, PartialTime.Parse(testValue));
                Assert.AreEqual(testValue, parsed.ToString());
            }

            void reject(string testValue)
            {
                Assert.IsFalse(PartialTime.TryParse(testValue, out _));
            }
        }

        [TestMethod]
        public void TimeComparison()
        {
            Assert.IsTrue(PartialDateTime.Parse("2012-03-04T13:00:00Z") > PartialDateTime.Parse("2012-03-04T12:00:00Z"));
            Assert.IsTrue(PartialDateTime.Parse("2012-03-04T13:00:00Z") < PartialDateTime.Parse("2012-03-04T18:00:00+02:00"));

            Assert.IsTrue(PartialTime.Parse("12:34:00+00:00") > PartialTime.Parse("12:33:55+00:00"));
            Assert.IsTrue(PartialTime.Parse("13:00:00+00:00") < PartialTime.Parse("15:01:00+02:00"));
            Assert.IsTrue(PartialTime.Parse("13:00:00+00:00") > PartialTime.Parse("14:59:00+02:00"));
        }

    }
}