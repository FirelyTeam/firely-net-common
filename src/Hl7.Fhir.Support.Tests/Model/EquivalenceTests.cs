using Hl7.Fhir.Model.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Decimal = Hl7.Fhir.Model.Primitives.Decimal;

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

            void test(decimal d, int p) => Assert.AreEqual(p, Decimal.Scale(d, ignoreTrailingZeroes: true));
        }


        [TestMethod]
        public void TestEquality()
        {
            // Test chapter 6.1 - Equality
            var tests = new (object, object, bool?)[]
            {
                (null, null, null),

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
                (null, "right", null),

                (PartialDate.Parse("2001"), PartialDate.Parse("2001"), true),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-01"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01-30"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01"), null),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001"), null),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002-01"), false),   // false, not null - first compare components, then precision!
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-02"), false),
                (PartialDate.Parse("2010-06-02"), PartialDate.Parse("2010-06-03"), false),

                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01-01"), true),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01"), null),
                (PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:00"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41:50+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02"), null),

                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00.00"), null),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00"), null),

                (PartialTime.Parse("13:40:50+02:00"), PartialTime.Parse("13:40:50+02:00"), true),
                (PartialTime.Parse("13:40:50+00:00"), PartialTime.Parse("13:40:50Z"), true),
                (PartialTime.Parse("13:40:50+00:10"), PartialTime.Parse("13:40:50Z"), false),
                (PartialTime.Parse("13:45:02Z"), PartialTime.Parse("13:45:02+00:00"), true),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:02+01:00"), true),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:45:02+01:00"), false),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:03+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:46+01:00"), false),

                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                (Quantity.Parse("24.0 'kg'"), Quantity.Parse("25.0 'kg'"), false),
            };

            foreach (var (a,b,s) in tests) doTest(a,b,s);

            void doTest(object a,object b, bool? s)
            {
                var result = Any.IsEqualTo(a, b);

                if(result != s)
                {
                    Assert.Fail($"IsEqualTo({sn(a)},{sn(b)}) was expected to be '{sn(s)}', " +
                            $"but was '{sn(result)}' for {sn((a ?? b)?.GetType().Name)}");
                }             
            }

            //IsTrue(@"Patient.identifier = Patient.identifier");
            //IsTrue(@"Patient.identifier.first() != Patient.identifier.skip(1)");
            //IsTrue(@"(1|2|3) = (1|2|3)");
            //IsTrue(@"(1|2|3) = (1.0|2.0|3)");
            //IsTrue(@"(1|Patient.identifier|3) = (1|Patient.identifier|3)");
            //IsTrue(@"(3|Patient.identifier|1) != (1|Patient.identifier|3)");

            //IsTrue(@"Patient.gender = 'male'"); // gender has an extension
            //IsTrue(@"Patient.communication = Patient.communication");       // different extensions, same values
            //IsTrue(@"Patient.communication.first() = Patient.communication.skip(1)");       // different extensions, same values
        }

        private static string sn(object x) => x?.ToString() ?? "null";

        [TestMethod]
        public void TestEquivalence()
        {
            var tests = new (object, object, bool)[]
            {
                (null, null, true),

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
                (4m, 4.14m, true),
                (1.2m/1.8m, 0.67m, true),
                (4.2m, 4.14m, false),
                (4m, 5m, false),
                (null, -4m, false),

                ("left", "left", true),
                ("left", "right", false),
                ("left", " left", false),
                ("left", "lEft", true),
                ("\tleft", " lEft", true),
                ("encyclopaedia", "encyclopædia", true),
                ("café", "cafe", true),
                (null,"cafe", false),

                (PartialDate.Parse("2001"), PartialDate.Parse("2001"), true),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-01"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01-30"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01"), false),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002"), false),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-02"), false),
                (PartialDate.Parse("2010-06-02"), PartialDate.Parse("2010-06-03"), false),

                (PartialDate.Parse("2001"), PartialDate.Parse("2001"), true),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-01"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01-30"), true),
                (PartialDate.Parse("2001-01-30"), PartialDate.Parse("2001-01"), false),
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002"), false),
                (PartialDate.Parse("2001"), PartialDate.Parse("2002-01"), false),   // false, not null - first compare components, then precision!
                (PartialDate.Parse("2001-01"), PartialDate.Parse("2001-02"), false),
                (PartialDate.Parse("2010-06-02"), PartialDate.Parse("2010-06-03"), false),

                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01-01"), true),
                (PartialDateTime.Parse("2015-01-01"), PartialDateTime.Parse("2015-01"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), PartialDateTime.Parse("2015-01-02T13:40:50+02:00"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:00"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), true),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:40:50Z"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41:50+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02T13:41+00:10"), false),
                (PartialDateTime.Parse("2015-01-02T13:40:50+00:10"), PartialDateTime.Parse("2015-01-02"), false),

                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00"), true),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00:00.00"), false),
                (PartialTime.Parse("12:00:00"), PartialTime.Parse("12:00"), false),

                (PartialTime.Parse("13:40:50+02:00"), PartialTime.Parse("13:40:50+02:00"), true),
                (PartialTime.Parse("13:40:50+00:00"), PartialTime.Parse("13:40:50Z"), true),
                (PartialTime.Parse("13:40:50+00:10"), PartialTime.Parse("13:40:50Z"), false),
                (PartialTime.Parse("13:45:02Z"), PartialTime.Parse("13:45:02+00:00"), true),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:02+01:00"), true),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:45:02+01:00"), false),
                (PartialTime.Parse("13:45:02+01:00"), PartialTime.Parse("13:45:03+01:00"), false),
                (PartialTime.Parse("13:45:02+00:00"), PartialTime.Parse("13:46+01:00"), false),

                //(Quantity.Parse("24.0 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                //(Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                //(Quantity.Parse("24 'kg'"), Quantity.Parse("24.0 'kg'"), true),
                //(Quantity.Parse("24.0 'kg'"), Quantity.Parse("25.0 'kg'"), false),
            };

            foreach (var (a, b, s) in tests) doTest(a, b, s);

            void doTest(object a, object b, bool s)
            {
                var result = Any.IsEquivalentTo(a, b);

                if (result != s)
                {
                    Assert.Fail($"IsEquivalentTo({sn(a)},{sn(b)}) was expected to be '{sn(s)}', " +
                            $"but was '{sn(result)}' for {sn((a ?? b)?.GetType().Name)}");
                }
            }
        }

    }
}
