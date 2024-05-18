﻿using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class GetNoteTiesUnitTests_SixFour
{
    [TestMethod]
    public void WholeNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.WholeNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0]);
    }

    [TestMethod]
    public void HalfNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.HalfNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.HalfNote, ties[0]);
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
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }

    [TestMethod]
    public void SixteenthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.SixteenthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixteenthNote, ties[0]);
    }

    [TestMethod]
    public void ThirtysecondNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.ThirtySecondNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.ThirtySecondNote, ties[0]);
    }

    [TestMethod]
    public void SixtyfourthNoteOnBeatOne()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        var ties = time.GetNoteTies(Durations.SixtyFourthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.SixtyFourthNote, ties[0]);
    }

    [TestMethod]
    [Ignore] // need support for tuplets
    public void SixtyfourthNoteOnBeatOne72()
    {
        Time time = new Time();
        time.SetTimeSignature(6, 4);
        time.AddTime(72);

        var ties = time.GetNoteTies(72);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(72, ties[0]);
    }
}
