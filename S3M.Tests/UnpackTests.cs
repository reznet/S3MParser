namespace S3M.Tests
{
    [TestClass]
    public class UnpackTests
    {
        [TestMethod]
        public void ReadShortAsIntWithFirstByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)1, (byte)0 }, 1);
        }

        [TestMethod]
        public void ReadShortAsIntWithSecondByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)0, (byte)1 }, 16);
        }

        [TestMethod]
        public void ReadByteAsInt()
        {
            ReadBytesAndAssertExpected(new[] { (byte)1 }, 1);
        }

        [TestMethod]
        public void ReadLongAsIntWithFirstByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)1, (byte)0, (byte)0, (byte)0 }, 1 << 4);
        }

        [TestMethod]
        public void ReadLongAsIntWithSecondByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)0, (byte)1, (byte)0, (byte)0 }, 1 << 0);
        }

        [TestMethod]
        public void ReadLongAsIntWithThirdByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)0, (byte)0, (byte)1, (byte)0 }, 1 << 12);
        }

        [TestMethod]
        public void ReadLongAsIntWithFourthByteSet()
        {
            ReadBytesAndAssertExpected(new[] { (byte)0, (byte)0, (byte)0, (byte)1 }, 1 << 8);
        }

        private void ReadBytesAndAssertExpected(byte[] bytes, int expected)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(memoryStream))
                {
                    var actual = reader.ReadBytesAsInt(bytes.Length);
                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
