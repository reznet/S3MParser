using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace S3MParser
{
    class NoteEvent
    {
        public enum EventType
        {
            NoteOn,
            NoteOff
        }

        public NoteEvent(int tick, EventType eventType, int channel, int pitch, int velocity)
        {
            this.Tick = tick;
            this.Type = eventType;
            this.Channel = channel;
            this.Pitch = pitch;
            this.Velocity = velocity;

            Debug.Assert(0 <= this.Tick, "negative tick");
            Debug.Assert(0 <= this.Channel, "negative channel");
            Debug.Assert(this.Channel < 16, "channel must be less than 16");
            Debug.Assert(0 <= this.Pitch, "negative pitch");
            Debug.Assert(0 <= this.Velocity, "negative velocity");
        }

        public EventType Type { get; set; }
        public int Tick { get; set; }
        public int Channel { get; set; }
        public int Pitch { get; set; }
        public int Velocity { get; set; }

        public NoteEvent Clone()
        {
            return new NoteEvent(this.Tick, this.Type, this.Channel, this.Pitch, this.Velocity);
        }

        public override string ToString()
        {
            return string.Format("NoteEvent Tick:{0} Type:{1} Channel:{2} Pitch:{3} Velocity:{4}", Tick, Type, Channel, Pitch, Velocity);
        }
    }
}
