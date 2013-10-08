using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S3M;

namespace TestProject2
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    [DeploymentItem("01-v-drac.s3m")]
    public class ParserTests
    {
        private readonly string vDracFilename = "01-v-drac.s3m";

        private S3MFile GetTestFile()
        {
            return S3MFile.Parse(vDracFilename);
        }

        [TestMethod]
        public void TestParse()
        {
            S3MFile.Parse(vDracFilename);
        }

        [TestMethod]
        public void TestFileName()
        {
            S3MFile file = GetTestFile();
            Assert.AreEqual<string>("good morning, dracula -jk", file.Name);
        }

        [TestMethod]
        public void TestFileType()
        {
            S3MFile file = GetTestFile();
            Assert.AreEqual<int>(16, file.Type);
        }

        [TestMethod]
        public void TestOrderCount()
        {
            S3MFile file = GetTestFile();
            Assert.AreEqual<int>(44, file.OrderCount);
        }

        [TestMethod]
        public void TestInstrumentCount()
        {
            S3MFile file = GetTestFile();
            Assert.AreEqual<int>(10, file.InstrumentCount);
        }

        [TestMethod]
        public void TestPatternCount()
        {
            S3MFile file = GetTestFile();
            Assert.AreEqual<int>(43, file.PatternCount);
        }
    }
}
