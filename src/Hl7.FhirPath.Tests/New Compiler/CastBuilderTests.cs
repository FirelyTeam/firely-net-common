using Firely.FhirPath;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace HL7.FhirPath.Tests.NewCompiler
{
    [TestClass]
    public class CastBuilderTests
    {
        [TestMethod]
        public void TestValueConverter()
        {
            test(Expression.Constant(3), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            test(Expression.Constant(3L), typeof(int), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            test(Expression.Constant(3), typeof(string));
            test(Expression.Constant(3), typeof(long?), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            test(Expression.Constant(3, typeof(int?)), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
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
            sr.Complexity.Should().Be(2);  // int -> long, wrap in collection
            var stce = sr.Expression.Should().BeOfType<SingleToCollectionExpression>().Subject;
            serialize(stce.SourceElement).Should().Be(serialize(build(Expression.Constant(3), typeof(long))));

            test(Expression.Constant(3), typeof(List<string>));
        }

        [TestMethod]
        public void TestFromCollection()
        {
            var result = test(Expression.Constant(new[] { 4 }), typeof(long), typeof(UnaryExpression), ExpressionType.ConvertChecked);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Complexity.Should().Be(2);  // unwrap, int -> long
            var ue = sr.Expression.Should().BeOfType<UnaryExpression>().Subject;
            ue.Operand.Should().BeOfType<CollectionToSingleExpression>();

            test(Expression.Constant(new[] { 4 }), typeof(string));
        }

        [TestMethod]
        public void TestInterCollection()
        {
            var source = Expression.Constant(new[] { 4 });
            var result = test(source, typeof(List<long>), typeof(CollectionToCollectionExpression), ExpressionType.Extension);
            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Complexity.Should().Be(3);  // unwrap, int -> long, wrap
            var ctc = sr.Expression.Should().BeOfType<CollectionToCollectionExpression>().Subject;
            ctc.SourceList.Should().Be(source);

            var converter = ".Lambda #Lambda1<System.Func`2[System.Int32,System.Int64]>(System.Int32 $var1) {" + Environment.NewLine +
                $"    #(System.Int64)$var1{Environment.NewLine}}}";

            serialize(ctc.Converter).Should().Be(converter);

            test(Expression.Constant(new[] { 4 }), typeof(List<string>));
        }

        private static string serialize(Expression e)
        {
            var dvp = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return dvp.GetValue(e) as string;
        }

        private Expression build(Expression source, Type target)
        {
            var result = CastStepBuilding.BuildCast(Expression.Constant(3), typeof(long), new GenericParamAssignments());
            return result switch
            {
                StepBuildResult<Expression>.Success s => s.Expression,
                _ => throw new AssertFailedException()
            };
        }


        private StepBuildResult<Expression> test(Expression source, Type target, Type converterType = null, ExpressionType? nodeType = null)
        {
            var parameterAssignments = new GenericParamAssignments();
            var result = CastStepBuilding.BuildCast(source, target, parameterAssignments);

            if (converterType is not null)
            {
                result.IsSuccess.Should().BeTrue();
                var sr = (StepBuildResult<Expression>.Success)result;
                sr.Expression.Type.Should().Be(target);
                sr.Expression.GetType().Should().BeAssignableTo(converterType);
                sr.Expression.NodeType.Should().Be(nodeType);
            }
            else
                result.IsFail.Should().BeTrue();

            return result;
        }

        private Func<U> compile<U>(Expression body) =>
            Expression.Lambda<Func<U>>(body).Compile();
    }
}
