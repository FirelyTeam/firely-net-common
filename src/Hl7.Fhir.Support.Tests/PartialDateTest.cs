using System;
using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.FhirPath.Tests
{
    [TestClass]
    public class PartialDateTest
    {
        [TestMethod]
        public void DateConstructor() {
            accept("2010", 2010, null, null, PartialPrecision.Year, null);
            accept("2010-12", 2010, 12, null, PartialPrecision.Month, null);
            accept("2010-08-12", 2010, 08, 12, PartialPrecision.Day, null);
            accept("2010-08-12+02:00", 2010, 08, 12, PartialPrecision.Day, new TimeSpan(2,0,0));
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

        void accept(string testInput, int? y, int? m, int? d, PartialPrecision? p, TimeSpan? o) {
            Assert.IsTrue(PartialDate.TryParse(testInput, out PartialDate parsed), "TryParse");
            Assert.AreEqual(y, parsed.Year, "years");
            Assert.AreEqual(m, parsed.Month, "months");
            Assert.AreEqual(d, parsed.Day, "days");
            Assert.AreEqual(o, parsed.Offset, "offset");
            Assert.AreEqual(p, parsed.Precision, "precision");
            Assert.AreEqual(testInput, parsed.ToString(), "ToString");
        }

        void reject(string testValue)
        {
            Assert.IsFalse(PartialDate.TryParse(testValue, out _));
        }

        [TestMethod]
        public void ToDateTimeOffset() {
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
        public void FromDateTimeOffset() {
            var plusOne = new TimeSpan(1, 0, 0);

            var dateTimeOffset = new DateTimeOffset(2019, 7, 23, 13, 45, 56, 567, plusOne);
            var partialDate = PartialDate.FromDateTimeOffset(dateTimeOffset);
            Assert.AreEqual(2019, partialDate.Year);
            Assert.AreEqual(7, partialDate.Month);
            Assert.AreEqual(23, partialDate.Day);
            Assert.AreEqual(plusOne, partialDate.Offset);
        }

        [TestMethod]
        public void DateComparison()
        {
            Assert.IsTrue(PartialDate.Parse("2010-06-03") > PartialDate.Parse("2010-05-03"));
            Assert.IsTrue(PartialDate.Parse("2010-06-03") < PartialDate.Parse("2010-07-03"));
            Assert.IsTrue(PartialDate.Parse("2010-12-05") > PartialDate.Parse("2010-12-04"));
            Assert.IsTrue(PartialDate.Parse("2010-12-03") < PartialDate.Parse("2010-12-04"));
            Assert.IsTrue(PartialDate.Parse("2011") > PartialDate.Parse("2010"));
            Assert.IsTrue(PartialDate.Parse("2011") < PartialDate.Parse("2013"));
            Assert.IsTrue(PartialDate.Parse("2011-03") > PartialDate.Parse("2011-02"));
            Assert.IsTrue(PartialDate.Parse("2011-03") < PartialDate.Parse("2011-04"));
            Assert.IsTrue(PartialDate.Parse("2010-12-03+02:00") < PartialDate.Parse("2010-12-04+02:00"));
            Assert.IsTrue(PartialDate.Parse("2010-12-05+02:00") > PartialDate.Parse("2010-12-04+02:00"));
            Assert.IsTrue(PartialDate.Parse("2010-12-03+06:00") < PartialDate.Parse("2010-12-04+02:00"));
            Assert.IsTrue(PartialDate.Parse("2010-12-05+08:00") > PartialDate.Parse("2010-12-04+02:00"));
        }

        [TestMethod]
        public void DateEquality()
        {
            Assert.IsTrue(PartialDate.Parse("2010-06-03") == PartialDate.Parse("2010-06-03"));
            Assert.IsTrue(PartialDate.Parse("2010-07") == PartialDate.Parse("2010-07"));
            Assert.IsTrue(PartialDate.Parse("2011") == PartialDate.Parse("2011"));
            Assert.IsTrue(PartialDate.Parse("2010-06-02") != PartialDate.Parse("2010-06-03"));
            Assert.IsTrue(PartialDate.Parse("2010-07") != PartialDate.Parse("2010-05"));
            Assert.IsTrue(PartialDate.Parse("2010-06-02") != PartialDate.Parse("2010-06-03"));
            Assert.IsTrue(PartialDate.Parse("2018") != PartialDate.Parse("2019"));
            Assert.IsTrue(PartialDate.Parse("2010-06-03+02:00") == PartialDate.Parse("2010-06-03+02:00"));
            Assert.IsTrue(PartialDate.Parse("2010-06-03+02:00") != PartialDate.Parse("2010-06-03+05:00"));
        }

        [TestMethod]
        public void CheckOrdering()
        {
            Assert.AreEqual(0, PartialDate.Parse("2010-06-04").CompareTo(PartialDate.Parse("2010-06-04")));
            Assert.AreEqual(0, PartialDate.Parse("2010-06").CompareTo(PartialDate.Parse("2010-06")));
            Assert.AreEqual(0, PartialDate.Parse("2010").CompareTo(PartialDate.Parse("2010")));
            Assert.AreEqual(1, PartialDate.Parse("2010-06-04").CompareTo(PartialDate.Parse("2010-06-03")));
            Assert.AreEqual(1, PartialDate.Parse("2010-07").CompareTo(PartialDate.Parse("2010-06")));
            Assert.AreEqual(1, PartialDate.Parse("2017").CompareTo(PartialDate.Parse("2015")));
            Assert.AreEqual(-1, PartialDate.Parse("2010-06-04").CompareTo(PartialDate.Parse("2010-06-05")));
            Assert.AreEqual(-1, PartialDate.Parse("2010-05").CompareTo(PartialDate.Parse("2010-06")));
            Assert.AreEqual(-1, PartialDate.Parse("2010").CompareTo(PartialDate.Parse("2015")));
        }
    }
}