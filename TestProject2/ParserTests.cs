using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S3MParser;

namespace TestProject2
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    [DeploymentItem("01-v-drac.s3m")]
    public class ParserTests
    {
        public ParserTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

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
