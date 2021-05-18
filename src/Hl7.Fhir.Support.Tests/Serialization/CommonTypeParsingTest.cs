/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hl7.Fhir.Support.Tests.Serialization
{
    [TestClass]
    public class CommonTypeParsingTest
    {
        [TestMethod]
        public void CanConvertPocoToTypedElement()
        {
            Coding c = new Coding("http://nu.nl", "bla");
            var te = TypedSerialization.ToTypedElement(c);
            Assert.AreEqual("Coding", te.InstanceType);

            Coding c2 = TypedSerialization.ToPoco<Coding>(te);

            Assert.AreEqual(c.Code, c2.Code);
            Assert.AreEqual(c.System, c2.System);
        }

        [TestMethod]
        public void CanConvertPocoToSourceNode()
        {
            Coding c = new Coding("http://nu.nl", "bla");
            var sn = TypedSerialization.ToSourceNode(c, "kode");
            Assert.AreEqual("kode", sn.Name);

            Coding c2 = TypedSerialization.ToPoco<Coding>(sn);

            Assert.AreEqual(c.Code, c2.Code);
            Assert.AreEqual(c.System, c2.System);
        }
    }
}
