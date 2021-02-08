using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Specification;
using System;

namespace Hl7.Fhir.Utility.Tests
{
    [TestClass]
    public class FhirVersionTests
    {
        [TestMethod]
        public void TestFhirReleaseFromVersion()
        {           
            Assert.AreEqual(FhirRelease.DSTU1, FhirReleaseParser.Parse("0.01"));
            Assert.AreEqual(FhirRelease.DSTU1, FhirReleaseParser.Parse("0.11"));
            Assert.AreEqual(FhirRelease.DSTU1, FhirReleaseParser.Parse("0.0.80"));

            Assert.AreEqual(FhirRelease.DSTU2, FhirReleaseParser.Parse("0.4.0"));
            Assert.AreEqual(FhirRelease.DSTU2, FhirReleaseParser.Parse("1.0.2"));

            Assert.AreEqual(FhirRelease.STU3, FhirReleaseParser.Parse("1.8.0"));
            Assert.AreEqual(FhirRelease.STU3, FhirReleaseParser.Parse("3.0.0"));
            Assert.AreEqual(FhirRelease.STU3, FhirReleaseParser.Parse("3.0.2"));

            Assert.AreEqual(FhirRelease.R4, FhirReleaseParser.Parse("3.5a.0"));
            Assert.AreEqual(FhirRelease.R4, FhirReleaseParser.Parse("3.6.0"));
            Assert.AreEqual(FhirRelease.R4, FhirReleaseParser.Parse("4.0.0"));
            Assert.AreEqual(FhirRelease.R4, FhirReleaseParser.Parse("4.0.1"));

            Assert.AreEqual(FhirRelease.R5, FhirReleaseParser.Parse("4.2.0"));
            Assert.AreEqual(FhirRelease.R5, FhirReleaseParser.Parse("4.5.0"));
            Assert.AreEqual(FhirRelease.R5, FhirReleaseParser.Parse("5.0.0"));
        }

        [TestMethod]
        public void TestsFhirVersionFromRelease()
        {            
            Assert.AreEqual("0.0.82", FhirReleaseParser.FhirVersionFromRelease(FhirRelease.DSTU1));
            Assert.AreEqual("1.0.2", FhirReleaseParser.FhirVersionFromRelease(FhirRelease.DSTU2));
            Assert.AreEqual("3.0.2", FhirReleaseParser.FhirVersionFromRelease(FhirRelease.STU3));
            Assert.AreEqual("4.0.1", FhirReleaseParser.FhirVersionFromRelease(FhirRelease.R4));
            Assert.AreEqual("4.5.0", FhirReleaseParser.FhirVersionFromRelease(FhirRelease.R5));
        }

        [TestMethod]
        public void TestsMimeVersionFromRelease()
        {
            Assert.AreEqual("0.0", FhirReleaseParser.MimeVersionFromFhirRelease(FhirRelease.DSTU1));
            Assert.AreEqual("1.0", FhirReleaseParser.MimeVersionFromFhirRelease(FhirRelease.DSTU2));
            Assert.AreEqual("3.0", FhirReleaseParser.MimeVersionFromFhirRelease(FhirRelease.STU3));
            Assert.AreEqual("4.0", FhirReleaseParser.MimeVersionFromFhirRelease(FhirRelease.R4));
            Assert.AreEqual("5.0", FhirReleaseParser.MimeVersionFromFhirRelease(FhirRelease.R5));
        }

        [TestMethod]
        public void TestsFhirVersionFromMimeVersion()
        {
            Assert.AreEqual(FhirRelease.DSTU1, FhirReleaseParser.FhirReleaseFromMimeVersion("0.0"));
            Assert.AreEqual(FhirRelease.DSTU2, FhirReleaseParser.FhirReleaseFromMimeVersion("1.0"));
            Assert.AreEqual(FhirRelease.STU3, FhirReleaseParser.FhirReleaseFromMimeVersion("3.0"));
            Assert.AreEqual(FhirRelease.R4, FhirReleaseParser.FhirReleaseFromMimeVersion("4.0"));
            Assert.AreEqual(FhirRelease.R5, FhirReleaseParser.FhirReleaseFromMimeVersion("5.0"));

        }



    }
}
