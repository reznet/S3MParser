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
}
