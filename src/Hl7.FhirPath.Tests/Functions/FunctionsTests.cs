using Hl7.Fhir.ElementModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace HL7.FhirPath.Tests
{
    [TestClass]
    public class FunctionsTests
    {
        public static IEnumerable<object[]> ExistenceFunctionTestcases()
        {
            // function empty() : Boolean
            yield return new object[] { "{}.empty()", true };
            yield return new object[] { "(1).empty()", false };
            yield return new object[] { "(1 | 2 | 3).empty()", false };

            // function exists([criteria : expression]) : Boolean
            yield return new object[] { "{}.exists()", false };
            yield return new object[] { "(1).exists()", true };
            yield return new object[] { "(1 | 2 | 3).exists()", true };
            yield return new object[] { "(1 | 2 | 3).exists($this > 0)", true };
            yield return new object[] { "(1 | 2 | 3).exists($this < 0)", false };


            // function all(criteria : expression) : Boolean
            yield return new object[] { "{}.all(true)", true };
            yield return new object[] { "{}.all(false)", true };
            yield return new object[] { "(1 | 2 | 3).all($this > 0 )", true };

            // function  allTrue() : Boolean
            yield return new object[] { "{}.allTrue()", true };
            yield return new object[] { "true.allTrue()", true };
            yield return new object[] { "false.allTrue()", false };
            yield return new object[] { "(true | true).allTrue()", true };
            yield return new object[] { "(true | false).allTrue()", false };
            yield return new object[] { "(false | true).allTrue()", false };
            yield return new object[] { "(false | false).allTrue()", false };

            // function anyTrue() : Boolean
            yield return new object[] { "{}.anyTrue()", false };
            yield return new object[] { "true.anyTrue()", true };
            yield return new object[] { "false.anyTrue()", false };
            yield return new object[] { "(true | true).anyTrue()", true };
            yield return new object[] { "(true | false).anyTrue()", true };
            yield return new object[] { "(false | true).anyTrue()", true };
            yield return new object[] { "(false | false).anyTrue()", false };

            // function allFalse() : Boolean
            yield return new object[] { "{}.allFalse()", true };
            yield return new object[] { "true.allFalse()", false };
            yield return new object[] { "false.allFalse()", true };
            yield return new object[] { "(true | true).allFalse()", false };
            yield return new object[] { "(true | false).allFalse()", false };
            yield return new object[] { "(false | true).allFalse()", false };
            yield return new object[] { "(false | false).allFalse()", true };

            // function anyFalse() : Boolean
            yield return new object[] { "{}.anyFalse()", false };
            yield return new object[] { "true.anyFalse()", false };
            yield return new object[] { "false.anyFalse()", true };
            yield return new object[] { "(true | true).anyFalse()", false };
            yield return new object[] { "(true | false).anyFalse()", true };
            yield return new object[] { "(false | true).anyFalse()", true };
            yield return new object[] { "(false | false).anyFalse()", true };

            // function subsetOf(other : collection) : Boolean
            yield return new object[] { "(1 | 2).subsetOf((1 | 2 | 3))", true };
            yield return new object[] { "(1 | 3).subsetOf((1 | 2 | 3))", true };
            yield return new object[] { "(4 | 5).subsetOf((1 | 2 | 3))", false };
            yield return new object[] { "{}.subsetOf(1)", true };
            yield return new object[] { "{}.subsetOf({})", false };

            // function supersetOf(other : collection) : Boolean
            yield return new object[] { "(1 | 2 | 3).supersetOf((1 | 2))", true };
            yield return new object[] { "(1 | 2 | 3).supersetOf((1 | 3))", true };
            yield return new object[] { "(1 | 2 | 3).supersetOf(4 | 5))", false };
            yield return new object[] { "1.supersetOf({})", true };
            yield return new object[] { "{}.supersetOf({})", false };

            // function count() : Integer
            yield return new object[] { "{}.count() = 0", true };
            yield return new object[] { "34.count() = 1", true };
            yield return new object[] { "(1 | 2 | 3).count() = 3", true };
            yield return new object[] { "(1 | 1 | 1).count() = 1", true };
            yield return new object[] { "(1.combine(1.combine(1))).count() = 3", true };

            // function distinct() : collection
            yield return new object[] { "{}.distinct().empty()", true };
            yield return new object[] { "(2).distinct() = 2", true };
            yield return new object[] { "(1 | 2).distinct() = (1 | 2)", true };
            yield return new object[] { "(1 | 2).distinct() = (2 | 1)", false };
            yield return new object[] { "(1 | 1).distinct() = 1", true };
            yield return new object[] { "(1.combine(1.combine(1))).distinct() = 1", true };

            // function isDistinct() : Boolean
            yield return new object[] { "{}.isDistinct()", true };
            yield return new object[] { "(2).isDistinct()", true };
            yield return new object[] { "(1 | 2).isDistinct()", true };
            yield return new object[] { "(1.combine(1)).isDistinct()", false };
        }

        public static IEnumerable<object[]> FilteringAndProjectionFunctionTestcases()
        {
            // function where(criteria : expression) : collection
            yield return new object[] { "{}.where(true).empty()", true };
            yield return new object[] { "{}.where(false).empty()", true };
            yield return new object[] { "(1).where(false).empty()", true };
            yield return new object[] { "(1 | 2 | 3).where($this > 1) = (2 | 3)", true };
            yield return new object[] { "(1 | 2 | 3).where(true) = (1 | 2 | 3)", true };

            // function select(projection: expression) : collection
            yield return new object[] { "{}.select(true).empty()", true };
            yield return new object[] { "(1 | 2).select($this + 1) = (2 | 3)", true };
            yield return new object[] { "(1 | 2).select($this as String) = ('1' | '2')", true };

            // function repeat(projection: expression) : collection

            // function ofType(type : type specifier) : collection 
            yield return new object[] { "{}.ofType(String).empty()", true };
            yield return new object[] { "('a').ofType(String).count() = 1", true };
            yield return new object[] { "('a').ofType('String').count() = 1", false };
            yield return new object[] { "(1 | 2).ofType(`Integer64`).count() = 2", true };
            yield return new object[] { "(1 | 2).ofType(System.`Integer64`).count() = 2", true };
            yield return new object[] { "(1 | 2).ofType(System.Integer64).count() = 2", true };
            yield return new object[] { "(1 | 2).ofType(Integer64).count() = 2", true };
        }

        public static IEnumerable<object[]> SubsettingFunctionTestcases()
        {
            // function  [ index : Integer ] : collection
            yield return new object[] { "{}[0].empty()", true };
            yield return new object[] { "(1 | 2)[0] = 1", true };
            yield return new object[] { "(1 | 2)[1] = 2", true };
            yield return new object[] { "(1 | 2)[2].empty()", true };
            yield return new object[] { "(1 | 2 | 3)[2] = 3", true };

            // function single() : collection
            yield return new object[] { "{}.single().empty()", true };
            yield return new object[] { "(1).single() = 1", true };
            yield return new object[] { "(1 | 2).single()", true };

            // function first() : collection
            yield return new object[] { "{}.first().empty()", true };
            yield return new object[] { "1.first() = 1", true };
            yield return new object[] { "(1).first() = 1", true };
            yield return new object[] { "(1 | 2).first() = 1", true };

            // function last() : collection
            yield return new object[] { "{}.last().empty()", true };
            yield return new object[] { "1.last() = 1", true };
            yield return new object[] { "(1).last() = 1", true };
            yield return new object[] { "(1 | 2).last() = 2", true };

            // function tail() : collection
            yield return new object[] { "{}.tail().empty()", true };
            yield return new object[] { "1.tail().empty()", true };
            yield return new object[] { "(1).tail().empty()", true };
            yield return new object[] { "(1 | 2).tail() = 2", true };
            yield return new object[] { "(1 | 2 | 3).tail() = (2 | 3)", true };

            // function skip(num : Integer) : collection
            yield return new object[] { "{}.skip(1).empty()", true };
            yield return new object[] { "{}.skip(0).empty()", true };
            yield return new object[] { "{}.skip(-1).empty()", true };
            yield return new object[] { "1.skip(1).empty()", true };
            yield return new object[] { "(1).skip(1).empty()", true };
            yield return new object[] { "(1 | 2).skip(1) = 2", true };
            yield return new object[] { "(1 | 2 | 3).skip(2) = 3", true };
            yield return new object[] { "(1 | 2 | 3).skip(3).empty()", true };
            yield return new object[] { "(1 | 2 | 3).skip(0) = (1 | 2 | 3)", true };
            yield return new object[] { "(1 | 2 | 3).skip(-1) = (1 | 2 | 3)", true };

            // function take(num : Integer) : collection
            yield return new object[] { "{}.take(1).empty()", true };
            yield return new object[] { "{}.take(0).empty()", true };
            yield return new object[] { "{}.take(-1).empty()", true };
            yield return new object[] { "1.take(0).empty()", true };
            yield return new object[] { "(1).take(1) = 1", true };
            yield return new object[] { "(1 | 2).take(1) = 1", true };
            yield return new object[] { "(1 | 2).take(2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2).take(3) = (1 | 2)", true };
            yield return new object[] { "(1 | 2).take(2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2 | 3).take(2) = (1 | 2)", true };

            // function intersect(other: collection) : collection
            yield return new object[] { "{}.intersect(1 | 2).empty()", true };
            yield return new object[] { "{}.intersect({}).empty()", true };
            yield return new object[] { "(1 | 2).intersect({}).empty()", true };
            yield return new object[] { "(1).intersect(1 | 2) = 1", true };
            yield return new object[] { "(1 | 2).intersect(1 | 2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2 | 3).intersect(1 | 2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2).intersect(1 | 2 | 3) = (1 | 2)", true };
            yield return new object[] { "(1 | 2 | 3).intersect(4 | 5).empty()", true };
            yield return new object[] { "(1 | 2 | 2 | 3).intersect(1 | 1 | 5) = 1", true };

            // function exclude(other: collection) : collection
            yield return new object[] { "{}.exclude({}).empty()", true };
            yield return new object[] { "{}.exclude(1).empty()", true };
            yield return new object[] { "(1).exclude({}) = 1", true };
            yield return new object[] { "(1).exclude(1).empty()", true };
            yield return new object[] { "(1).exclude(2) = 1", true };
            yield return new object[] { "(1 | 2 | 3).exclude(2) = (1 | 3)", true };
            yield return new object[] { "(1 | 2 | 2 | 3).exclude(2) = (1 | 3)", false }; //  Duplicate items will not be eliminated by this function
            yield return new object[] { "(1 | 2 | 3).exclude(2 | 3) = 1", true };
            yield return new object[] { "(1 | 2 | 3).exclude(3 | 2) = 1", true };
        }

        public static IEnumerable<object[]> CombiningFunctionTestcases()
        {
            // function union(other : collection)
            yield return new object[] { "{}.union({}).empty()", true };
            yield return new object[] { "{}.union(1) = 1", true };
            yield return new object[] { "{}.union(1 | 2) = (1 | 2)", true };
            yield return new object[] { "1.union({}) = 1", true };
            yield return new object[] { "(1 | 2).union({}) = (1 | 2)", true };
            yield return new object[] { "1.union(2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2).union(3 | 4) = (1 | 2 | 3 | 4)", true };
            yield return new object[] { "(1.combine(1.combine(2.combine(3)))).union(2 | 3) = (1 | 2 | 3)", true };

            // function combine(other : collection) : collection
            yield return new object[] { "{}.combine({}).empty()", true };
            yield return new object[] { "{}.combine(1) = 1", true };
            yield return new object[] { "{}.combine(1 | 2) = (1 | 2)", true };
            yield return new object[] { "1.combine({}) = 1", true };
            yield return new object[] { "(1 | 2).combine({}) = (1 | 2)", true };
            yield return new object[] { "1.combine(2) = (1 | 2)", true };
            yield return new object[] { "(1 | 2).combine(3 | 4) = (1 | 2 | 3 | 4)", true };
            yield return new object[] { "(1.combine(1.combine(2.combine(3)))).combine(2 | 3) = (1.combine(1.combine(2.combine(3.combine(2.combine(3))))))", true };
        }

        public static IEnumerable<object[]> ConversionFunctionTestcases()
        {
            // function iif(criterion: expression, true-result: collection [, otherwise-result: collection]) : collection
            yield return new object[] { "iif({}, true, false)", false };
            yield return new object[] { "iif(false, true).empty()", true };

            yield return new object[] { "iif({ }, true, false)", false };
            yield return new object[] { "iif(true, true, false)", true };
            yield return new object[] { "iif({ } | true, true, false)", true };
            yield return new object[] { "iif(true, true, 1/0)", true };
            yield return new object[] { "iif(false, 1/0, true)", true };
        }

        public static IEnumerable<object[]> StringManipulationFunctionTestcases()
        {
            // function indexOf(substring : String) : Integer
            yield return new object[] { "{}.indexOf('a').empty()", true };
            yield return new object[] { "{}.indexOf('').empty()", true };
            yield return new object[] { "'abcdefg'.indexOf('bc') = 1", true };
            yield return new object[] { "'abcdefg'.indexOf('x') = -1", true };
            yield return new object[] { "'abcdefg'.indexOf('abcdefg') = 0", true };
            yield return new object[] { "('a' | 'b').indexOf('a')", true }; // should throw an error

            // function substring(start : Integer [, length : Integer]) : String
            yield return new object[] { "{}.substring(0).empty()", true };
            yield return new object[] { "{}.substring(0, 1).empty()", true };
            yield return new object[] { "'abc'.substring({}).empty()", true };
            yield return new object[] { "'abc'.substring(1) = 'bc'", true };
            yield return new object[] { "'abc'.substring(1, {}) = 'bc'", true }; // this should be the same as the previous testcase
            yield return new object[] { "'abcdefg'.substring(3) = 'defg'", true };
            yield return new object[] { "'abcdefg'.substring(1, 2) = 'bc'", true };
            yield return new object[] { "'abcdefg'.substring(6, 2) = 'g'", true };
            yield return new object[] { "'abcdefg'.substring(7, 1).empty()", true };

            // function startsWith(prefix : String) : Boolean
            yield return new object[] { "{}.startsWith('a').empty()", true };
            yield return new object[] { "'abc'.startsWith('')", true };
            yield return new object[] { "'abc'.startsWith('a')", true };
            yield return new object[] { "'abc'.startsWith('ab')", true };
            yield return new object[] { "'abc'.startsWith('abc')", true };
            yield return new object[] { "'abc'.startsWith('x')", false };

            // function endsWith(suffix : String) : Boolean
            yield return new object[] { "{}.endsWith('a').empty()", true };
            yield return new object[] { "'abc'.endsWith('')", true };
            yield return new object[] { "'abc'.endsWith('c')", true };
            yield return new object[] { "'abc'.endsWith('bc')", true };
            yield return new object[] { "'abc'.endsWith('abc')", true };
            yield return new object[] { "'abc'.endsWith('x')", false };

            // function contains(substring : String) : Boolean
            yield return new object[] { "{}.contains('a').empty()", true };
            yield return new object[] { "'abc'.contains('')", true };
            yield return new object[] { "'abc'.contains('b')", true };
            yield return new object[] { "'abc'.contains('bc')", true };
            yield return new object[] { "'abc'.contains('d')", false };

            // function upper() : String
            yield return new object[] { "{}.upper().empty()", true };
            yield return new object[] { "'a'.upper() = 'A'", true };
            yield return new object[] { "'abcdefg'.upper() = 'ABCDEFG'", true };
            yield return new object[] { "'AbCdefg'.upper() = 'ABCDEFG'", true };
            yield return new object[] { "'AbC2e;~fg'.upper() = 'ABC2E;~FG'", true };

            // function lower() : String
            yield return new object[] { "{}.lower().empty()", true };
            yield return new object[] { "'A'.lower() = 'a'", true };
            yield return new object[] { "'a'.lower() = 'a'", true };
            yield return new object[] { "'ABCDEFG'.lower() = 'abcdefg'", true };
            yield return new object[] { "'AbCdefg'.lower() = 'abcdefg'", true };
            yield return new object[] { "'AbC2e;~fg'.lower() = 'abc2e;~fg'", true };

            // function replace(pattern : String, substitution : String) : String
            yield return new object[] { "{}.replace({}, {}).empty()", true };
            yield return new object[] { "''.replace({}, {}).empty()", true };
            yield return new object[] { "'abc'.replace('b', '') = 'ac'", true };
            yield return new object[] { "'abcbdbebf'.replace('b', '') = 'acdef'", true };
            yield return new object[] { "'abc'.replace('', 'x') = 'xaxbxcx'", true };
            yield return new object[] { "'abc'.replace('', 'x').replace('x', '') = 'abc'", true };
            yield return new object[] { "'abcdefg'.replace('cde', '123') = 'ab123fg'", true };
            yield return new object[] { "'abcdefg'.replace('cde', '') = 'abfg'", true };

            // function matches(regex : String) : Boolean
            yield return new object[] { "{}.matches('').empty()", true };
            yield return new object[] { "{}.matches({}).empty()", true };
            yield return new object[] { "'abc'.matches({}).empty()", true };
            yield return new object[] { "'123'.matches('^[1-3]+$')", true };
            yield return new object[] { "'1234'.matches('^[1-3]+$')", false };
            yield return new object[] { "''.matches('^[1-3]+$')", false };
            yield return new object[] { "'1055 RW'.matches('^[1-9][0-9]{3} ?(?!SA|SD|SS)[A-Z]{2}$')", true };
            yield return new object[] { "'1055 SS'.matches('^[1-9][0-9]{3} ?(?!SA|SD|SS)[A-Z]{2}$')", false };
            yield return new object[] { "'1055RW'.matches('^[1-9][0-9]{3} ?(?!SA|SD|SS)[A-Z]{2}$')", true };

            // function replaceMatches(regex : String, substitution: String) : String
            yield return new object[] { "{}.replaceMatches({}, {}).empty()", true };
            yield return new object[] { @"'11/30/1972'.replaceMatches('\\b(?<month>\\d{1,2})/(?<day>\\d{1,2})/(?<year>\\d{2,4})\\b','${day}-${month}-${year}') = '30-11-1972'", true };

            // function length() : Integer
            yield return new object[] { "{}.length().empty()", true };
            yield return new object[] { "'a'.length() = 1", true };
            yield return new object[] { "'abcd'.length() = 4", true };

            // function toChars() : collection
            yield return new object[] { "{}.toChars().empty()", true };
            yield return new object[] { "'a'.toChars() = ('a')", true };
            yield return new object[] { "'abc'.toChars() = ('a' | 'b' | 'c')", true };
        }

        public static IEnumerable<object[]> MathFunctionTestcases()
        {
            // function abs() : Integer | Decimal | Quantity
            yield return new object[] { "{}.abs().empty()", true };
            yield return new object[] { "(-5).abs() = 5", true };
            yield return new object[] { "(-5.5).abs() = 5.5", true };
            yield return new object[] { "(5.5 'mg').abs() = (5.5 'mg')", true };
            yield return new object[] { "(-5.5 'mg').abs() = (5.5 'mg')", true };

            // function ceiling() : Integer
            yield return new object[] { "{}.ceiling().empty()", true };
            yield return new object[] { "1.ceiling() = 1", true };
            yield return new object[] { "1.1.ceiling() = 2", true };
            yield return new object[] { "(-1.1).ceiling() = -1", true };
            //yield return new object[] { "(1 | 2).ceiling()", true }; // should throw an error
            //yield return new object[] { "'a'.ceiling()", true }; // should throw an error

            // function exp() : Decimal
            yield return new object[] { "{}.exp().empty()", true };
            yield return new object[] { "0.exp() = 1.0", true };
            yield return new object[] { "(-0.0).exp() = 1.0", true };
            //yield return new object[] { "(1 | 2).exp()", true }; // should throw an error
            //yield return new object[] { "'a'.exp()", true }; // should throw an error

            // function floor() : Integer
            yield return new object[] { "{}.floor().empty()", true };
            yield return new object[] { "1.floor() = 1", true };
            yield return new object[] { "1.1.floor() = 1", true };
            yield return new object[] { "2.9.floor() = 2", true };
            yield return new object[] { "(-2.1).floor() = -3", true };
            //yield return new object[] { "(1 | 2).floor()", true }; // should throw an error
            //yield return new object[] { "'a'.floor()", true }; // should throw an error

            // function ln() : Decimal
            yield return new object[] { "{}.ln().empty()", true };
            yield return new object[] { "1.ln() = 0.0", true };
            yield return new object[] { "1.0.ln() = 0.0", true };
            //yield return new object[] { "(1 | 2).ln()", true }; // should throw an error
            //yield return new object[] { "'a'.ln()", true }; // should throw an error

            // function log(base : Decimal) : Decimal
            yield return new object[] { "{}.log(1).empty()", true };
            yield return new object[] { "{}.log({}).empty()", true };
            yield return new object[] { "1.log({}).empty()", true };
            yield return new object[] { "16.log(2) = 4.0", true };
            yield return new object[] { "100.0.log(10.0) = 2.0", true };
            //yield return new object[] { "(1 | 2).log(2)", true }; // should throw an error
            //yield return new object[] { "'a'.log(2)", true }; // should throw an error

            // function power(exponent : Integer | Decimal) : Integer | Decimal
            yield return new object[] { "{}.power(1).empty()", true };
            yield return new object[] { "{}.power({}).empty()", true };
            yield return new object[] { "1.power({}).empty()", true };
            yield return new object[] { "2.power(3) = 8", true };
            yield return new object[] { "2.5.power(2) = 6.25", true };
            yield return new object[] { "(-1).power(0.5).empty()", true };
            // yield return new object[] { "(1 | 2).power(2)", true }; // should throw an error
            // yield return new object[] { "'a'.power(2)", true }; // should throw an error

            // function round([precision : Integer]) : Decimal
            yield return new object[] { "{}.round().empty()", true };
            yield return new object[] { "1.round() = 1", true };
            yield return new object[] { "1.5.round(0) = 2", true };
            yield return new object[] { "1.5.round() = 2", true };
            yield return new object[] { "3.14159.round(3) = 3.142", true };
            //yield return new object[] { "1.0.round(-1)", true }; // should throw an error
            // yield return new object[] { "(1 | 2).round(2)", true }; // should throw an error
            // yield return new object[] { "'a'.round(2)", true }; // should throw an error

            // function sqrt() : Decimal
            yield return new object[] { "{}.sqrt().empty()", true };
            yield return new object[] { "(-1).sqrt().empty()", true };
            yield return new object[] { "81.sqrt() = 9.0", true };
            yield return new object[] { "9.0.sqrt() = 3.0", true };
            // yield return new object[] { "(1 | 2).sqrt()", true }; // should throw an error
            // yield return new object[] { "'a'.sqrt()", true }; // should throw an error

            // function truncate() : Integer
            yield return new object[] { "{}.truncate().empty()", true };
            yield return new object[] { "101.truncate() = 101", true };
            yield return new object[] { "1.00000001.truncate() = 1", true };
            yield return new object[] { "(-1.56).truncate() = -1", true };
            //yield return new object[] { "(1 | 2).truncate()", true }; // should throw an error
            //yield return new object[] { "'a'.truncate()", true }; // should throw an error
        }

        public static IEnumerable<object[]> AllFunctionTestcases()
        {
            return
                Enumerable.Empty<object[]>()
                //.Union(ExistenceFunctionTestcases())
                //.Union(FilteringAndProjectionFunctionTestcases())
                //.Union(SubsettingFunctionTestcases())
                //.Union(CombiningFunctionTestcases())
                //.Union(ConversionFunctionTestcases())
                // .Union(StringManipulationFunctionTestcases())
                .Union(MathFunctionTestcases())
                ;
        }

        [DataTestMethod]
        [DynamicData(nameof(AllFunctionTestcases), DynamicDataSourceType.Method)]
        public void AssertTestcases(string expression, bool expected)
        {
            var dummy = ElementNode.ForPrimitive(true);
            Assert.IsTrue(Hl7.FhirPath.IValueProviderFPExtensions.IsBoolean(dummy, expression, expected));
        }
    }
}
