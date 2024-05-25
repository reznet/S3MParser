using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests;

[TestClass]
public class GetNoteTiesUnitTests_FourFour_Syncopated
{
    [TestMethod]
    public void HalfNoteOnBeatTwo()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.QuarterNote);

        var ties = time.GetNoteTies(Durations.HalfNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.HalfNote, ties[0]);
    }

    [TestMethod]
    public void QuarterNoteOnBeatOneAnd()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.EighthNote);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0]);
    }

    [TestMethod]
    public void QuarterNoteOnBeatTwoAnd()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.QuarterNote + Durations.EighthNote);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0]);
    }

    [TestMethod]
    public void QuarterNoteOnBeatThreeAnd()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.HalfNote + Durations.EighthNote);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0]);
    }

    [TestMethod]
    public void EighthNoteOnBeatOneE()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.SixteenthNote);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }

    [TestMethod]
    public void EighthNoteOnBeatOneA()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.DottedEighthNote);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(2, ties.Length, "duration should have been subdivided");
        Assert.AreEqual(Durations.SixteenthNote, ties[0]);
        Assert.AreEqual(Durations.SixteenthNote, ties[1]);
    }

    [TestMethod]
    public void EighthNoteOnBeatTwoE()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.QuarterNote + Durations.SixteenthNote);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }
    [TestMethod]
    public void EighthNoteOnBeatTwoA()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.QuarterNote + Durations.DottedEighthNote);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(2, ties.Length, "duration should have been subdivided");
        Assert.AreEqual(Durations.SixteenthNote, ties[0]);
        Assert.AreEqual(Durations.SixteenthNote, ties[1]);
    }
}
