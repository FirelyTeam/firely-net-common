using System;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class PartialDateTest
    {
        [TestMethod]
        public void DateConstructor()
        {
            accept("2010", 2010, null, null, PartialPrecision.Year, null);
            accept("2010-12", 2010, 12, null, PartialPrecision.Month, null);
            accept("2010-08-12", 2010, 08, 12, PartialPrecision.Day, null);
            accept("2010-08-12+02:00", 2010, 08, 12, PartialPrecision.Day, new TimeSpan(2, 0, 0));
            accept("2010-08-12+00:00", 2010, 08, 12, PartialPrecision.Day, TimeSpan.Zero);

            reject("");
            reject("+02:00");
            reject("12-10");
            reject("12");
            reject("Test2010-08-12");
            reject("2010-08-12Test");
            reject("2010-2-2");
            reject("2010-02-4");
            reject("2010-2-04");
        }

        void accept(string testInput, int? y, int? m, int? d, PartialPrecision? p, TimeSpan? o)
        {
            Assert.IsTrue(PartialDate.TryParse(testInput, out PartialDate parsed), "TryParse");
            Assert.AreEqual(y, parsed.Years, "years");
            Assert.AreEqual(m, parsed.Months, "months");
            Assert.AreEqual(d, parsed.Days, "days");
            Assert.AreEqual(o, parsed.Offset, "offset");
            Assert.AreEqual(p, parsed.Precision, "precision");
            Assert.AreEqual(testInput, parsed.ToString(), "ToString");
        }

        void reject(string testValue)
        {
            Assert.IsFalse(PartialDate.TryParse(testValue, out _));
        }

        [TestMethod]
        public void GetToday()
        {
            var today = PartialDate.Today();
            var today2 = DateTimeOffset.Now;   // just don't run this unit test a split second before midnight

            Assert.AreEqual(today2.Year, today.Years);
            Assert.AreEqual(today2.Month, today.Months);
            Assert.AreEqual(today2.Day, today.Days);
            Assert.AreEqual(PartialPrecision.Day, today.Precision);
            Assert.IsFalse(today.HasOffset);

            today = PartialDate.Today(includeOffset: true);
            Assert.IsTrue(today.HasOffset);
        }

        [TestMethod]
        public void ToDateTimeOffset()
        {
            var plusOne = new TimeSpan(1, 0, 0);
            var plusTwo = new TimeSpan(2, 0, 0);

            var partialDate = PartialDate.Parse("2010-06-04");
            var dateTimeOffset = partialDate.ToDateTimeOffset(12, 3, 4, 5, plusOne);
            Assert.AreEqual(2010, dateTimeOffset.Year);
            Assert.AreEqual(06, dateTimeOffset.Month);
            Assert.AreEqual(04, dateTimeOffset.Day);
            Assert.AreEqual(12, dateTimeOffset.Hour);
            Assert.AreEqual(3, dateTimeOffset.Minute);
            Assert.AreEqual(4, dateTimeOffset.Second);
            Assert.AreEqual(5, dateTimeOffset.Millisecond);
            Assert.AreEqual(plusOne, dateTimeOffset.Offset);

            partialDate = PartialDate.Parse("2010-06-04+02:00");
            dateTimeOffset = partialDate.ToDateTimeOffset(12, 3, 4, 5, plusOne);
            Assert.AreEqual(2010, dateTimeOffset.Year);
            Assert.AreEqual(06, dateTimeOffset.Month);
            Assert.AreEqual(04, dateTimeOffset.Day);
            Assert.AreEqual(12, dateTimeOffset.Hour);
            Assert.AreEqual(3, dateTimeOffset.Minute);
            Assert.AreEqual(4, dateTimeOffset.Second);
            Assert.AreEqual(5, dateTimeOffset.Millisecond);
            Assert.AreEqual(plusTwo, dateTimeOffset.Offset);

            partialDate = PartialDate.Parse("2010-06");
            dateTimeOffset = partialDate.ToDateTimeOffset(12, 3, 4, 5, plusOne);
            Assert.AreEqual(2010, dateTimeOffset.Year);
            Assert.AreEqual(06, dateTimeOffset.Month);
            Assert.AreEqual(1, dateTimeOffset.Day);
            Assert.AreEqual(12, dateTimeOffset.Hour);
            Assert.AreEqual(3, dateTimeOffset.Minute);
            Assert.AreEqual(4, dateTimeOffset.Second);
            Assert.AreEqual(5, dateTimeOffset.Millisecond);
            Assert.AreEqual(plusOne, dateTimeOffset.Offset);

            partialDate = PartialDate.Parse("2010");
            dateTimeOffset = partialDate.ToDateTimeOffset(12, 3, 4, 5, plusOne);
            Assert.AreEqual(2010, dateTimeOffset.Year);
            Assert.AreEqual(1, dateTimeOffset.Month);
            Assert.AreEqual(1, dateTimeOffset.Day);
            Assert.AreEqual(12, dateTimeOffset.Hour);
            Assert.AreEqual(3, dateTimeOffset.Minute);
            Assert.AreEqual(4, dateTimeOffset.Second);
            Assert.AreEqual(5, dateTimeOffset.Millisecond);
            Assert.AreEqual(plusOne, dateTimeOffset.Offset);
        }

        [TestMethod]
        public void FromDateTimeOffset()
        {
            var plusOne = new TimeSpan(1, 0, 0);

            var dateTimeOffset = new DateTimeOffset(2019, 7, 23, 13, 45, 56, 567, plusOne);
            var partialDate = PartialDate.FromDateTimeOffset(dateTimeOffset);
            Assert.AreEqual(2019, partialDate.Years);
            Assert.AreEqual(7, partialDate.Months);
            Assert.AreEqual(23, partialDate.Days);
            Assert.IsNull(partialDate.Offset);

            partialDate = PartialDate.FromDateTimeOffset(dateTimeOffset, includeOffset: true);
            Assert.AreEqual(2019, partialDate.Years);
            Assert.AreEqual(7, partialDate.Months);
            Assert.AreEqual(23, partialDate.Days);
            Assert.AreEqual(plusOne, partialDate.Offset);

            partialDate = PartialDate.FromDateTimeOffset(dateTimeOffset, prec: PartialPrecision.Year, includeOffset: true);
            Assert.AreEqual(2019, partialDate.Years);
            Assert.IsNull(partialDate.Months);
            Assert.AreEqual(PartialPrecision.Year, partialDate.Precision);
            Assert.AreEqual(plusOne, partialDate.Offset);
        }

    }
}