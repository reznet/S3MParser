using System.Collections.Immutable;
using CommandLine;

namespace S3mToMidi
{
    internal class LilyPondOutputChannel : OutputChannel
    {
        private List<Event> events= new List<Event>();

        public LilyPondOutputChannel() : base(1)
        {
            
        }

        public override ImmutableList<Event> GetEvents()
        {
            return [.. events];
        }

        protected override void AddEventInternal(Event @event)
        {
            events.Add(@event);

            if(@event is NoteEvent)
            {
                var noteEvent = (NoteEvent)@event;

                if(noteEvent.Type == NoteEvent.EventType.NoteOff)
                {
                    var pitch = noteEvent.Pitch;
                    Console.Out.Write("{0}{1} ", ChannelNoteToLilyPondPitch(pitch), LilyPondDuration(noteEvent.Tick));
                }
            }
        }

        private static string[] PitchNames = ["c", "c-sharp", "d", "d-sharp", "e", "f", "f-sharp", "g", "g-sharp", "a", "a-sharp", "b"];

        private static string ChannelNoteToLilyPondPitch(int note)
        {
            // C5 = 64 = octave 5 + step 0
            int step = note & 15;
            int octave = 1 + (note >> 4);
            string octaveName = "";

            if(5 < octave)
            {
                octaveName = new string('\'', octave - 5);
            }
            else if (5 == octave)
            {
                // no-op
            }
            else if (octave < 5)
            {
                octaveName = new string(',', 5 - octave);
            }

            return PitchNames[step] + octaveName;
        }

        private static string LilyPondDuration(int duration)
        {
            return "4";
        }
    }
}