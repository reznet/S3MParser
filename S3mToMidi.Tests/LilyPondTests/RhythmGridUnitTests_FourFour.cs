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
            var rhythmGrid = new RhythmGrid(Durations.WholeNote);

            // act
            var options = rhythmGrid.GetDurationOptionsByStartTime(0);

            // assert
            CollectionAssert.Contains(options, Durations.DottedHalfNote);
        }
    }
}