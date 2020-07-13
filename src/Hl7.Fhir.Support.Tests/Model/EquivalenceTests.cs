using Hl7.Fhir.Model.Primitives;
using Hl7.Fhir.Support.Utility;
using Hl7.FhirPath.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using P = Hl7.Fhir.Model.Primitives;

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

            static void test(decimal d, int p) => Assert.AreEqual(p, P.Decimal.Scale(d, ignoreTrailingZeroes: true));
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
        }

        [TestMethod]
        public void TestEqualityIncompatibleTypes()
        {
            Assert.IsFalse((bool)EqualityOperators.IsEqualTo(new Quantity(4.0, "kg"), new Code("http://nu.nl", "R")));
            Assert.IsFalse((bool)EqualityOperators.IsEqualTo(new P.Integer(0), new P.String("hi!")));

            Assert.IsFalse(EqualityOperators.IsEquivalentTo(new Quantity(4.0, "kg"), new Code("http://nu.nl", "R")));
            Assert.IsFalse(EqualityOperators.IsEquivalentTo(new P.Integer(0), new P.String("hi!")));
        }

        [TestMethod]
        public void TestEquality()
        {
            var tests = new (object, object, bool?)[]
            {
                (0, 0, true),
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
            };

            foreach (var (a,b,s) in tests) doTest(a,b,s, nameof(ICqlEquatable.IsEqualTo));          
        }

        void doTest(object a, object b, Result<bool> s, string op)
        {
            Assert.IsTrue(Any.TryConvertToSystemValue(a, out var aAny));
            Any bAny = null;
            if (!Any.TryConvertToSystemValue(b, out bAny)) bAny = null;

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

        [TestMethod]
        public void TestEquivalence()
        {
            var tests = new (object, object, bool)[]
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
            };

            foreach (var (a, b, s) in tests) doTest(a, b, s, nameof(ICqlEquatable.IsEquivalentTo));
        }
    }
}
