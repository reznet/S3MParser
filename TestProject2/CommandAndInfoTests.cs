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
        public void Axx()
        {
            CreateAndVerify("A10", CommandType.SetSpeed, 16, 0);
        }

        [TestMethod]
        public void Bxx()
        {
            CreateAndVerify("BAA", CommandType.JumpToOrder, 170, 0);
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
            return CommandAndInfo.Create(byte.Parse(commandAndInfo.Substring(0, 1), NumberStyles.HexNumber), byte.Parse(commandAndInfo.Substring(1), NumberStyles.HexNumber));
        }
    }
}
