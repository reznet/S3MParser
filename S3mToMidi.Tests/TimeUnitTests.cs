using System.Reflection;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class TimeUnitTests
{
    [TestMethod]
    public void TwoWholeRestsInFourFour()
    {
        Time time= new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetBarlineTies(96 * 4 * 2);

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(384, ties[0], "wrong first tie");
        Assert.AreEqual(384, ties[1], "wrong second tie");
    }

    [TestMethod]
    public void WholeRestTiedToQuarterNoteInFourFour()
    {
        Time time= new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetBarlineTies(96 * 5);

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(384, ties[0], "wrong first tie");
        Assert.AreEqual(96, ties[1], "wrong second tie");
    }

    [TestMethod]
    public void QuarterNoteInFourFour()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(96);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(96, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void WholeNoteInFourFour()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Time.TICKS_PER_QUARTERNOTE * 4);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Time.TICKS_PER_QUARTERNOTE * 4, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void DottedHalfNoteInFourFour()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(Time.TICKS_PER_QUARTERNOTE * 3);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(Time.TICKS_PER_QUARTERNOTE * 3, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void DottedQuarterStartingOnAnd()
    {
        // arrange
        Time time = new Time();
        time.SetTimeSignature(4,4);
        time.AddTime(Time.TICKS_PER_QUARTERNOTE / 2);

        // act
        var ties = time.GetNoteTies(Time.TICKS_PER_QUARTERNOTE * 3 / 2);

        // assert
        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Time.TICKS_PER_QUARTERNOTE / 2, ties[0], "expected eighth note for first tie");
        Assert.AreEqual(Time.TICKS_PER_QUARTERNOTE * 1, ties[1], "expected quarter note for second tie");
    }

    [TestMethod]
    public void BzamPattern47Channel1()
    {
        Time time = new Time();
        time.SetTimeSignature(5, 8);
        time.AddTime(240);
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(684);

        Assert.AreEqual(1, ties.Length, "wrong number of ties");
        Assert.AreEqual(96, ties[0], "wrong quarter note duration");
    }

    [TestMethod]
    public void DracPattern04Channel2BarTies()
    {
        const int duration32 = Time.TICKS_PER_QUARTERNOTE / 8;
        const int durationWholeNote = Time.TICKS_PER_QUARTERNOTE * 4;
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(duration32);
        
        // 32nd note rest + 2 whole notes minus 2 32nd notes = 32 + 30 32nd notes
        var ties = time.GetBarlineTies((durationWholeNote * 2) - (duration32 * 2));

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(durationWholeNote - duration32, ties[0], "wrong duration of note in first measure");
        Assert.AreEqual(durationWholeNote - duration32, ties[1], "wrong duration of note in second measure");
    }

    [TestMethod]
    public void DracPattern04Channel2FirstMeasure()
    {
        const int duration32 = Time.TICKS_PER_QUARTERNOTE / 8;
        const int duration16 = Time.TICKS_PER_QUARTERNOTE / 4;
        const int duration8 = Time.TICKS_PER_QUARTERNOTE / 2;
        const int duration4 = Time.TICKS_PER_QUARTERNOTE / 1;
        const int duration2 = Time.TICKS_PER_QUARTERNOTE * 2;
        const int durationWholeNote = Time.TICKS_PER_QUARTERNOTE * 4;
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(duration32);

        var ties = time.GetNoteTies(durationWholeNote - duration32);

        Assert.AreEqual(5, ties.Length, "wrong number of ties");
        Assert.AreEqual(duration32, ties[0]);
        Assert.AreEqual(duration16, ties[1]);
        Assert.AreEqual(duration8, ties[2]);
        Assert.AreEqual(duration4, ties[3]);
        Assert.AreEqual(duration2, ties[4]);
    }

    [TestMethod]
    public void DracPattern04Channel2SecondMeasure()
    {
        const int duration32 = Time.TICKS_PER_QUARTERNOTE / 8;
        const int duration16 = Time.TICKS_PER_QUARTERNOTE / 4;
        const int duration8 = Time.TICKS_PER_QUARTERNOTE / 2;
        const int duration4 = Time.TICKS_PER_QUARTERNOTE / 1;
        const int duration2 = Time.TICKS_PER_QUARTERNOTE * 2;
        const int durationWholeNote = Time.TICKS_PER_QUARTERNOTE * 4;
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        var ties = time.GetNoteTies(durationWholeNote - duration32);

        Assert.AreEqual(5, ties.Length, "wrong number of ties");
        Assert.AreEqual(duration2, ties[0]);
        Assert.AreEqual(duration4, ties[1]);
        Assert.AreEqual(duration8, ties[2]);
        Assert.AreEqual(duration16, ties[3]);
        Assert.AreEqual(duration32, ties[4]);
    }

    [TestMethod]
    public void GetSubdivisionCells_EighthNote_DottedQuarterStartingOnAnd()
    {
        // arrange

        // act
        var subdivisionCells = Time.GetSubdivisionCells(3, Time.TICKS_PER_QUARTERNOTE / 2, Time.TICKS_PER_QUARTERNOTE * 3 / 2);

        // assert
        CollectionAssert.AreEqual(new bool[]{ false, true, true, true, false, false, false, false}, subdivisionCells);
    }
}
