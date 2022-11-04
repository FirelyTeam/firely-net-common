using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HL7.FhirPath.Tests.Tests
{
    [TestClass]
    public class CsharpGeneratorTests
    {
        [TestMethod]
        public void GenerateTest()
        {
            var compiler = new FhirPathCompiler();
            var parsed = compiler.Parse("a.where(b = 'bee').fun(4,c)");
            var result = parsed.ToCsharp();
        }

        /*
         * var x = new[] { 2, 3, 4 };

	        Func<int,int> z = @this => Let(@this,focus => func(focus,@this));
	
	        static R Let<F,R>(F t, Func<F,R> f) => f(t);
	
	        static int func(int a, int b) => a+b;
	
	        x.Select(j => z(j)).Dump();
        */
    }
}
