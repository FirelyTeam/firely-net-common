/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

// To introduce the DSTU2 FHIR specification
// extern alias dstu2;

using System;
using System.Linq;
using Hl7.FhirPath.Functions;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantity = Hl7.Fhir.Model.Primitives.Quantity;

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
            var inputs = ElementNode.CreateList(1L, "2", -4L, "-5", "+4", true, false);
            var vals = new[] { 1L, 2L, -4L, -5L, 4L, 1L, 0L };

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
                            new Quantity(80.0m, "kg"));
            var vals = new[] { new Quantity(75m, "1"), new Quantity(75.6m, "1"),
                    new Quantity(30m,"wk"), new Quantity(0.0, "1"),
                        new Quantity(1.0, "1"), new Quantity(80m, "kg") };

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
            var now = PartialDateTime.Parse("2019-01-11T15:47:00+01:00");
            var inputs = ElementNode.CreateList(new DateTimeOffset(2019, 1, 11, 15, 47, 00, new TimeSpan(1, 0, 0)),
                                "2019-01", "2019-01-11T15:47:00+01:00");
            var vals = new[] { now, PartialDateTime.Parse("2019-01"), now };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.i.ToDateTime(), c.v));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToDateTime()));

            var wrong = ElementNode.CreateList("hi", 2.6m, false, PartialTime.Parse("16:05:49")).ToList();
            wrong.ForEach(c => Assert.IsNull(c.ToDateTime()));
            wrong.ForEach(c => Assert.IsFalse(c.ConvertsToDateTime()));
        }


        [TestMethod]
        public void ConvertToTime()
        {
            var now = PartialTime.Parse("15:47:00+01:00");
            var inputs = ElementNode.CreateList(now, "T12:05:45");
            var vals = new[] { now, PartialTime.Parse("12:05:45") };

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
            var inputs = ElementNode.CreateList("hoi", 4L, 3.4m, true, false, PartialTime.Parse("15:47:00+01:00"),
                PartialDateTime.Parse("2019-01-11T15:47:00+01:00"));
            var vals = new[] { "hoi", "4", "3.4", "true", "false", "T15:47:00+01:00", "2019-01-11T15:47:00+01:00" };

            inputs.Zip(vals, (i, v) => (i, v))
                .ToList()
                .ForEach(c => Assert.AreEqual(c.v, c.i.ToString()));
            inputs.ToList().ForEach(c => Assert.IsTrue(c.ConvertsToString()));
        }
    }
}