using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firely.Fhir.Packages.Tests
{
    [TestClass]
    public class Packaging
    {
        [TestMethod]
        public void FolderOrganization()
        {
            var file = 
                new FileEntry{ FilePath = @"C:\random\project\subfolder\subfolder\myresource.txt" }
                .MakeRelativePath(@"C:\random\project\")
                .OrganizeToPackageStructure();

            Assert.AreEqual(@"package\other\myresource.txt", file.FilePath);


            file = 
                new FileEntry { FilePath = @"C:\random\project\subfolder\subfolder\patient.xml" }
                .MakeRelativePath(@"C:\random\project\")
                .OrganizeToPackageStructure();

            Assert.AreEqual(@"package\patient.xml", file.FilePath);

        }
    }
}
