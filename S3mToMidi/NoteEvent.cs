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

        public NoteEvent(int tick, EventType eventType, int channel, int pitch, int velocity)
            : base(tick)
        {
            Type = eventType;
            Channel = channel;
            Pitch = pitch;
            Velocity = velocity;

            Debug.Assert(0 <= Channel, "negative channel");
            Debug.Assert(0 <= Pitch, "negative pitch");
            Debug.Assert(Channel < 16, "channel must be less than 16");
            Debug.Assert(0 <= Velocity, "negative velocity");
        }

        public EventType Type { get; set; }

        public int Channel { get; set; }
        public int Pitch { get; set; }
        public int Velocity { get; set; }

        public NoteEvent Clone(int tick)
        {
            return new NoteEvent(tick, Type, Channel, Pitch, Velocity);
        }

        public override string ToString()
        {
            return string.Format("NoteEvent Tick:{0} Type:{1} Channel:{2} Pitch:{3} Velocity:{4}", Tick, Type, Channel, Pitch, Velocity);
        }
    }
}
