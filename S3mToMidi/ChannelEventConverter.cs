using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
{
    /*
    class ChannelEventConverter
    {
        public Dictionary<int, ChannelSettings> channelSettings;

        public ChannelEventConverter()
        {
            this.channelSettings = new Dictionary<int, ChannelSettings>();
        }

        public IEnumerable<NoteEvent> Convert(IEnumerable<ChannelEvent> events)
        {
            NoteEvent lastNoteEvent = null;
            int tick = 0;
            int speed = 3;
            foreach (ChannelEvent ce in events)
            {
                if (ce.Instrument != -1)
                {
                    if (ce.Note == 0xFF) // nil, use current
                    {
                        if (lastNoteEvent != null && ce.Instrument == lastNoteEvent.Channel)
                        {
                            // volume may change
                            // ignore for now
                            // TODO
                        }
                        else
                        {
                            // if there's a note playing on currentInstrument
                            // stop it
                            // then start new note playing on ce.Instrument
                            if (lastNoteEvent != null && lastNoteEvent.Type == NoteEvent.EventType.NoteOn)
                            {
                                NoteEvent offEvent = lastNoteEvent.Clone();
                                offEvent.Type = NoteEvent.EventType.NoteOff;
                                offEvent.Tick = tick;
                                lastNoteEvent = offEvent;
                                yield return offEvent;
                            }
                            lastNoteEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOn, ce.Instrument, lastNoteEvent.Pitch, GetMissingVelocity(ce));
                            this[ce.ChannelNumber].Volume = lastNoteEvent.Velocity;
                            yield return lastNoteEvent;
                        }
                    }
                    else if (ce.Note == 0xFE) // note off
                    {
                        lastNoteEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOff, ce.Instrument, lastNoteEvent.Pitch, GetMissingVelocity(ce));
                        this[ce.ChannelNumber].Volume = lastNoteEvent.Velocity;
                        yield return lastNoteEvent;
                    }
                    else // note start
                    {
                        if (lastNoteEvent != null && lastNoteEvent.Type == NoteEvent.EventType.NoteOn)
                        {
                            NoteEvent offEvent = lastNoteEvent.Clone();
                            offEvent.Type = NoteEvent.EventType.NoteOff;
                            offEvent.Tick = tick;
                            lastNoteEvent = offEvent;
                            yield return offEvent;
                        }
                        lastNoteEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOn, ce.Instrument, ChannelNoteToMidiPitch(ce.Note), GetMissingVelocity(ce));
                        this[ce.ChannelNumber].Volume = lastNoteEvent.Velocity;
                        yield return lastNoteEvent;
                    }
                }
                tick += speed;
            }
            if (lastNoteEvent.Type == NoteEvent.EventType.NoteOn)
            {
                NoteEvent offEvent = lastNoteEvent.Clone();
                offEvent.Type = NoteEvent.EventType.NoteOff;
                offEvent.Tick = tick;
                lastNoteEvent = offEvent;
                yield return offEvent;
            }
        }

        private ChannelSettings this[int channelNumber]
        {
            get
            {
                if (!this.channelSettings.Keys.Contains(channelNumber))
                {
                    this.channelSettings.Add(channelNumber, new ChannelSettings(channelNumber));
                }
                return this.channelSettings[channelNumber];
            }
        }

        private static int ChannelNoteToMidiPitch(int note)
        {
            int step = note & 15;
            int octave = note >> 4;

            return (octave * 12) + step;
        }

        private int GetMissingVelocity(ChannelEvent ce)
        {
            if (ce.Volume == -1) return this[ce.ChannelNumber].Volume;
            return ce.Volume;
        }
    }
     * */
}
