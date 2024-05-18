﻿using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class SubdivisionUnitTests
{
    [TestMethod]
    public void FourFourWholeNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.WholeNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0]);
    }

    [TestMethod]
    public void FourFourHalfNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.HalfNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.HalfNote, ties[0]);
    }

    [TestMethod]
    public void FourFourQuarterNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0]);
    }

    [TestMethod]
    public void FourFourEighthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }

    [TestMethod]
    public void FourFourSixteenthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.SixteenthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixteenthNote, ties[0]);
    }

    [TestMethod]
    public void FourFourThirtysecondNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.ThirtySecondNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.ThirtySecondNote, ties[0]);
    }

    [TestMethod]
    public void FourFourSixtyfourthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.SixtyFourthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixtyFourthNote, ties[0]);
    }
}
