using System.Runtime.CompilerServices;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests;

[TestClass]
public class GetNoteTiesUnitTests_FourFour_Complex
{
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

    [TestMethod]
    public void WeirdThing2()
    {
        Time time = new Time();
        time.SetTimeSignature(4, 4);
        time.AddTime(Durations.EighthNote * 5);

        var ties = time.GetNoteTies(Durations.DottedEighthNote);

        Assert.AreEqual(2, ties.Length, "wrong number of ties");
        Assert.AreEqual(Durations.EighthNote, ties[0]);
        Assert.AreEqual(Durations.SixteenthNote, ties[1]);
    }
}