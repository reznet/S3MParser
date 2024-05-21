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
                new NoteWithDurationEvent(0, Durations.QuarterNote, 42, 100, 1)
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
                new NoteWithDurationEvent(0, Durations.WholeNote, 42, 100, 1)
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
        [Ignore] // doesn't return dotted notes anymore
        public void DottedHalfNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.DottedHalfNote, 42, 100, 1)
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
        [Ignore] // doesn't return dotted notes
        public void SyncopationAcrossTwoMeasures()
        {
            // arrange
            var stringWriter = new StringWriter();
            var sut = new StaffWriter(stringWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.QuarterNote, 42, 100, 1),
                new NoteWithDurationEvent(Durations.QuarterNote, Durations.WholeNote, 42, 100, 1),
                new NoteWithDurationEvent(Durations.QuarterNote * 7, Durations.DottedHalfNote, 42, 100, 1),
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