using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests
{
    [TestClass]
    public class RhythmGridUnitTests_FourFour
    {
        [TestMethod]
        public void DottedHalfNoteOnBeatOne()
        {
            // arrange
            var rhythmGrid = new RhythmGrid(4, Durations.QuarterNote);

            // act
            var options = rhythmGrid.GetDurationOptionsByStartTime(0);

            // assert
            CollectionAssert.Contains(options, Durations.DottedHalfNote);
        }

        [TestMethod]
        [DataRow(Durations.WholeNote, Durations.QuarterNote, Durations.QuarterNote * 0)]
        [DataRow(Durations.WholeNote, Durations.QuarterNote, Durations.QuarterNote * 1)]
        [DataRow(Durations.WholeNote, Durations.QuarterNote, Durations.QuarterNote * 2)]
        [DataRow(Durations.WholeNote, Durations.QuarterNote, Durations.QuarterNote * 3)]
        public void GridContainsAllQuarterNoteOptions(int duration, int expectedOption, int startTime)
        {
            // arrange
            var rhythmGrid = new RhythmGrid(4, Durations.QuarterNote);

            // act
            var options = rhythmGrid.GetDurationOptionsByStartTime(startTime);

            // assert
            CollectionAssert.Contains(options, expectedOption);
        }

        [TestMethod]
        [DataRow(Durations.WholeNote, Durations.DottedEighthNote, 0)]
        [DataRow(Durations.WholeNote, Durations.DottedEighthNote, Durations.SixteenthNote * 9)]
        public void GridContainsAllDottedEightNoteOptions(int duration, int expectedOption, int startTime)
        {
            // arrange
            var rhythmGrid = new RhythmGrid(4, Durations.QuarterNote);

            // act
            var options = rhythmGrid.GetDurationOptionsByStartTime(startTime);

            // assert
            CollectionAssert.Contains(options, expectedOption, "options did not contain expected start time " + startTime);
        }
    }
}