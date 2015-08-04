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
    [DeploymentItem("sd1.s3m")]
    [DeploymentItem("sd.s3m")]
    [DeploymentItem("d04.s3m")]
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

        [TestMethod]
        public void TestNoteDelay()
        {
            S3MFile file = S3MFile.Parse("sd1.s3m");
            Assert.AreEqual(CommandType.Notedelay, file.Patterns[0].Rows[0].ChannelEvents[0].Command);
        }

        [TestMethod]
        public void TestNoteDelay1()
        {
            S3MFile file = S3MFile.Parse("sd.s3m");
            Row row = null;

            // .00
            row = file.Patterns[0].Rows[0];
            Assert.AreEqual(CommandType.None, row.ChannelEvents[0].Command);

            // SD1
            row = file.Patterns[0].Rows[1];
            Assert.AreEqual(CommandType.Notedelay, row.ChannelEvents[0].Command);
            Assert.AreEqual(1, row.ChannelEvents[0].Data);

            // SD2
            row = file.Patterns[0].Rows[2];
            Assert.AreEqual(CommandType.Notedelay, row.ChannelEvents[0].Command);
            Assert.AreEqual(2, row.ChannelEvents[0].Data);
        }

        [TestMethod]
        public void TestVolumeSlideDown()
        {
            S3MFile file = S3MFile.Parse("d04.s3m");
            Assert.AreEqual(CommandType.VolumeSlideDown, file.Patterns[0].Rows[0].ChannelEvents[0].Command);
        }
    }
}
