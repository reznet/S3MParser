using System.Reflection;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class TimeUnitTests
{
    [TestMethod]
    public void TwoWholeNotesInFourFour()
    {
        Time time= new();
        time.SetTimeSignature(4, 4);

        var ties = time.GetBarlineTies(Durations.WholeNote * 2);

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0], "wrong first tie");
        Assert.AreEqual(Durations.WholeNote, ties[1], "wrong second tie");
    }

    [TestMethod]
    public void WholeNoteTiedToQuarterNoteInFourFour()
    {
        Time time= new();
        time.SetTimeSignature(4, 4);

        var ties = time.GetBarlineTies(Durations.QuarterNote * 5);

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0], "wrong first tie");
        Assert.AreEqual(Durations.QuarterNote, ties[1], "wrong second tie");
    }

    [TestMethod]
    public void QuarterNoteInFourFour()
    {
        Time time = new();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.QuarterNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void WholeNoteInFourFour()
    {
        Time time = new();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.WholeNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void DottedQuarterStartingOnAnd()
    {
        // arrange
        Time time = new();
        time.SetTimeSignature(4,4);
        time.AddTime(Durations.EighthNote);

        // act
        var ties = time.GetNoteTies(Durations.DottedQuarterNote);

        // assert
        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0], "expected eighth note for first tie");
        Assert.AreEqual(Durations.QuarterNote, ties[1], "expected quarter note for second tie");
    }

    [TestMethod]
    [Ignore] // we currently only support 4/4
    public void BzamPattern47Channel1()
    {
        Time time = new();
        time.SetTimeSignature(5, 8);
        time.AddTime(240);
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(684);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.QuarterNote, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void DracPattern04Channel2BarTies()
    {
        Time time = new();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.ThirtySecondNote);
        
        // 32nd note rest + 2 whole notes minus 2 32nd notes = 32 + 30 32nd notes
        var ties = time.GetBarlineTies((Durations.WholeNote * 2) - (Durations.ThirtySecondNote * 2));

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.WholeNote - Durations.ThirtySecondNote, ties[0], "wrong duration of note in first measure");
        Assert.AreEqual(Durations.WholeNote - Durations.ThirtySecondNote, ties[1], "wrong duration of note in second measure");
    }

    [TestMethod]
    public void DracPattern04Channel2FirstMeasure()
    {
        Time time = new();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.ThirtySecondNote);

        var ties = time.GetNoteTies(Durations.WholeNote - Durations.ThirtySecondNote);

        Assert.AreEqual(5, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.ThirtySecondNote, ties[0]);
        Assert.AreEqual(Durations.SixteenthNote, ties[1]);
        Assert.AreEqual(Durations.EighthNote, ties[2]);
        Assert.AreEqual(Durations.QuarterNote, ties[3]);
        Assert.AreEqual(Durations.HalfNote, ties[4]);
    }

    [TestMethod]
    public void DracPattern04Channel2SecondMeasure()
    {
        Time time = new();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Durations.WholeNote - Durations.ThirtySecondNote);

        Assert.AreEqual(5, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.HalfNote, ties[0]);
        Assert.AreEqual(Durations.QuarterNote, ties[1]);
        Assert.AreEqual(Durations.EighthNote, ties[2]);
        Assert.AreEqual(Durations.SixteenthNote, ties[3]);
        Assert.AreEqual(Durations.ThirtySecondNote, ties[4]);
    }

    [TestMethod]
    public void PtimePattern14Channel3Beat4And()
    {
        Time time = new();
        time.SetTimeSignature(6, 4);
        time.AddTime(Durations.WholeNote + Durations.HalfNote);

        var ties = time.GetNoteTies(Durations.EighthNote);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
    }
}
