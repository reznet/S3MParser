using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S3M;
using System.Globalization;

namespace TestProject2
{
    [TestClass]
    public class CommandAndInfoTests
    {
        [TestMethod]
        public void EmptyEffectLetter()
        {
            CreateAndVerify("@AB", CommandType.None, 0xA, 0xB);
        }

        [TestMethod]
        public void Axx()
        {
            CreateAndVerify("A10", CommandType.SetSpeed, 0x10, 0);
        }

        [TestMethod]
        public void Bxx()
        {
            CreateAndVerify("BAA", CommandType.JumpToOrder, 0xAA, 0);
        }

        [TestMethod]
        public void EFx()
        {
            CreateAndVerify("EF1", CommandType.FinePitchSlideDown, 1, 0);
        }

        [TestMethod]
        public void EEx()
        {
            CreateAndVerify("EE1", CommandType.ExtraFinePitchSlideDown, 1, 0);
        }

        [TestMethod]
        public void Exx()
        {
            CreateAndVerify("EAB", CommandType.PitchSlideDown, 0xAB, 0);
        }

        [TestMethod]
        public void FFx()
        {
            CreateAndVerify("FF1", CommandType.FinePitchSlideUp, 1, 0);
        }

        [TestMethod]
        public void FEx()
        {
            CreateAndVerify("FE1", CommandType.ExtraFinePitchSlideUp, 1, 0);
        }

        [TestMethod]
        public void Fxx()
        {
            CreateAndVerify("FBA", CommandType.PitchSlideUp, 0xBA, 0);
        }

        private static void CreateAndVerify(string commandAndInfo, CommandType expectedCommand, int expectedX, int expectedY)
        {
            var result = CreateCommandAndInfo(commandAndInfo);

            Assert.AreEqual(expectedCommand, result.Commmand, "wrong command type");
            Assert.AreEqual(expectedX, result.X, "wrong X");
            Assert.AreEqual(expectedY, result.Y, "wrong Y");
        }

        private static CommandAndInfo CreateCommandAndInfo(string commandAndInfo)
        {
            return CommandAndInfo.Create((byte)(commandAndInfo[0] - 'A' + 1), byte.Parse(commandAndInfo.Substring(1), NumberStyles.HexNumber));
        }
    }
}
