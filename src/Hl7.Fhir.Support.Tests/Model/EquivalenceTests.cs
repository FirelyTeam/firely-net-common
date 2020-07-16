using Hl7.FhirPath.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel.Types;

namespace HL7.FhirPath.Tests.Tests
{
    [TestClass]
    public class EquivalenceTests
    {
        [TestMethod]
        public void TestScale()
        {
            test(0m, 0);
            test(0.67m, 2);
            test(1.04m, 2);
            test(1.0m, 0);
            test(1.00m, 0);
            test(1.40m, 1);
            test(1m, 0);
            test(10000m, 0);
            test(10000.123m, 3);

            static void test(decimal d, int p) => Assert.AreEqual(p, Decimal.Scale(d, ignoreTrailingZeroes: true));
        }


        [TestMethod]
        public void TestEqualityNullBehaviour()
        {
            Assert.IsNull(EqualityOperators.IsEqualTo(null, null));
            Assert.IsNull(EqualityOperators.IsEqualTo(new Quantity(4.0, "kg"), null));
            Assert.IsNull(EqualityOperators.IsEqualTo(null, new Quantity(4.0, "kg")));

            Assert.IsTrue(EqualityOperators.IsEquivalentTo(null, null));
            Assert.IsFalse(EqualityOperators.IsEquivalentTo(new Quantity(4.0, "kg"), null));
            Assert.IsFalse(EqualityOperators.IsEquivalentTo(null, new Quantity(4.0, "kg")));

            Assert.IsNull(EqualityOperators.Compare(null, null, "<="));
            Assert.IsNull(EqualityOperators.Compare(new Quantity(4.0, "kg"), null, "<"));
            Assert.IsNull(EqualityOperators.Compare(null, new Quantity(4.0, "kg"), ">"));
        }

        [TestMethod]
        public void TestEqualityIncompatibleTypes()
        {
            Assert.IsFalse((bool)EqualityOperators.IsEqualTo(new Quantity(4.0, "kg"), new Code("http://nu.nl", "R")));
            Assert.IsFalse((bool)EqualityOperators.IsEqualTo(new Integer(0), new String("hi!")));

            Assert.IsFalse(EqualityOperators.IsEquivalentTo(new Quantity(4.0, "kg"), new Code("http://nu.nl", "R")));
            Assert.IsFalse(EqualityOperators.IsEquivalentTo(new Integer(0), new String("hi!")));

            Assert.ThrowsException<System.ArgumentException>( () => EqualityOperators.Compare(new Quantity(4.0, "kg"), new Code("http://nu.nl", "R"), "="));
            Assert.ThrowsException<System.ArgumentException>( () => EqualityOperators.Compare(new Integer(0), new String("hi!"), ">="));
        }

