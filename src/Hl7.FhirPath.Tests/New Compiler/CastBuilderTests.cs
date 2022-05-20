using Firely.FhirPath;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace HL7.FhirPath.Tests.NewCompiler
{
    [TestClass]
    public class CastBuilderTests
    {
        [TestMethod]
        public void TestValueConverter()
        {
            var parameterAssignments = new GenericParamAssignments();
            var result = CastStepBuilding.ConvertValue(Expression.Constant(3), typeof(long), parameterAssignments);

            var sr = result.Should().BeOfType<StepBuildResult<Expression>.Success>().Subject;
            sr.Expression.Type.Should().Be(typeof(long));
            sr.Expression.NodeType.Should().Be(ExpressionType.Convert);

            compile<long>(sr.Expression)().Should().Be(3L);
        }

        private Func<U> compile<U>(Expression body) =>
            Expression.Lambda<Func<U>>(body).Compile();
    }
}
