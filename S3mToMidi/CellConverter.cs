using S3M;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3MParser
{
    class CellConverter
    {
        public Dictionary<int, ChannelSettings> channelSettings;

        public CellConverter()
        {
            this.channelSettings = new Dictionary<int, ChannelSettings>();
        }

        private int tick;

        public IEnumerable<NoteEvent> Convert(IEnumerable<Cell> cells)
        {
            NoteEvent lastNoteEvent = null;
            int speed = 3;
            foreach (var cell in cells)
            {
                if (cell.Event == null)
                {
                    tick++;

                    continue;
                }
                ChannelEvent ce = cell.Event;
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
                            lastNoteEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOn, ce.Instrument, GetNoteOffPitch(lastNoteEvent, ce), GetMissingVelocity(ce));
                            this[ce.ChannelNumber].Volume = lastNoteEvent.Velocity;
                            this[ce.ChannelNumber].Pitch = lastNoteEvent.Pitch;
                            yield return lastNoteEvent;
                        }
                    }
                    else if (ce.Note == 0xFE) // note off
                    {
                        lastNoteEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOff, ce.Instrument, GetNoteOffPitch(lastNoteEvent, ce), GetMissingVelocity(ce));
                        this[ce.ChannelNumber].Volume = lastNoteEvent.Velocity;
                        this[ce.ChannelNumber].Pitch = lastNoteEvent.Pitch;
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
                        this[ce.ChannelNumber].Pitch = lastNoteEvent.Pitch;
                        yield return lastNoteEvent;
                    }
                }
                tick += speed;
            }
            if (lastNoteEvent != null && lastNoteEvent.Type == NoteEvent.EventType.NoteOn)
            {
                NoteEvent offEvent = lastNoteEvent.Clone();
                offEvent.Type = NoteEvent.EventType.NoteOff;
                offEvent.Tick = tick;
                lastNoteEvent = offEvent;
                yield return offEvent;
            }
        }

        private int GetNoteOffPitch(NoteEvent lastNoteEvent, ChannelEvent ce)
        {
            if (lastNoteEvent == null)
            {
                return this[ce.ChannelNumber].Pitch;
            }
            return lastNoteEvent.Pitch;
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
            return ce.Volume == 0 ? 0 : (ce.Volume * 2) - 1;
        }
    }
}
