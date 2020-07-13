using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Hl7.Fhir.Support.Utility.Result;

namespace Hl7.Fhir.Support.Utility.Tests
{
    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void ResultsTest()
        {
            var r = Ok(4);
            Assert.AreEqual(10, r + 6);

            Result<int> resultInt = 5;
            Assert.AreEqual(Ok(5), resultInt);

            var ex = new NotFiniteNumberException();
            var e = Fail<int>(ex);
            Assert.AreEqual(Fail<int>(ex),e);

            Assert.AreEqual(4, r.ValueOrThrow());
            Assert.ThrowsException<NotFiniteNumberException>(() => e.ValueOrThrow());

            Assert.AreEqual(8,e.ValueOrDefault(8));
            Assert.AreEqual(ex.Message.Length,e.ValueOrElse(e => e.Message.Length));

            Assert.AreEqual(Ok(true),resultInt.Chain(i => Ok(i*2).Chain(j => Ok(j > i))));
            Assert.AreEqual(e.Error, ((Fail<string>)e.Chain(_ => Ok($"Success!"))).Error);

            Assert.AreEqual(Ok(9),
                                from a in r
                                from b in resultInt
                                select a + b);

            Assert.AreEqual(e,
                                from a in r
                                from b in e
                                select a + b);

            Assert.AreEqual("Success: 5", resultInt.Handle(v => $"Success: {v}", e => $"Failed with: {e.Message}"));
            Assert.AreEqual("Failed with: " + ex.Message, e.Handle(v => $"Success: {v}", e => $"Failed with: {e.Message}"));

            Assert.AreEqual(Ok(5), r.Combine(resultInt));
            Assert.AreEqual(e, r.Combine(e));
            Assert.AreEqual(e, e.Combine(r));

            Assert.AreEqual(Ok(5), r & resultInt);
            Assert.AreEqual(e, r & e);
            Assert.AreEqual(e, e & r);

            Assert.IsTrue(Ok(5) == Ok(5));
            Assert.IsTrue(Ok(5) != Ok(6));
            Assert.IsTrue(Fail(ex) == Fail(ex));
            Assert.IsTrue(Fail(ex) != Fail(new ArgumentException()));
        }
    }
}