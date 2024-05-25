using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests;

[TestClass]
public class GetNoteTiesUnitTests_FourFour_Dotted
{
    [TestMethod]
    public void DottedHalfNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedHalfNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedHalfNote, ties[0]);
    }

    [TestMethod]
    public void DottedQuarterNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedQuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedHalfNote, ties[0]);
    }

    [TestMethod]
    public void DottedEighthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedEighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedEighthNote, ties[0]);
    }

    [TestMethod]
    public void DottedSixteenthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedSixteenthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedHalfNote, ties[0]);
    }

    [TestMethod]
    public void DottedThirtysecondNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedThirtySecondNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedThirtySecondNote, ties[0]);
    }

    [TestMethod]
    public void DottedSixtyfourthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.DottedSixtyFourthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.DottedSixtyFourthNote, ties[0]);
    }
}