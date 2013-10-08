using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using S3MParser;

namespace TestProject2
{
    /// <summary>
    /// Summary description for CommandTypeTests
    /// </summary>
    [TestClass]
    public class CommandTypeTests
    {
        public void SetSpeed()
        {
            byte command = (int)'A';

            Assert.AreEqual(CommandType.SetSpeed, CommandTypeExtensions.Parse(command));
        }
    }
}
