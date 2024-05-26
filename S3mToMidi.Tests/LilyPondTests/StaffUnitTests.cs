using System.Collections.Immutable;
using System.Text.RegularExpressions;
using S3mToMidi.LilyPond;

namespace S3mToMidi.Tests.LilyPondTests
{
    internal static class StringExtensions
    {
        private static readonly Regex _regex = new Regex(@"\s+", RegexOptions.Multiline);
        public static string NormalizeLilyPondText(this string text)
        {
            return _regex.Replace(text, " ").Replace(Environment.NewLine, " ");
        }
    }
    [TestClass]
    public class StaffUnitTests
    {
        
        [TestMethod]
        public void EmptyEvents()
        {
            // arrange
            var stringWriter = new StringWriter();
            var lilypondTextWriter = new LilyPondTextWriter();
            var sut = new StaffWriter(lilypondTextWriter);
            var events = new List<Event>();

            // act
            sut.Write(events.ToImmutableList());
            lilypondTextWriter.Flush(stringWriter);

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
}
".NormalizeLilyPondText(), stringWriter.ToString().NormalizeLilyPondText());
        }

        [TestMethod]
        public void QuarterNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var lilypondTextWriter = new LilyPondTextWriter();
            var sut = new StaffWriter(lilypondTextWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.QuarterNote, 42, 100, 1)
            };

            // act
            sut.Write(events.ToImmutableList());
            lilypondTextWriter.Flush(stringWriter);

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,4
}
".NormalizeLilyPondText(), stringWriter.ToString().NormalizeLilyPondText());
        }

        [TestMethod]
        public void WholeNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var lilypondTextWriter = new LilyPondTextWriter();
            var sut = new StaffWriter(lilypondTextWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.WholeNote, 42, 100, 1)
            };

            // act
            sut.Write(events.ToImmutableList());
            lilypondTextWriter.Flush(stringWriter);

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,1
}
".NormalizeLilyPondText(), stringWriter.ToString().NormalizeLilyPondText());
        }

        [TestMethod]
        [Ignore] // doesn't return dotted notes anymore
        public void DottedHalfNote()
        {
            // arrange
            var stringWriter = new StringWriter();
            var lilypondTextWriter = new LilyPondTextWriter();
            var sut = new StaffWriter(lilypondTextWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.DottedHalfNote, 42, 100, 1)
            };

            // act
            sut.Write(events.ToImmutableList());
            lilypondTextWriter.Flush(stringWriter);

            // assert
            Assert.AreEqual(@"\new Staff { \key c \minor
\clef bass
\time 4/4
\set fontSize = #0
b-flat,2.
}
".NormalizeLilyPondText(), stringWriter.ToString().NormalizeLilyPondText());
        }

        [TestMethod]
        [Ignore] // doesn't return dotted notes
        public void SyncopationAcrossTwoMeasures()
        {
            // arrange
            var stringWriter = new StringWriter();
            var lilypondTextWriter = new LilyPondTextWriter();
            var sut = new StaffWriter(lilypondTextWriter);
            var events = new List<Event>(){
                new TimeSignatureEvent(0, 4, 4),
                new NoteWithDurationEvent(0, Durations.QuarterNote, 42, 100, 1),
                new NoteWithDurationEvent(Durations.QuarterNote, Durations.WholeNote, 42, 100, 1),
                new NoteWithDurationEvent(Durations.QuarterNote * 7, Durations.DottedHalfNote, 42, 100, 1),
            };

            // act
            sut.Write(events.ToImmutableList());
            lilypondTextWriter.Flush(stringWriter);

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
".NormalizeLilyPondText(), stringWriter.ToString().NormalizeLilyPondText());
        }
    }
}