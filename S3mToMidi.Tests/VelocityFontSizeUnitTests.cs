using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]

public class VelocityFontSizeUnitTests
{
    [TestMethod]
    public void MinimumVelocity()
    {
        Assert.AreEqual(-7, Clef.GetFontSizeForVelocity(0));
    }

    [TestMethod]
    public void MinimumVelocityBucket()
    {
        Assert.AreEqual(-7, Clef.GetFontSizeForVelocity(1));
    }

    [TestMethod]
    public void MaximumVelocity()
    {
        Assert.AreEqual(0, Clef.GetFontSizeForVelocity(64));
    }

    [TestMethod]
    public void MaximumVelocityBucket()
    {
        Assert.AreEqual(0, Clef.GetFontSizeForVelocity(63));
    }
}
