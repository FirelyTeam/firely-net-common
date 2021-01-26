using FluentAssertions;
using Hl7.Fhir.ElementModel;
using Hl7.FhirPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7.FhirPath.Tests
{
    [TestClass]
    public class OperationsTests
    {
        private static IEnumerable<object[]> EqualityOperatorTestcases() =>
            new (string expression, bool expected, bool invalid)[]
                 {
                    ("@2012 = @2012", true, false),
                    ("@2012 = @2013", false, false),
                    ("(@2012-01 = @2012).empty()", true, false),
                    ("@2012-01-01T10:30 = @2012-01-01T10:30", true, false),
                    ("@2012-01-01T10:30 = @2012-01-01T10:31", false, false),
                    ("(@2012-01-01T10:30:31 = @2012-01-01T10:30).empty()", true, false),
                    ("@2012-01-01T10:30:31.0 = @2012-01-01T10:30:31", true, false),
                    ("@2012-01-01T10:30:31.1 = @2012-01-01T10:30:31", false, false)
                 }.Select(t => new object[] { t.expression, t.expected, t.invalid });
        private static IEnumerable<object[]> GreaterThanOperatorTestcases() =>
            new (string expression, bool expected, bool invalid)[]
                 {
                    ("10 > 5", true, false),
                    ("10 > 5.0", true, false),
                    ("'abc' > 'ABC'", true, false),
                    ("8 'm' > 4 'm'", true, false),
                    ("(4 'm' > 4 'cm').empty()", true, false),   // we do not support unit conversion at the moment
                    ("@2018-03-01 > @2018-01-01", true, false),
                    ("(@2018-03 > @2018-03-01).empty()", true, false),
                    ("@2018-03-01T10:30:00 > @2018-03-01T10:00:00", true, false),
                    ("(@2018-03-01T10 > @2018-03-01T10:30).empty()", true, false),
                    ("@2018-03-01T10:30:00 > @2018-03-01T10:30:00.0", false, false),
                    ("@T10:30:00 > @T10:00:00", true, false),
                    ("(@T10 > @T10:30).empty()", true, false),
                    ("@T10:30:00 > @T10:30:00.0", false, false)
                 }.Select(t => new object[] { t.expression, t.expected, t.invalid });

        public static IEnumerable<object[]> AllFunctionTestcases()
        {
            return
                Enumerable.Empty<object[]>()
                .Union(EqualityOperatorTestcases())
                .Union(GreaterThanOperatorTestcases())
                ;
        }

        [DataTestMethod]
        [DynamicData(nameof(AllFunctionTestcases), DynamicDataSourceType.Method)]
        public void AssertTestcases(string expression, bool expected, bool invalid = false)
        {
            ITypedElement dummy = ElementNode.ForPrimitive(true);

            if (invalid)
            {
                Action act = () => dummy.IsBoolean(expression, expected);
                act.Should().Throw<Exception>();
            }
            else
            {
                dummy.IsBoolean(expression, expected).Should().BeTrue();
            }
        }
    }
}
