namespace S3M.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    [DeploymentItem("test.s3m")]
    [DeploymentItem("abc.s3m")]
    [DeploymentItem("sd1.s3m")]
    [DeploymentItem("sd.s3m")]
    [DeploymentItem("d04.s3m")]
    [DeploymentItem("longname.s3m")]
    public class ParserTests
    {
        [TestMethod]
        public void TestParse()
        {
            var _ = S3MFile.Parse("test.s3m");
        }

        [TestMethod]
        public void TestSongName()
        {
            Assert.AreEqual<string>("test", S3MFile.Parse("test.s3m").Name);
        }

        [TestMethod]
        public void TestMaxSongName()
        {
            Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXY", S3MFile.Parse("longname.s3m").Name);
        }

        [TestMethod]
        public void TestFileType()
        {
            Assert.AreEqual<int>(16, S3MFile.Parse("test.s3m").Type);
        }

        [TestMethod]
        public void TestOrderCount()
        {
            Assert.AreEqual<int>(4, S3MFile.Parse("test.s3m").OrderCount);
        }

        [TestMethod]
        public void TestInstrumentCount()
        {
            Assert.AreEqual(3, S3MFile.Parse("test.s3m").InstrumentCount);
        }

        [TestMethod]
        public void TestPatternCount()
        {
            Assert.AreEqual<int>(2, S3MFile.Parse("test.s3m").PatternCount);
        }

        [TestMethod]
        public void TestNoteDelay()
        {
            Assert.AreEqual(CommandType.Notedelay, S3MFile.Parse("sd1.s3m").Patterns[0].Rows[0].ChannelEvents[0].Command);
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
