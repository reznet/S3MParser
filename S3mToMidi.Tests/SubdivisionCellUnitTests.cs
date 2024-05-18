using System.Collections.ObjectModel;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests;

[TestClass]
public class SubdivisionCellUnitTests
{

    public static IEnumerable<object[]> FourFourData
    {
        get
        {
            return
            [
                // tick in measure, duration, subdivision, expected cells
                new object[] {0, Durations.WholeNote, 0, new bool[]{ true }},
                new object[] {0, Durations.WholeNote, 1, new bool[]{ true, true }},
                new object[] {0, Durations.WholeNote, 2, new bool[]{ true, true, true, true }},
                new object[] {0, Durations.WholeNote, 3, new bool[]{ true, true, true, true, true, true, true, true }},
                new object[] {0, Durations.WholeNote, 4, new bool[]{ true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }},

                new object[] {0, Durations.HalfNote, 0, new bool[]{ false }},
                new object[] {0, Durations.HalfNote, 1, new bool[]{ true, false }},
                new object[] {0, Durations.HalfNote, 2, new bool[]{ true, true, false, false }},
                new object[] {0, Durations.HalfNote, 3, new bool[]{ true, true, true, true, false, false, false, false }},
                new object[] {0, Durations.HalfNote, 4, new bool[]{ true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false }},

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
    [DynamicData(nameof(FourFourData))]
    public void GetSubdivisionCellsFourFour(int tickInMeasure, int duration, int subdivision, bool[] expectedCells)
    {
        // arrange
        Time time = new Time();
        time.SetTimeSignature(4, 4);

        // act
        (var _, var actualCells) = time.GetSubdivisionCells(subdivision, tickInMeasure, duration);

        // assert
        Assert.IsNotNull(actualCells, "null cell array returned");
        Assert.AreEqual(expectedCells.Length, actualCells.Length, "wrong number of cells returned");
        CollectionAssert.AreEqual(expectedCells, actualCells);
    }

    public static IEnumerable<object[]> SixFourData
    {
        get
        {
            return
            [
                // tick in measure, duration, subdivision, expected cells
                new object[] {0, Durations.DottedWholeNote, 0, new bool[]{ true, false }},
                new object[] {0, Durations.DottedWholeNote, 1, new bool[]{ true, true, true }},
                new object[] {0, Durations.DottedWholeNote, 2, new bool[]{ true, true, true, true, true, true }},
                new object[] {0, Durations.DottedWholeNote, 3, new bool[]{ true, true, true, true, true, true, true, true, true, true, true, true }},
                new object[] {0, Durations.DottedWholeNote, 4, new bool[]{ true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true }},

                new object[] {0, Durations.WholeNote, 0, new bool[]{ true, false }},
                new object[] {0, Durations.WholeNote, 1, new bool[]{ true, true, false }},
                new object[] {0, Durations.WholeNote, 2, new bool[]{ true, true, true, true, false, false }},
                new object[] {0, Durations.WholeNote, 3, new bool[]{ true, true, true, true, true, true, true, true, false, false, false, false }},
                new object[] {0, Durations.WholeNote, 4, new bool[]{ true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false }},

                new object[] {0, Durations.HalfNote, 0, new bool[]{ false, false }},
                new object[] {0, Durations.HalfNote, 1, new bool[]{ true, false, false }},
                new object[] {0, Durations.HalfNote, 2, new bool[]{ true, true, false, false, false, false }},
                new object[] {0, Durations.HalfNote, 3, new bool[]{ true, true, true, true, false, false, false, false, false, false, false, false }},
                new object[] {0, Durations.HalfNote, 4, new bool[]{ true, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }},

                new object[] {0, Durations.QuarterNote, 0, new bool[]{ false, false }},
                new object[] {0, Durations.QuarterNote, 1, new bool[]{ false, false, false }},
                new object[] {0, Durations.QuarterNote, 2, new bool[]{ true, false, false, false, false, false }},
                new object[] {0, Durations.QuarterNote, 3, new bool[]{ true, true, false, false, false, false, false, false, false, false, false, false }},
                new object[] {0, Durations.QuarterNote, 4, new bool[]{ true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }},

                new object[] {Durations.EighthNote, Durations.QuarterNote, 0, new bool[]{ false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 1, new bool[]{ false, false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 2, new bool[]{ false, false, false, false, false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 3, new bool[]{ false, true, true, false, false, false, false, false, false, false, false, false }},
                new object[] {Durations.EighthNote, Durations.QuarterNote, 4, new bool[]{ false, false, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }},

                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 0, new bool[]{ false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 1, new bool[]{ false, false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 2, new bool[]{ false, false, false, false, false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 3, new bool[]{ false, true, false, false, false, false, false, false, false, false, false, false }},
                new object[] {Durations.SixteenthNote, Durations.QuarterNote, 4, new bool[]{ false, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false }},
            ];
        }
    }

    [TestMethod]
    [DynamicData(nameof(SixFourData))]
    public void GetSubdivisionCellsSixFour(int tickInMeasure, int duration, int subdivision, bool[] expectedCells)
    {
        // arrange
        Time time = new Time();
        time.SetTimeSignature(6, 4);

        // act
        (var _, var actualCells) = time.GetSubdivisionCells(subdivision, tickInMeasure, duration);

        // assert
        Assert.IsNotNull(actualCells, "null cell array returned");
        Assert.AreEqual(expectedCells.Length, actualCells.Length, "wrong number of cells returned");
        CollectionAssert.AreEqual(expectedCells, actualCells);
    }
}
