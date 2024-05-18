using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests
{
    [TestClass]
    public class ConvertToLilyPondDurationUnitTests
    {
        [TestMethod]
        public void DottedHalfNote()
        {
            // arrange
            var sut = new Time();

            // act
            var result = sut.ConvertToLilyPondDuration(Durations.DottedHalfNote);

            // assert
            Assert.AreEqual("2.", result);
        }
    }
}