        internal static IEnumerable<object[]> equalityTestcases() =>
            new (object, object, bool?)[]
            {
                (1, 1, true),
                (-4, -4, true),
                (5, 5, true),
                (5, 6, false),
                (5, null, null),

                (0L, 0L, true),
                (-4L, -4L, true),
                (5L, 5L, true),
                (5L, 6L, false),
                (5L, null, null),

                (true, true, true),
                (false, true, false),
                (true, null, null),

                (4m, 4.0m, true),
                (4.0m, 4.000m, true),
                (400m, 4E2m, true),
                (4m, 4.1m, false),
                (5m, 4m, false),
                (4m, null, null),
                (1.2m/1.8m, 0.66666667m, true),
                (1.2m/1.8m, 0.6666667m, false),

                ("left", "left", true),
                ("left", "right", false),
                ("left", " left", false),
                ("left", "lEft", false),
                ("\tleft", " lEft", false),
                ("encyclopaedia", "encyclopædia", false),
                ("café", "cafe", false),
                ("right", null, null),

                (PartialDate.Parse("2001"), PartialDate.Parse("2001"), true),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-01"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01-30"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01"), null),
                (PartialDate.Parse("2002-01-30"), PartialDate.Parse("2001-01"), false),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001"), null),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002-01"), false),   // false, not null - first compare components, then precision!
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-02"), false),
                (PartialDate.Parse("2010-06-02"), PartialDate.Parse("2010-06-03"), false),
                (PartialDate.Parse("2010-06-02"), null, null),

                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01-01"), true),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01"), null),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-02"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:00"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41:50+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02"), null),
                (PartialDateTime.Parse("2010-06-02"), null, null),

                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00.00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00"), null),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:01"), false),
                (PartialTime.Parse("13:40:50+02:00"), PartialTime.Parse("13:40:50+02:00"), true),
                (PartialTime.Parse("13:40:50+00:00"), PartialTime.Parse("13:40:50Z"), true),
                (PartialTime.Parse("13:40:50+00:10"), PartialTime.Parse("13:40:50Z"), false),
                (PartialTime.Parse("13:45:02Z"), PartialTime.Parse("13:45:02+00:00"), true),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:02+01:00"), true),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:45:02+01:00"), false),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:03+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:46+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), null, null),

                (PartialDateTime.Parse("2012-04-15T15:00:00Z"), PartialDateTime.Parse("2012-04-17"), false),  // clearly false, whatever the timezone
                (PartialDateTime.Parse("2012-04-15T15:00:00Z"), PartialDateTime.Parse("2012-04-15T10:00:00"), null),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T10:00:00Z"), null),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T10:00:00"), false),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T15:00:00"), true),

                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("25.0 'kg'"), false),
                //Until we actually implement UCUM, these tests cannot yet be run correctly
                //(Quantity.Parse("1 year"), Quantity.Parse("1 'a'"), false),
                //(Quantity.Parse("1 month"), Quantity.Parse("1 'mo'"), false),
                //(Quantity.Parse("1 hour"), Quantity.Parse("1 'h'"), false),
                //(Quantity.Parse("1 second"), Quantity.Parse("1 's'"), true),
                //(Quantity.Parse("1 millisecond"), Quantity.Parse("1 'ms'"), true),
                (Quantity.Parse("24.0 'kg'"), null, null),
            }.Select(t => new[] { t.Item1, t.Item2, t.Item3 } );


        internal static IEnumerable<object> equivalenceTestcases()
        {
            return new (object, object, bool)[]
            {
                (0, 0, true),
                (-4, -4, true),
                (5, 5, true),
                (5, 6, false),
                (5, null, false),

                (0L, 0L, true),
                (-4L, -4L, true),
                (5L, 5L, true),
                (5L, 6L, false),
                (5L, null, false),

                (true, true, true),
                (false, true, false),
                (true, null, false),

                (4m, 4.0m, true),
                (4.0m, 4.000m, true),
                (400m, 4E2m, true),
                (4m, 4.1m, true),
                (5m, 4m, false),
                (4m, null, false),
                (1.2m/1.8m, 0.66666667m, true),

                ("left", "left", true),
                ("left", "right", false),
                ("left", " left", false),
                ("left", "lEft", true),
                ("\tleft", " lEft", true),
                ("encyclopaedia", "encyclopædia", true),
                ("café", "cafe", true),
                ("right", null, false),

                (PartialDate.Parse("2001"), PartialDate.Parse("2001"), true),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-01"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01-30"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01"), false),
                (PartialDate.Parse("2002-01-30"), PartialDate.Parse("2001-01"), false),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002-01"), false),   // false, not null - first compare components, then precision!
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-02"), false),
                (PartialDate.Parse("2010-06-02"), PartialDate.Parse("2010-06-03"), false),
                (PartialDate.Parse("2010-06-02"), null, false),

                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01-01"), true),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01"), false),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-02"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:00"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41:50+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02"), false),
                (PartialDateTime.Parse("2010-06-02"), null, false),

                (PartialDateTime.Parse("2012-04-15T15:00:00Z"), PartialDateTime.Parse("2012-04-17"), false),  // clearly false, whatever the timezone
                (PartialDateTime.Parse("2012-04-15T15:00:00Z"), PartialDateTime.Parse("2012-04-15T10:00:00"), false),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T10:00:00Z"), false),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T10:00:00"), false),
                (PartialDateTime.Parse("2012-04-15T15:00:00"), PartialDateTime.Parse("2012-04-15T15:00:00"), true),

                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00.00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00"), false),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:01"), false),
                (PartialTime.Parse("13:40:50+02:00"), PartialTime.Parse("13:40:50+02:00"), true),
                (PartialTime.Parse("13:40:50+00:00"), PartialTime.Parse("13:40:50Z"), true),
                (PartialTime.Parse("13:40:50+00:10"), PartialTime.Parse("13:40:50Z"), false),
                (PartialTime.Parse("13:45:02Z"), PartialTime.Parse("13:45:02+00:00"), true),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:02+01:00"), true),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:45:02+01:00"), false),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:03+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:46+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), null, false),

                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("25.0 'kg'"), false),
                (Quantity.Parse("1 year"), Quantity.Parse("1 'a'"), true),
                (Quantity.Parse("1 month"), Quantity.Parse("1 'mo'"), true),
                (Quantity.Parse("1 hour"), Quantity.Parse("1 'h'"), true),
                (Quantity.Parse("1 second"), Quantity.Parse("1 's'"), true),
                (Quantity.Parse("1 millisecond"), Quantity.Parse("1 'ms'"), true),
                (Quantity.Parse("24.0 'kg'"), null, false),
            }.Select(t => new[] { t.Item1, t.Item2, t.Item3 });
        }

        [DataTestMethod]
        [DynamicData(nameof(equalityTestcases), DynamicDataSourceType.Method)]
        public void EqualityTest(object a, object b, bool? s)
        {
            doEqEquivTest(a, b, s, nameof(ICqlEquatable.IsEqualTo));
        }

        [DataTestMethod]
        [DynamicData(nameof(equivalenceTestcases), DynamicDataSourceType.Method)]
        public void EquivalenceTest(object a, object b, bool? s)
        {
            doEqEquivTest(a, b, s, nameof(ICqlEquatable.IsEquivalentTo));
        }

        private static void doEqEquivTest(object a, object b, bool? s, string op)
        {
            Assert.IsTrue(Any.TryConvert(a, out var aAny));
            if (!Any.TryConvert(b, out Any bAny)) bAny = null;

            var result = aAny is ICqlEquatable ce ?
                (op == nameof(ICqlEquatable.IsEqualTo) ? ce.IsEqualTo(bAny) : ce.IsEquivalentTo(bAny))
                : false;

            if (result != s)
            {
                Assert.Fail($"{op}({sn(a)},{sn(b)}) was expected to be '{sn(s)}', " +
                        $"but was '{sn(result)}' for {sn((a ?? b)?.GetType().Name)}");
            }
        }

        private static string sn(object x) => x?.ToString() ?? "null";


        internal static IEnumerable<object> orderingTestcases() =>
            new (object, object, int?)[]
            {
                (4, 4, 0),
                (-4, -3, -1),
                (5, 6, -1),
                (5, null, null),

                (4L, 4L, 0),
                (-4L, -3L, -1),
                (5L, 6L, -1),
                (5L, null, null),

                (4m, 4.0m, 0),
                (4.0m, 4.000m, 0),
                (401m, 4E2m, 1),
                (4m, 4.1m, -1),
                (5m, 4m, 1),
                (3.141592651m,3.14159265m,0),
                (3.141592651m,3.14159264m,1),

                (4m, null, null),

                ("left", "left", 0),
                ("aleft", "bright", -1),
                ("aaab", "aaac", -1),

                (PartialDate.Parse("2010-06-03"), null,null),
                (PartialDate.Parse("2010-06-03"), PartialDate.Parse("2010-06-03"),0),
                (PartialDate.Parse("2010-06"), PartialDate.Parse("2010-06"),0),
                (PartialDate.Parse("2010"), PartialDate.Parse("2010"),0),
                (PartialDate.Parse("2010-06-03"), PartialDate.Parse("2010-05-03"),1),
                (PartialDate.Parse("2010-07-03"), PartialDate.Parse("2010-07-02"),1),
                (PartialDate.Parse("2011"), PartialDate.Parse("2010"),1),
                (PartialDate.Parse("2011-03"), PartialDate.Parse("2011-03"),0),
                (PartialDate.Parse("2011-03"), PartialDate.Parse("2011-02"),1),
                (PartialDate.Parse("2010-12-05+02:00"), PartialDate.Parse("2010-12-04+02:00"),1),
                (PartialDate.Parse("2010-12-05+02:00"), PartialDate.Parse("2010-12-05+02:00"),0),
                (PartialDate.Parse("2010-12-05+08:00"), PartialDate.Parse("2010-12-04+02:00"),1),
                (PartialDate.Parse("2011-03"), PartialDate.Parse("2011-04-05"),-1),
                (PartialDate.Parse("2012"), PartialDate.Parse("2011-04-05"),1),
                (PartialDate.Parse("2011-03"), PartialDate.Parse("2011-03-05"),null),
                (PartialDate.Parse("2011-03"), PartialDate.Parse("2011"),null),

                (PartialTime.Parse("13:00:00Z"), PartialTime.Parse("12:00:00Z"),1),
                (PartialTime.Parse("13:00:00Z"), PartialTime.Parse("18:00:00+02:00"), -1),
                (PartialTime.Parse("12:34:00Z"), PartialTime.Parse("12:33:55+00:00"), 1),
                (PartialTime.Parse("13:00Z"), PartialTime.Parse("14Z"), -1),
                (PartialTime.Parse("13:01:00Z"), PartialTime.Parse("13:00Z"), 1),
                (PartialTime.Parse("13:00:00Z"), PartialTime.Parse("13:00:01Z"), -1),
                (PartialTime.Parse("13:00:00Z"), PartialTime.Parse("13:00:01"), null),
                (PartialTime.Parse("13:00:00"), PartialTime.Parse("13:00:01Z"), null),
                (PartialTime.Parse("13:00:00"), PartialTime.Parse("13:00:00"), 0),
                (PartialTime.Parse("13:01:00Z"), null, null),

                (PartialDateTime.Parse("2010-12-05T13:00:00+08:00"), PartialDateTime.Parse("2010-11-01"),1),
                (PartialDateTime.Parse("2010-12-05T13:00:00"), PartialDateTime.Parse("2010-12-05"),null),
                (PartialDateTime.Parse("2010-12-05T13:00:00"), PartialDateTime.Parse("2010-12-05Z"),null),
                (PartialDateTime.Parse("2010-12-05T13:00:00Z"), PartialDateTime.Parse("2010-12-05Z"),null),
                (PartialDateTime.Parse("2010-12-05T13:00:00"), PartialDateTime.Parse("2010-11-01T13:00:01"),1),
                (PartialDateTime.Parse("2010-12-05T13:00:00Z"), PartialDateTime.Parse("2010-12-05T13:00:01"),null),

                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("24.0 'kg'"), 0),
                (Quantity.Parse("25 'kg'"), Quantity.Parse("24.0 'kg'"), 1),
                //Until we actually implement UCUM, these tests cannot yet be run correctly
                //(Quantity.Parse("1 year"), Quantity.Parse("1 'a'"), true),
                //(Quantity.Parse("1 month"), Quantity.Parse("1 'mo'"), true),
                //(Quantity.Parse("1 hour"), Quantity.Parse("1 'h'"), true),
                //(Quantity.Parse("1 second"), Quantity.Parse("1 's'"), true),
                //(Quantity.Parse("1 millisecond"), Quantity.Parse("1 'ms'"), true),
                (Quantity.Parse("24.0 'kg'"), null, null),
             }.Select(t => new[] { t.Item1, t.Item2, t.Item3 });
        
        [DataTestMethod]
        [DynamicData(nameof(orderingTestcases), DynamicDataSourceType.Method)]
        public void OrderingTest(object a, object b, int? s)
        {
            if (s == 1 || s == -1)
            {
                doOrderingTest(a, b, s);
                doOrderingTest(b, a, -s);
            }
            else
                doOrderingTest(a, b, s);
        }

        private static void doOrderingTest(object a, object b, int? s)
        {
            Assert.IsTrue(Any.TryConvert(a, out var aAny));
            if (!Any.TryConvert(b, out Any bAny)) bAny = null;

            var result = aAny is ICqlOrderable ce ? ce.CompareTo(bAny) : -100;

            if (result != s)
            {
                Assert.Fail($"{nameof(ICqlOrderable.CompareTo)}({sn(a)},{sn(b)}) was expected to be '{sn(s)}', " +
                        $"but was '{sn(result)}' for {sn((a ?? b)?.GetType().Name)}");
            }
        }
    }
}
