using System.Collections.ObjectModel;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class SubdivisionCellUnitTests
{

    public static IEnumerable<object[]> AdditionData
    {
        get
        {
            return
            [
                // tick in measure, duration, subdivision, expected cells
                new object[] {0, Durations.QuarterNote, 0, new bool[]{ false }},
                new object[] {0, Durations.QuarterNote, 1, new bool[]{ false, false }},
                new object[] {0, Durations.QuarterNote, 2, new bool[]{ true, false, false, false }},
                new object[] {0, Durations.QuarterNote, 3, new bool[]{ true, true, false, false, false, false, false, false }},
                new object[] {0, Durations.QuarterNote, 4, new bool[]{ true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false }},

                new object[] {Durations.EighthNote, Durations.QuarterNote, 0, new bool[]{ false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 1, new bool[]{ false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 2, new bool[]{ false, false, false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 3, new bool[]{ false, true, true, false, false, false, false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 4, new bool[]{ false, false, true, true, true, true, false, false, false, false, false, false, false, false, false, false }},

                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 0, new bool[]{ false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 1, new bool[]{ false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 2, new bool[]{ false, false, false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 3, new bool[]{ false, true, false, false, false, false, false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 4, new bool[]{ false, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false }},
            ];
        }
    }

    [TestMethod]
    [DynamicData(nameof(AdditionData))]
    public void GetSubdivisionCells(int tickInMeasure, int duration, int subdivision, bool[] expectedCells)
    {
        // arrange
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        // act
        var actualCells = time.GetSubdivisionCells(subdivision, tickInMeasure, duration);

        // assert
        CollectionAssert.AreEqual(expectedCells, actualCells);
    }
}
