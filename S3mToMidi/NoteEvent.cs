using System.Diagnostics;

namespace S3mToMidi
{
    internal class NoteEvent : Event
    {
        public enum EventType
        {
            NoteOn,
            NoteOff
        }

        public NoteEvent(int tick, EventType eventType, int channel, int instrument, int pitch, int velocity)
            : base(tick)
        {
            Type = eventType;
            Channel = channel;
            Instrument = instrument;
            Pitch = pitch;
            Velocity = velocity;

            Debug.Assert(0 <= Channel, "invalid channel");
            Debug.Assert(0 <= Instrument, "invalid instrument");
            Debug.Assert(0 <= Pitch, "invalid pitch");
            Debug.Assert(Channel < 16, "channel must be less than 16");
            Debug.Assert(0 <= Velocity, "invalid velocity");
        }

        public EventType Type { get; set; }

        public int Channel { get; set; }

        public int Instrument { get; set; }
        public int Pitch { get; set; }
        public int Velocity { get; set; }

        public NoteEvent Clone(int tick)
        {
            return new NoteEvent(tick, Type, Channel, Instrument, Pitch, Velocity);
        }

        public override string ToString()
        {
            return string.Format("NoteEvent Tick:{0} Type:{1} Channel:{2} Instrument:{3} Pitch:{4} Velocity:{5}", Tick, Type, Channel, Instrument, Pitch, Velocity);
        }
    }
}
