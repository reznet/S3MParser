using System.Collections.Immutable;
using System.Diagnostics;

namespace S3mToMidi
{
    internal abstract class OutputChannel
    {
        public int ChannelNumber { get; }
        public NoteEvent? CurrentNote;

        public abstract ImmutableList<Event> GetEvents();

        public bool IsPlayingNote => CurrentNote != null;

        public OutputChannel(int channelNumber)
        {
            ChannelNumber = channelNumber;
        }

        public void AddEvent(Event @event)
        {
            AddEventInternal(@event);
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

        protected abstract void AddEventInternal(Event @event);
    }
}