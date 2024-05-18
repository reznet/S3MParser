using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class SubdivisionUnitTests_FourFour
{
    [TestMethod]
    public void WholeNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.WholeNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0]);
    }

    [TestMethod]
    public void HalfNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.HalfNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.HalfNote, ties[0]);
    }

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
    public void QuarterNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0]);
    }

    [TestMethod]
    public void EighthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }

    [TestMethod]
    public void SixteenthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.SixteenthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixteenthNote, ties[0]);
    }

    [TestMethod]
    public void ThirtysecondNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.ThirtySecondNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.ThirtySecondNote, ties[0]);
    }

    [TestMethod]
    public void SixtyfourthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.SixtyFourthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixtyFourthNote, ties[0]);
    }

    [TestMethod]
    public void WeirdThingAfterEighthAndThirtySecondNote()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.EighthNote + Durations.ThirtySecondNote);

        var ties = time.GetNoteTies(Durations.DottedQuarterNote + Durations.SixtyFourthNoteTriplet);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixtyFourthNote, ties[0]);
    }

    [TestMethod]
    public void WeirdThing()
    {
        Time time = new Time();
        time.SetTimeSignature(15, 8); // 720 ticks, eighth note = 48 ticks
        time.AddTime(608);

        var ties = time.GetNoteTies(112);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixtyFourthNote, ties[0]);
    }
}
