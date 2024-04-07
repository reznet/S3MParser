using System.Diagnostics;

namespace S3mToMidi
{
    internal class OutputChannel
    {
        private static int nextChannelNumber = 1;
        
        public OutputChannel()
        {
            ChannelNumber = nextChannelNumber++;
            if (ChannelNumber == 09) // 10 in zero-based counter is 09
            {
                // avoid General Midi channel 10 because players interpret it as a drum channel
                Console.WriteLine("Skipping output drum channel 10");
                ChannelNumber = nextChannelNumber++;
            }
            Console.WriteLine("Creating output channel {0}", ChannelNumber);
            Debug.Assert(ChannelNumber <= 16, "output channel number out of range.");
        }

        public int ChannelNumber { get; }
        public NoteEvent? CurrentNote;

        public List<Event> Events = [];

        public bool IsPlayingNote => CurrentNote != null;

        public void AddEvent(Event noteEvent)
        {
            Events.Add(noteEvent);
        }

        public void AddEvent(NoteEvent noteEvent)
        {
            Debug.Assert(noteEvent.Channel == ChannelNumber, "trying to add a note event to the wrong output channel");
            AddEvent((Event)noteEvent);
            if (noteEvent.Type == NoteEvent.EventType.NoteOn)
            {
                Debug.Assert(CurrentNote == null, "trying to add note on event when there is already a current note");
                CurrentNote = noteEvent;
            }
            else if (noteEvent.Type == NoteEvent.EventType.NoteOff)
            {
                Debug.Assert(CurrentNote != null, "Trying to add a ntoe off event where there is no note playing.");
                CurrentNote = null;
            }
        }
    }
    
}