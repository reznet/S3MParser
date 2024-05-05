using System.ComponentModel;
using Melanchall.DryWetMidi.Core;
using S3M;
namespace S3mToMidi.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void EmptyPattern()
    {
        S3MFile trackerFile = new S3MFile();
        trackerFile.InitialTempo = 120;
        trackerFile.OrderCount = 1;
        trackerFile.Orders = new int[] { 0 };
        trackerFile.PatternCount = 1;
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0 } };

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions(), (c) => new MidiOutputChannel(c)).Generate(trackerFile);

        var midiFile = new MidiWriter().Write(noteEvents);

        Assert.AreEqual(1, midiFile.GetTrackChunks().Count());
    }

    [TestMethod]
    public void SingleNote()
    {
        S3MFile trackerFile = new S3MFile();
        trackerFile.InitialTempo = 120;
        trackerFile.InitialSpeed = 3;
        trackerFile.OrderCount = 1;
        trackerFile.Orders = new int[] { 0 };
        trackerFile.PatternCount = 1;
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0 }};

        trackerFile.Patterns[0].Rows[0].ChannelEvents[0] = new S3M.ChannelEvent(1) { Note = 32, Instrument = 1 };
        trackerFile.Patterns[0].Rows[1].ChannelEvents[0] = new S3M.ChannelEvent(1) { Note = 0xFE};

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions(), (c) => new MidiOutputChannel(c)).Generate(trackerFile);

        Console.WriteLine("note generator returned {0} keys and {1} events", noteEvents.Keys.Count, noteEvents.Values.SelectMany(v => v).Count());

        var midiFile = new MidiWriter().Write(noteEvents);

        var chunks = midiFile.GetTrackChunks();
        var chunk = chunks.First();
        foreach(var chunkEvent in chunk.Events){
            Console.WriteLine(chunkEvent);
        }
        Assert.AreEqual(6, chunk.Events.Count);

        var midiNoteEvents = chunk.Events.Where(e => e is Melanchall.DryWetMidi.Core.NoteEvent).Cast<Melanchall.DryWetMidi.Core.NoteEvent>().ToList();
        Assert.AreEqual(2, midiNoteEvents.Count);

        Assert.AreEqual(MidiEventType.NoteOn, midiNoteEvents[0].EventType); 
        Assert.AreEqual(36, midiNoteEvents[0].NoteNumber);
        Assert.AreEqual(0, midiNoteEvents[0].DeltaTime);

        Assert.AreEqual(MidiEventType.NoteOff, midiNoteEvents[1].EventType); 
        Assert.AreEqual(36, midiNoteEvents[1].NoteNumber);
        Assert.AreEqual(12, midiNoteEvents[1].DeltaTime);
    }

    [TestMethod]
    public void BogeyPattern000()
    {
        S3MFile trackerFile = new S3MFile();
        trackerFile.InitialTempo = 120;
        trackerFile.InitialSpeed = 3;
        trackerFile.OrderCount = 1;
        trackerFile.Orders = new int[] { 0 };
        trackerFile.PatternCount = 1;
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0 } };

        var row1 = new Row() { RowNumber = 1 };
        row1.ChannelEvents[0] = new S3M.ChannelEvent(1) { Note = 32, Instrument = 1 };

        var row2 = new Row() { RowNumber = 2 };
        row2.ChannelEvents[0] = new S3M.ChannelEvent(1) { Note = 0xFE};

        trackerFile.Patterns[0].Rows[0].ChannelEvents[0] = new S3M.ChannelEvent(1){ Note = 32, Instrument = 1 };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[1] = new S3M.ChannelEvent(2){ Note = 0xFF, Command = CommandType.SetTempo, Data = 80 };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[2] = new S3M.ChannelEvent(3){ Note = 32, Instrument = 5, Command = CommandType.SetSpeed, Data = 3 };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[3] = new S3M.ChannelEvent(4){ Note = 32, Instrument = 6, Volume = 40, Command = CommandType.VolumeSlideDown, Data = 4 };
        trackerFile.Patterns[0].Rows[0].ChannelEvents[4] = new S3M.ChannelEvent(5){ Note = 32, Instrument = 8 };
        trackerFile.Patterns[0].Rows[1].ChannelEvents[0] = new S3M.ChannelEvent(1){ Note = 0xFE };

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions(){ ExcludedChannels = [2, 3, 4, 5]}, (c) => new MidiOutputChannel(c)).Generate(trackerFile);

        Console.WriteLine("note generator returned {0} keys and {1} events", noteEvents.Keys.Count, noteEvents.Values.SelectMany(v => v).Count());

        var midiFile = new MidiWriter().Write(noteEvents);

        var chunks = midiFile.GetTrackChunks();
        var chunk = chunks.First();
        foreach(var chunkEvent in chunk.Events){
            Console.WriteLine(chunkEvent);
        }
        Assert.AreEqual(6, chunk.Events.Count);

        var midiNoteEvents = chunk.Events.Where(e => e is Melanchall.DryWetMidi.Core.NoteEvent).Cast<Melanchall.DryWetMidi.Core.NoteEvent>().ToList();
        Assert.AreEqual(2, midiNoteEvents.Count);

        Assert.AreEqual(MidiEventType.NoteOn, midiNoteEvents[0].EventType); 
        Assert.AreEqual(36, midiNoteEvents[0].NoteNumber);
        Assert.AreEqual(0, midiNoteEvents[0].DeltaTime);

        Assert.AreEqual(MidiEventType.NoteOff, midiNoteEvents[1].EventType); 
        Assert.AreEqual(36, midiNoteEvents[1].NoteNumber);
        Assert.AreEqual(12, midiNoteEvents[1].DeltaTime);
    }
}