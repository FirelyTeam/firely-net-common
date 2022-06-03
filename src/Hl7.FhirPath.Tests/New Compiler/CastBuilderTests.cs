using Firely.FhirPath;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#nullable enable

namespace HL7.FhirPath.Tests.NewCompiler
{
    [TestClass]
    public class CastBuilderTests
    {
        [TestMethod]
        public void TestValueConverter()
        {
            test(Expression.Constant(3), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            test(Expression.Constant(3L), typeof(int));
            test(Expression.Constant(3), typeof(string));
            //test(Expression.Constant(3), typeof(long?), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            //test(Expression.Constant(3, typeof(int?)), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
        }

        [TestMethod]
        public void TestIdentity()
        {
            test(Expression.Constant(3L), typeof(long), typeof(ConstantExpression), ExpressionType.Constant);
        }

        [TestMethod]
        public void TestToCollection()
        {
            var result = test(Expression.Constant(3), typeof(List<long>), typeof(SingleToCollectionExpression), ExpressionType.Extension);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Complexity.Should().Be(4);  // int -> long, wrap in collection
            var stce = sr.Expression.Should().BeOfType<SingleToCollectionExpression>().Subject;
            serialize(stce.SourceElement).Should().Be(serialize(success(build(Expression.Constant(3), typeof(long))).Expression));

            test(Expression.Constant(3), typeof(List<string>));

        }

        [TestMethod]
        public void TestFromCollection()
        {
            var result = test(Expression.Constant(new[] { 4 }), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Complexity.Should().Be(4);  // unwrap, int -> long
            var ue = sr.Expression.Should().BeOfType<UnaryExpression>().Subject;
            ue.Operand.Should().BeOfType<CollectionToSingleExpression>();

            compile<long>(ue)().Should().Be(4L);

            test(Expression.Constant(new[] { 4 }), typeof(string));
        }

        [TestMethod]
        public void TestInterCollection()
        {
            var source = Expression.Constant(new[] { 4 });
            var result = test(source, typeof(List<long>), typeof(CollectionToCollectionExpression), ExpressionType.Extension);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Complexity.Should().Be(4);  // unwrap, int -> long, wrap
            var ctc = sr.Expression.Should().BeOfType<CollectionToCollectionExpression>().Subject;
            ctc.SourceList.Should().Be(source);

            var converter = ".Lambda #Lambda1<System.Func`2[System.Int32,System.Int64]>(System.Int32 $var1) {" + Environment.NewLine +
                $"    #(System.Int64)$var1{Environment.NewLine}}}";

            serialize(ctc.Converter).Should().Be(converter);

            test(Expression.Constant(new[] { 4 }), typeof(List<string>));
        }


        [TestMethod]
        public void TestNewGpAssignment()
        {
            var gpX = Type.MakeGenericMethodParameter(0);

            var result = CastStepBuilding.BuildCast(typeof(long), gpX, new GenericParamAssignments());
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Expression.Should().BeOfType<ConstantExpression>();
            sr.Assignments.Should().HaveCount(1);
            sr.Assignments[gpX].Should().Be(typeof(long));
        }

        [TestMethod]
        public void TestGpAssignmentWithConversion()
        {
            var gpX = Type.MakeGenericMethodParameter(0);
            var gpa = new GenericParamAssignments() { [gpX] = typeof(long) };

            // Now, given generic param X is a "long", pass it a "int32" param. This should be successful, resulting
            // in a conversion
            var result = CastStepBuilding.BuildCast(typeof(int), gpX, gpa);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            var conversion = sr.Expression.Should().BeOfType<UnaryExpression>().Subject;
            conversion.NodeType.Should().Be(ExpressionType.ConvertChecked);
            sr.Assignments.Should().HaveCount(1);
            sr.Assignments[gpX].Should().Be(typeof(long));
        }


        [TestMethod]
        public void TestGpAssignmentWithRestart()
        {
            var gpX = Type.MakeGenericMethodParameter(0);
            var gpa = new GenericParamAssignments() { [gpX] = typeof(long) };

            // Now, face the cast builder with the situation where generic param X is already "long", and
            // try to pass a string argument into that long parameter. It should suggest a restart with
            // the param X rebound to a string.
            var result = CastStepBuilding.BuildCast(typeof(string), gpX, gpa);
            var restart = result.Should().BeOfType<StepBuildResult<Expression>.Restart>().Subject;
            restart.Suggestion[gpX].Should().Be(typeof(string));
        }


        [TestMethod]
        public void TestGpAssignments()
        {
            var gpX = Type.MakeGenericMethodParameter(0);
            var gpY = Type.MakeGenericMethodParameter(1);

            var s1 = success(test((typeof(int), gpX), (typeof(ICollection<int>), gpX), (typeof(string), gpY)));
            s1.Assignments[gpX].Should().Be(typeof(int));
            s1.Assignments[gpY].Should().Be(typeof(string));
            s1.Assignments.Count.Should().Be(2);

            var s2 = success(test((typeof(ICollection<int>), typeof(ICollection<>).MakeGenericType(gpX)),
                    (typeof(long), gpX)));
            s2.Assignments[gpX].Should().Be(typeof(long));

            var s3 = test((typeof(int), typeof(ICollection<>).MakeGenericType(gpX)),
                    (typeof(string), gpX));
            s3.IsFail.Should().BeTrue();

        }

        private static StepBuildResult<IEnumerable<Expression>> test(params (Type, Type)[] casts)
        {
            var casts2 = casts.Select(t => Tuple.Create<Expression, Type>(Expression.Parameter(t.Item1), t.Item2)).ToArray();
            return CastStepBuilding.BuildCastMany(casts2, new GenericParamAssignments());
        }


        [TestMethod]
        public void TestLambda()
        {
            var s1 = success(build(Expression.Parameter(typeof(Func<int, int>)), typeof(Func<long, long>)));
            var cwe = s1.Expression.Should().BeOfType<CallWrapperExpression>().Subject;
            s1.Complexity.Should().Be(6);

            cwe.Parameters[0].Should().BeOfType<UnaryExpression>()
                .Subject.NodeType.Should().Be(ExpressionType.ConvertChecked);
            cwe.Parameters[1].Should().BeOfType<UnaryExpression>()
                .Subject.NodeType.Should().Be(ExpressionType.ConvertChecked);
        }

        [TestMethod]
        public void TestMatchMethod()
        {
            var method = typeof(ITestFunctions).GetMethods()[0];
            var correct = new[] { Expression.Constant(4), Expression.Constant(5L), Expression.Constant(true) };

            MethodMatcher.MatchMethod(method, "DoIt", correct).IsSuccess.Should().BeTrue();

            MethodMatcher.MatchMethod(method, "DoIt", new[] { Expression.Constant(4), Expression.Constant("hi!"), Expression.Constant(true) })
                .IsSuccess.Should().BeFalse();

            MethodMatcher.MatchMethod(method, "DoIt", new[] { Expression.Constant(4), Expression.Constant(5L) })
                .IsSuccess.Should().BeFalse();

            MethodMatcher.MatchMethod(method, "DoIt!", correct).IsSuccess.Should().BeFalse();
        }

        [TestMethod]
        public void TestMatchMethods()
        {
            var methods = typeof(ITestFunctions).GetMethods();

            // correct, but needs casts
            var correct1 = MethodMatcher.MatchMethods(methods, "DoIt", new[] { Expression.Constant(4), Expression.Constant(5L), Expression.Constant(true) });
            var match = correct1.Should().BeOfType<StepBuildResult<MethodMatch>.Success>().Subject;

            // correct, more direct match
            var correct2 = MethodMatcher.MatchMethods(methods, "DoIt", new[] { Expression.Constant(4), Expression.Constant(5), Expression.Constant(true) });
            var match2 = correct2.Should().BeOfType<StepBuildResult<MethodMatch>.Success>().Subject;

            match2.Complexity.Should().BeLessThan(match.Complexity);

            // no match found
            var wrong = MethodMatcher.MatchMethods(methods, "DoIt", new[] { Expression.Constant("Hi!"), Expression.Constant(5L), Expression.Constant(true) });
            wrong.IsFail.Should().BeTrue();
        }

        private interface ITestFunctions
        {
            U DoIt<U>(ICollection<U> x, U y, bool z);
            bool DoIt(ICollection<int> x, int y, bool q);
        }

        private static StepBuildResult<T>.Success success<T>(StepBuildResult<T> r) =>
            r.Should().BeOfType<StepBuildResult<T>.Success>().Subject;

        private static string serialize(Expression e)
        {
            var dvp = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic)!;
            return (string)dvp.GetValue(e)!;
        }

        private StepBuildResult<ExpressionGenerator> build(Expression source, Type target, GenericParamAssignments? gpa = null) =>
            CastStepBuilding.BuildCast(source, target, gpa ?? new GenericParamAssignments());

        private StepBuildResult<ExpressionGenerator> test(Expression source, Type target, Type? converterType = null, ExpressionType? nodeType = null)
        {
            var parameterAssignments = new GenericParamAssignments();
            var result = CastStepBuilding.BuildCast(source.Type, target, parameterAssignments);

            if (converterType is not null)
            {
                result.IsSuccess.Should().BeTrue();
                var sr = (StepBuildResult<ExpressionGenerator>.Success)result;
                var generated = sr.Expression.Item.Invoke(source);
                generated.Type.Should().Be(target);
                generated.GetType().Should().BeAssignableTo(converterType);
                generated.NodeType.Should().Be(nodeType);
            }
            else
                result.IsFail.Should().BeTrue();

            return result;
        }

        private Func<U> compile<U>(Expression body) =>
            Expression.Lambda<Func<U>>(body).Compile();
    }
}
