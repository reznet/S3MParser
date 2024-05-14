using System.Collections.Immutable;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests
{
    [TestClass]
    public class StaffUnitTests
    {
        [TestMethod]
        public void EmptyEvents()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>();

            // act
            sut.Write(events.ToImmutableList());

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
}
", stringWriter.ToString());
        }

        [TestMethod]
        public void QuarterNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Time.TICKS_PER_QUARTERNOTE, 42, 100)
            };

            // act
            sut.Write(events.ToImmutableList());

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,4
}
", stringWriter.ToString());
        }

        [TestMethod]
        public void WholeNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Time.TICKS_PER_QUARTERNOTE * 4, 42, 100)
            };

            // act
            sut.Write(events.ToImmutableList());

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,1
}
", stringWriter.ToString());
        }

        [TestMethod]
        public void DottedHalfNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Time.TICKS_PER_QUARTERNOTE * 3, 42, 100)
            };

            // act
            sut.Write(events.ToImmutableList());

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,2.
}
", stringWriter.ToString());
        }

        [TestMethod]
        public void SyncopationAcrossTwoMeasures()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Time.TICKS_PER_QUARTERNOTE, 42, 100),
                new NoteWithDurationEvent(Time.TICKS_PER_QUARTERNOTE, Time.TICKS_PER_QUARTERNOTE * 4, 42, 100),
                new NoteWithDurationEvent(Time.TICKS_PER_QUARTERNOTE * 7, Time.TICKS_PER_QUARTERNOTE * 3, 42, 100),
            };

            // act
            sut.Write(events.ToImmutableList());

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,4
\set fontSize = #0
b-flat,2.~ 4
\set fontSize = #0
b-flat,2.
}
", stringWriter.ToString());
        }
    }
}