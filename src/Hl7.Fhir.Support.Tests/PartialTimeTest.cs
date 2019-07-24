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
            accept("12:34:44.123456+02:00",12,34,44,123, new TimeSpan(2,0,0));
            accept("12:34:44.1+02:00", 12, 34, 44, 100, new TimeSpan(2, 0, 0));
            accept("12:34:44+02:00",12,34,44,null,new TimeSpan(2, 0, 0));
            accept("12:34:44Z",12,34,44,null,TimeSpan.Zero);
            accept("12:34:44+00:00",12,34,44,null,TimeSpan.Zero);
            accept("12:34:44",12,34,44,null,null);
            
            accept("12:34Z",12,34,null,null,TimeSpan.Zero);
            accept("12:34",12,34,null,null,null);
            accept("12", 12, null, null, null, null);
            accept("12-04:30", 12, null,null,null, new TimeSpan(-4,-30,0));
            accept("+05:00",null,null,null,null, new TimeSpan(5,0,0));
            accept("Z", null, null, null, null, TimeSpan.Zero);

            reject("");
            reject("12:34.1234");
            reject("Hi12:34:44");
            reject("12:34:44there");
            reject("12:34:44+A");
            reject("12:34:44+345:432");
            reject("92:34:44");
            reject("12:34:AM");

            void accept(string testValue, int? h, int? m, int? s, int? ms, TimeSpan? o )
            {
                Assert.IsTrue(PartialTime.TryParse(testValue, out PartialTime parsed), "TryParse");
                Assert.AreEqual(h, parsed.Hours, "hours");
                Assert.AreEqual(m, parsed.Minutes, "minutes");
                Assert.AreEqual(s, parsed.Seconds, "seconds");
                Assert.AreEqual(ms, parsed.Millis, "millis");
                Assert.AreEqual(o, parsed.Offset, "offset");
                Assert.AreEqual(testValue, parsed.ToString(), "ToString");
            }

            void reject(string testValue)
            {
                Assert.IsFalse(PartialTime.TryParse(testValue, out _));
            }
        }

        [TestMethod]
        public void ToDateTimeOffset()
        {
            Assert.Fail();
            //var pt = PartialTime.Parse("13:45:56");
            //var dto = pt.ToDateTimeOffset(2019, 7, 23);
            //Assert.Are
            //pt = PartialTime.Parse("13:45:56+01:00");
            //what to do with timezone????
            //pass as a parameter ('defaultTimeZone')
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

        [TestMethod]
        public void TimeEquality()
        {
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01") == PartialDateTime.Parse("2015-01-01"));
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01") != PartialDateTime.Parse("2015-01"));
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01T13:40:50+02:00") == PartialDateTime.Parse("2015-01-01T13:40:50+02:00"));
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01T13:40:50+00:00") == PartialDateTime.Parse("2015-01-01T13:40:50Z"));
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01T13:40:50+00:10") != PartialDateTime.Parse("2015-01-01T13:40:50Z"));
            Assert.IsTrue(PartialDateTime.Parse("2015-01-01T13:40:50+00:10") != PartialDateTime.Parse("2015-01-01"));

            Assert.IsTrue(PartialTime.Parse("13:45:02Z") == PartialTime.Parse("13:45:02+00:00"));
            Assert.IsTrue(PartialTime.Parse("13:45:02+01:00") == PartialTime.Parse("13:45:02+01:00"));
            Assert.IsTrue(PartialTime.Parse("13:45:02+00:00") != PartialTime.Parse("13:45:02+01:00"));
        }

        [TestMethod]
        public void CheckOrdering()
        {
            Assert.AreEqual(1, PartialDateTime.Parse("2012-03-04T13:00:00Z").CompareTo(PartialDateTime.Parse("2012-03-04T12:00:00Z")));
            Assert.AreEqual(-1, PartialDateTime.Parse("2012-03-04T13:00:00Z").CompareTo(PartialDateTime.Parse("2012-03-04T18:00:00+02:00")));
            Assert.AreEqual(0, PartialDateTime.Parse("2015-01-01").CompareTo(PartialDateTime.Parse("2015-01-01")));

            Assert.AreEqual(1, PartialTime.Parse("12:34:00+00:00").CompareTo(PartialTime.Parse("12:33:55+00:00")));
            Assert.AreEqual(-1, PartialTime.Parse("13:00:00+00:00").CompareTo(PartialTime.Parse("15:01:00+02:00")));
            Assert.AreEqual(0, PartialTime.Parse("13:45:02+01:00").CompareTo(PartialTime.Parse("13:45:02+01:00")));
        }


    }
}