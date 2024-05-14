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
            var result = sut.ConvertToLilyPondDuration(Time.TICKS_PER_QUARTERNOTE * 3);

            // assert
            Assert.AreEqual("2.", result);
        }
    }
}