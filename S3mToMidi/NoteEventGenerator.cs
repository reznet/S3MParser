﻿using S3M;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3MParser
{
    static class NoteEventGenerator
    {
        public static List<List<NoteEvent>> Generate(S3MFile file)
        {
            Dictionary<int, Channel> channels = new Dictionary<int, Channel>();

            int speed = file.InitialSpeed;
            int tick = 0;
            int rowSkip = 0;

            foreach (var pattern in file.Patterns)
            {
                foreach (var row in pattern.Rows.Skip(rowSkip))
                {
                    rowSkip = 0;
                    bool breakPatternToRow = false;
                    foreach (var channelEvent in row.ChannelEvents)
                    {
                        Channel channel = GetChannel(channels, channelEvent.ChannelNumber);

                        bool needNoteOff = channel.IsPlayingNote && channelEvent.NoteAction != NoteAction.None;

                        if (needNoteOff)
                        {
                            channel.AddNoteEvent(GenerateNoteOffEvent(channel, tick));
                        }

                        if(channelEvent.NoteAction == NoteAction.Start)
                        {
                            channel.AddNoteEvent(GenerateNoteOnEvent(channel, channelEvent, tick));
                        }

                        if (channelEvent.Command == CommandType.BreakPatternToRow)
                        {
                            breakPatternToRow = true;
                            rowSkip = channelEvent.Data;
                            break;
                        }
                    }

                    tick += speed;

                    if (breakPatternToRow)
                    {
                        break;
                    }
                }
            }

            // finalize any leftover note on events
            foreach (var key in channels.Keys)
            {
                Channel channel = channels[key];
                if (channel.CurrentNote != null)
                {
                    channel.AddNoteEvent(GenerateNoteOffEvent(channel, tick));
                }
            }

            List<List<NoteEvent>> allEvents = new List<List<NoteEvent>>();
            foreach (var key in channels.Keys.OrderBy(i => i))
            {
                allEvents.Add(channels[key].NoteEvents);
            }
            return allEvents;
        }

        private static NoteEvent GenerateNoteOnEvent(Channel channel, ChannelEvent channelEvent,int tick)
        {
            channel.Volume = channelEvent.HasVolume ? channelEvent.Volume : channel.Volume;
            channel.Instrument = channelEvent.HasInstrument ? channelEvent.Instrument : channel.Instrument;
            NoteEvent noteOnEvent = new NoteEvent(tick, NoteEvent.EventType.NoteOn, channelEvent.Instrument, channelEvent.Note, channel.Volume);
            channel.CurrentNote = noteOnEvent;
            return noteOnEvent;
        }

        private static NoteEvent GenerateNoteOffEvent(Channel channel, int tick)
        {
            NoteEvent offEvent = channel.CurrentNote.Clone(tick);
            offEvent.Type = NoteEvent.EventType.NoteOff;
            channel.CurrentNote = null;
            return offEvent;
        }

        private static Channel GetChannel(Dictionary<int, Channel> channels, int channelNumber)
        {
            if(!channels.ContainsKey(channelNumber))
            {
                channels.Add(channelNumber, new Channel());
            }
            return channels[channelNumber];
        }

        private class Channel
        {
            public NoteEvent CurrentNote;
            public int Volume;
            public int Instrument;
            public List<NoteEvent> NoteEvents = new List<NoteEvent>();

            public bool IsPlayingNote { get { return CurrentNote != null; } }

            public void AddNoteEvent(NoteEvent noteEvent)
            {
                NoteEvents.Add(noteEvent);
            }
        }
    }
}
