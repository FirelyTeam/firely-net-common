/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

// To introduce the DSTU2 FHIR specification
// extern alias dstu2;

using Hl7.Fhir.ElementModel;
using Hl7.FhirPath.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using P = Hl7.Fhir.ElementModel.Types;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class ConversionsTests
    {
        [TestMethod]
        public void ConvertToBoolean()
        {
            var areTrue = ElementNode.CreateList(true, "TruE", "Yes", "y", "t", "1", "1.0", 1L, 1m, 1.0m).ToList();
            areTrue.ForEach(o => Assert.IsTrue(o.ToBoolean().Value));
            areTrue.ForEach(o => Assert.IsTrue(o.ConvertsToBoolean()));

            var areFalse = ElementNode.CreateList(false, "fAlse", "nO", "N", "f", "0", "0.0", 0L, 0m, 0.0m).ToList();
            areFalse.ForEach(o => Assert.IsFalse(o.ToBoolean().Value));
            areFalse.ForEach(o => Assert.IsTrue(o.ConvertsToBoolean()));

            var wrong = ElementNode.CreateList("truex", "falsx", "not", "tr", -1L, 2L, 2.0m, 1.1m).ToList();
            wrong.ForEach(o => Assert.IsNull(o.ToBoolean()));
            wrong.ForEach(o => Assert.IsFalse(o.ConvertsToBoolean()));
        }

        [TestMethod]
        public void ConvertToInteger()
        {
            var inputs = ElementNode.CreateList(1, "2", -4, "-5", "+4", true, false);
            var vals = new[] { 1, 2, -4, -5, 4, 1, 0 };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.i.ToInteger(), c.v));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToInteger()));

            var wrong = ElementNode.CreateList("2.4", "++6", "2,6", "no", "false", DateTimeOffset.Now).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToInteger()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToInteger()));
        }


        [TestMethod]
        public void ConvertToDecimal()
        {
            var inputs = ElementNode.CreateList(1L, 2m, "2", "3.14", -4.4m, true, false);
            var vals = new[] { 1m, 2m, 2m, 3.14m, -4.4m, 1m, 0m };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.i.ToDecimal(), c.v));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToDecimal()));

            var wrong = ElementNode.CreateList("hi", "++6", "2,6", "no", "false", DateTimeOffset.Now).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToDecimal()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToDecimal()));
        }

        [TestMethod]
        public void ConvertToQuantity()
        {
            var inputs = ElementNode.CreateList(75L, 75.6m, "30 'wk'", false, true,
                            new P.Quantity(80.0m, "kg"));
            var vals = new[] { new P.Quantity(75m), new P.Quantity(75.6m, P.Quantity.UCUM_UNIT),
                    new P.Quantity(30m,"wk"), new P.Quantity(0.0),
                        new P.Quantity(1.0), new P.Quantity(80m, "kg") };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.v, c.i.ToQuantity()));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToQuantity()));

            var wrong = ElementNode.CreateList("hi", "++6", "2,6", "no", "false",
                DateTimeOffset.Now).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToQuantity()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToQuantity()));
        }


        [TestMethod]
        public void ConvertToDateTime()
        {
            var now = P.DateTime.Parse("2019-01-11T15:47:00+01:00");
            var inputs = ElementNode.CreateList(new DateTimeOffset(2019, 1, 11, 15, 47, 00, new TimeSpan(1, 0, 0)),
                                "2019-01", "2019-01-11T15:47:00+01:00");
            var vals = new[] { now, P.DateTime.Parse("2019-01"), now };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.i.ToDateTime(), c.v));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToDateTime()));

            var wrong = ElementNode.CreateList("hi", 2.6m, false, P.Time.Parse("16:05:49")).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToDateTime()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToDateTime()));
        }


        [TestMethod]
        public void ConvertToTime()
        {
            var now = P.Time.Parse("15:47:00+01:00");
            var inputs = ElementNode.CreateList(now, "12:05:45");
            var vals = new[] { now, P.Time.Parse("12:05:45") };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.i.ToTime(), c.v));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToTime()));

            var wrong = ElementNode.CreateList(new DateTimeOffset(2019, 1, 11, 15, 47, 00, new TimeSpan(1, 0, 0)),
                "hi", 2.6m, false).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToTime()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToTime()));
        }

        [TestMethod]
        public void ConvertToString()
        {
            var inputs = ElementNode.CreateList("hoi", 4L, 3.4m, true, false, P.Time.Parse("15:47:00+01:00"),
                P.DateTime.Parse("2019-01-11T15:47:00+01:00"));
            var vals = new[] { "hoi", "4", "3.4", "true", "false", "15:47:00+01:00", "2019-01-11T15:47:00+01:00" };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.v, c.i.ToString()));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToString()));
        }

        [TestMethod]
        public void CheckTypeDetermination()
        {
            var values = ElementNode.CreateList(1, true, "hi", 4.0m, 4.0f, P.DateTime.Now());

            Test.IsInstanceOfType(values.Item(0).Single().Value, typeof(Int64));
            Test.IsInstanceOfType(values.Item(1).Single().Value, typeof(bool));
            Test.IsInstanceOfType(values.Item(2).Single().Value, typeof(string));
            Test.IsInstanceOfType(values.Item(3).Single().Value, typeof(decimal));
            Test.IsInstanceOfType(values.Item(4).Single().Value, typeof(decimal));
            Test.IsInstanceOfType(values.Item(5).Single().Value, typeof(P.DateTime));
        }


        [TestMethod]
        public void TestItemSelection()
        {
            var values = ElementNode.CreateList(1L, 2, 3L, 4, 5, 6, 7);

            Assert.AreEqual(1L, values.Item(0).Single().Value);
            Assert.AreEqual(2, values.Item(1).Single().Value);
            Assert.AreEqual(3L, values.Item(2).Single().Value);
            Assert.AreEqual(1L, values.First().Value);
            Assert.IsFalse(values.Item(100).Any());
        }

    }
}