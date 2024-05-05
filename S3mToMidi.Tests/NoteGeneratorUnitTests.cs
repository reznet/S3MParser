using S3M;

namespace S3mToMidi.Tests;

[TestClass]
public class NoteGeneratorUnitTests
{
    [TestMethod]
    public void BzamIntro()
    {
        S3MFile trackerFile = new S3MFile();
        trackerFile.InitialTempo = 120;
        trackerFile.InitialSpeed = 3;
        trackerFile.OrderCount = 1;
        trackerFile.Orders = new int[] { 0 };
        trackerFile.PatternCount = 1;
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0 } };

        trackerFile.Patterns[0].Rows[0].ChannelEvents[0] = new S3M.ChannelEvent(1){ Note = 0xFE };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[1] = new S3M.ChannelEvent(2){ Note = 0xFE };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[2] = new S3M.ChannelEvent(3){ Note = 0xFF, Command = CommandType.SetTempo, Data = 140 };

        trackerFile.Patterns[0].Rows[4].ChannelEvents[2] = new S3M.ChannelEvent(3){ Note = 32, Instrument = 5 };

        trackerFile.Patterns[0].Rows[19].ChannelEvents[4] = new S3M.ChannelEvent(5) { Command = CommandType.BreakPatternToRow, Data = 0 };

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions(), (c) => new LilyPondOutputChannel(c)).Generate(trackerFile);

        Console.WriteLine("note generator returned {0} keys and {1} events", noteEvents.Keys.Count, noteEvents.Values.SelectMany(v => v).Count());

        Assert.AreEqual(1, noteEvents.Keys.Count, "wrong number of channels returned");
        Assert.AreEqual(3, noteEvents.Keys.First(), "wrong channel number returned");
        Assert.AreEqual(5, noteEvents[noteEvents.Keys.First()].Count, "wrong number of events returned");
        
        Assert.AreEqual(typeof(TimeSignatureEvent), noteEvents[3][0].GetType());
        Assert.AreEqual(typeof(TempoEvent), noteEvents[3][1].GetType());

        Assert.AreEqual(typeof(NoteEvent), noteEvents[3][2].GetType());
        Assert.AreEqual(NoteEvent.EventType.NoteOn, ((NoteEvent)noteEvents[3][2]).Type);

        Assert.AreEqual(typeof(NoteEvent), noteEvents[3][3].GetType());
        Assert.AreEqual(NoteEvent.EventType.NoteOff, ((NoteEvent)noteEvents[3][3]).Type);

        Assert.AreEqual(typeof(SongEndEvent), noteEvents[3][4].GetType());
    }
}
