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
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0, Rows = new List<Row>()}};

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions()).Generate(trackerFile);

        var midiFile = MidiWriter.Write(noteEvents);

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
        trackerFile.Patterns = new List<Pattern>(){ new Pattern(){ PatternNumber = 0, Rows = new List<Row>()}};

        var row1 = new Row() { RowNumber = 1, ChannelEvents = new List<S3M.ChannelEvent>()};
        row1.ChannelEvents.Add(new S3M.ChannelEvent() { ChannelNumber = 1, Note = 32, Instrument = 1 });

        var row2 = new Row() { RowNumber = 2, ChannelEvents = new List<S3M.ChannelEvent>()};
        row2.ChannelEvents.Add(new S3M.ChannelEvent() { ChannelNumber = 1, Note = 0xFE});

        trackerFile.Patterns[0].Rows.Add(row1);
        trackerFile.Patterns[0].Rows.Add(row2);

        var noteEvents = new NoteEventGenerator(new NoteEventGeneratorOptions()).Generate(trackerFile);

        Console.WriteLine("note generator returned {0} keys and {1} events", noteEvents.Keys.Count, noteEvents.Values.SelectMany(v => v).Count());

        var midiFile = MidiWriter.Write(noteEvents);

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
        Assert.AreEqual(24, midiNoteEvents[1].DeltaTime);
    }
}