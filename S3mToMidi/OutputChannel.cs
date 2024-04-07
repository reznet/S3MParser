using System.Diagnostics;

namespace S3mToMidi
{
    internal class OutputChannel
    {
        private static int nextChannelNumber = 1;
        
        public OutputChannel()
        {
            ChannelNumber = nextChannelNumber++;
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