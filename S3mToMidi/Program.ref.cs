using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using Sanford.Multimedia.Midi;

namespace S3MParser
{
    class Program
    {
        static void Main(string[] args)
        {
            bool outputMidiVelocity = false;

            //var filename = "01-v-drac.s3m";
            //S3MFile f = S3MFile.Parse(filename);
            //var tempo = 60000000 / (f.Header.InitialTempo);
            //byte numerator = 4;
            //byte denominator = 4;
            //var speed = 3;
            //var skip = 0;
            
            var filename = "02-v-bewm.s3m";
            S3MFile f = S3MFile.Parse(filename);
            var tempo = 60000000 / (f.Header.InitialTempo);
            byte numerator = 4;
            byte denominator = 4;
            var speed = 3;
            var skip = 0;
            outputMidiVelocity = true;
            
            /*
            var filename = "V-OPTION.S3M";
            S3MFile f = S3MFile.Parse(filename);
            var tempo = 60000000 / (f.Header.InitialTempo);
            byte numerator = 6;
            byte denominator = 8;
            var speed = 3;
            var skip = 4;
          */
            /*
            var filename = "V-FALCON.S3M";
            S3MFile f = S3MFile.Parse(filename);
            var tempo = 60000000 / (1 * f.Header.InitialTempo);
            byte numerator = 4;
            byte denominator = 4;
            var speed = 3;
            var skip = 0;
            */
            /*
            var filename = "V-CONTRA.IT";
            S3MFile f = S3MFile.Parse(filename);
            var tempo = 60000000 / (1 * f.Header.InitialTempo);
            byte numerator = 4;
            byte denominator = 4;
            var speed = 3;
            var skip = 0;
             * */

            /*
            var filename = "V-BLAST.S3M";
            S3MFile f = S3MFile.Parse(filename);
            var tempo = 60000000 / (1 * f.Header.InitialTempo);
            byte numerator = 4;
            byte denominator = 4;
            var speed = 3;
            var skip = 0;*/

            Sequence s = new Sequence();
            Track t1 = new Track();
            Track t2 = new Track();
            Track t3 = new Track();
            Track drums = new Track();
            Track kick = new Track();
            Track timeTrack = new Track();
            s.Add(timeTrack);
            s.Add(t1);
            s.Add(t2);
            s.Add(t3);
            //s.Add(drums);
            //s.Add(kick);

            var drumPC = new ChannelMessageBuilder();
            drumPC.MidiChannel = 09;
            drumPC.Command = ChannelCommand.ProgramChange;
            drumPC.Data1 = 0;// 79 + ce.Instrument;
            drumPC.Build();
            drums.Insert(0, drumPC.Result);

            kick.Insert(0, drumPC.Result);

            TimeSignatureBuilder tsb = new TimeSignatureBuilder();
            tsb.Numerator = numerator;
            tsb.Denominator = denominator;
            tsb.ClocksPerMetronomeClick = 24;
            tsb.ThirtySecondNotesPerQuarterNote = 8;

            tsb.Build();

            timeTrack.Insert(0, tsb.Result);

            TempoChangeBuilder tcb = new TempoChangeBuilder();
            tcb.Tempo = tempo;
            tcb.Build();

            timeTrack.Insert(0, tcb.Result);

            var outputPatterns = new int[] { 5, 6, 7};

            //var c1 = (from p in f.OrderedPatterns
            //          where patterns.Contains(p.PatternNumber)
            //          select p)
            //         .SelectMany(p => p.Channels)
            //         .Where(c => c.ChannelNumber == 1)
            //         .SelectMany(ce => ce.ChannelEvents);

            //var c2 = (from p in f.OrderedPatterns
            //          where patterns.Contains(p.PatternNumber)
            //          select p)
            //         .SelectMany(p => p.Channels)
            //         .Where(c => c.ChannelNumber == 2)
            //         .SelectMany(ce => ce.ChannelEvents);

            //var c3 = (from p in f.OrderedPatterns
            //          where patterns.Contains(p.PatternNumber)
            //          select p)
            //         .SelectMany(p => p.Channels)
            //         .Where(c => c.ChannelNumber == 3)
            //         .SelectMany(ce => ce.ChannelEvents);



            var patterns = from p in f.OrderedPatterns.Skip(skip) /*where outputPatterns.Contains(p.PatternNumber) */select p;
            //.SelectMany(p => p.Rows);

            Dictionary<int, TrackInfo> tracks = new Dictionary<int, TrackInfo>();
            tracks.Add(1, new TrackInfo(t1));
            tracks[1].Channel = 0;
            tracks.Add(2, new TrackInfo(t2));
            tracks[2].Channel = 1;
            tracks.Add(3, new TrackInfo(t3));
            tracks[3].Channel = 2;
            var di = new TrackInfo(drums);
            di.Channel = 9;
            var mapper = new Func<ChannelEvent, int>(ce =>
            {
                switch (ce.Instrument)
                {
                    case 6:
                        return 42;
                    case 8:
                        return 35;
                    case 9:
                        return 38;
                    case 10:
                        return 47;
                    default:
                        Debug.Fail(ce.Instrument.ToString());
                        return 0;

                }
            });
            di.NoteMapper = mapper;
            var kickInfo = new TrackInfo(kick);
            kickInfo.Channel = 9;
            kickInfo.NoteMapper = mapper;
            tracks.Add(4, di);
            tracks.Add(5, kickInfo);
            

            var tick = 0;
            foreach (var pattern in patterns)
            {
                bool breakPattern = false;
                foreach (var row in pattern.Rows)
                {
                    //if ((row.RowNumber-1) % 16 == 0)
                    //{
                    //    drums.Insert(tick, kickOn);
                    //    drums.Insert(tick + speed, kickOff);
                    //}
                    foreach (var ce in row.ChannelEvents)
                    {
                        if (ce == null)
                        {
                            //Console.Out.WriteLine("skip");
                            continue;
                        }

                        if (ce.Command == 3)
                        {
                            var modulo = row.RowNumber % 32;
                            if (modulo != 0)
                            {
                                // sad to say, not sure why mod 8 but divide by 16 works
                                var m8 = row.RowNumber % 8;
                                if (m8 == 0)
                                {
                                    var restore = tsb.Result;
                                    tsb.Numerator = (byte)(row.RowNumber / 16);
                                    tsb.Build();
                                    var change = tsb.Result;
                                    timeTrack.Insert(tick, change);
                                    timeTrack.Insert(tick + 3, restore);
                                }
                                
                            }
                            //Debugger.Break();
                            // pattern break
                            // figure out the time signature of the pattern up to this point
                            // then go back and insert a time signature change message at the beginning
                            // of the pattern
                            // and another one at the end to revert
                            //var restore = tsb.Result;
                            //tsb.Numerator = 2;
                            //tsb.Build();
                            //var change = tsb.Result;
                            //timeTrack.Insert(rowStartTick, change);
                            //timeTrack.Insert(tick + 3, restore);
                            breakPattern = true;
                        }

                        if (!tracks.ContainsKey(ce.ChannelNumber)) continue;
                        TrackInfo ti = tracks[ce.ChannelNumber];

                        if (ce.Note == -1 )
                        {
                            // skip
                        }
                        else if( ce.Note == 0xFF)
                        {
                            // re-use current note, but change instrument
                            
                            //Console.Out.WriteLine("skip");
                        }
                        else if (ce.Note == 0xFE) //off
                        {
                            if (ti.LastPitch != -1)
                            {
                                NoteOff(tick, ti);
                            }
                        }
                        else
                        {
                            //if (ce.Volume != -1 && ce.Volume < 32) continue;
                            if (ti.LastPitch != -1)
                            {
                                NoteOff(tick, ti);
                            }

                            //p = ProgramChange(ti.Track, tick, p, ce);

                            var delay = 0;
                            if (ce.Command == 19)
                            {
                                if (208 <= ce.Data && ce.Data <= 214)  // HACK: 214 is a guess at the top range of SD commands
                                {
                                    delay = ce.Data - 208;
                                    Debug.Assert(delay >= 0);
                                }
                            }


                            NoteOn(tick + delay, ti, ce);

                        }
                        
                    }
                    tick += speed;
                    if (breakPattern)
                    {
                        break;
                    }
                }
            }

            foreach (var pair in tracks)
            {
                if (pair.Value.LastPitch != -1)
                {
                    NoteOff(tick, pair.Value);
                }
            }

            foreach (MidiEvent m in drums.Iterator().Take(5))
            {
                if (m.MidiMessage is ChannelMessage)
                {
                    ChannelMessage cm = (ChannelMessage)m.MidiMessage;
                    Console.Out.WriteLine("{0} {1}", m.AbsoluteTicks, cm.Command);
                }
            }

            s.Save(Path.ChangeExtension(filename, ".mid"));
            /*
            var tick = 0;
            var speed = 3;
            var lastNote = -1;
            var p = -1;
            foreach (var ce in c1)
            {
                if (ce == null || ce.Note == -1 || ce.Note == 0xFF)
                {
                    tick += speed;
                    Console.Out.WriteLine("skip");
                    continue;
                }
                else if (ce.Note == 0xFE) //off
                {
                    lastNote = NoteOff(t, tick, lastNote);
                }
                else
                {
                    if (lastNote != -1)
                    {
                        lastNote = NoteOff(t, tick, lastNote);
                    }

                    p = ProgramChange(t, tick, p, ce);

                    var delay = 0;
                    if (ce.Command == 19)
                    {
                        if (208 <= ce.Data && ce.Data <= 214)  // HACK: 214 is a guess at the top range of SD commands
                        {
                            delay = ce.Data - 208;
                            Debug.Assert(delay >= 0);
                        }
                    }

                    lastNote = NoteOn(t, tick + delay, ce.Note);

                }
                tick += speed;
            }
            lastNote = NoteOff(t, tick, lastNote);
             * 
             * */

            
        }

        private static ChannelMessage GetChannelMessage(ChannelCommand command, int channel, int data1, int data2)
        {
            ChannelMessageBuilder on = new ChannelMessageBuilder();
            on.MidiChannel = channel;
            on.Command = command;
            on.Data1 = data1;
            on.Data2 = data2;
            on.Build();

            return on.Result;
        }

    private class TrackInfo
    {
        public Track Track;
        public int LastPitch = -1;
        public int LastVolume = -1;
        public int Channel;

        public Func<ChannelEvent, int> NoteMapper;

        public TrackInfo(Track t)
        {
            this.Track = t;
        }
    }

        

        private static int ProgramChange(Track t, int tick, int p, int channel, ChannelEvent ce)
        {
            if (p != ce.Instrument)
            {
                Console.Out.WriteLine("PC  " + ce.Instrument);
                ChannelMessageBuilder pc = new ChannelMessageBuilder();
                pc.MidiChannel = channel;
                pc.Command = ChannelCommand.ProgramChange;
                pc.Data1 = 0;// 79 + ce.Instrument;
                pc.Build();

                t.Insert(tick, pc.Result);
                p = ce.Instrument;
            }
            return p;
        }

        private static void NoteOn(int tick, TrackInfo ti, ChannelEvent ce)
        {
            bool eventHasVolume = ce.Volume != -1;
            int noteVolume = eventHasVolume ? ce.Volume : 64; // TODO: read from sample settings

            var pitch = CellConverter.ChannelNoteToMidiPitch(ce.Note);
            if (ti.NoteMapper != null)
            {
                pitch = ti.NoteMapper(ce);
            }
            //Console.Out.WriteLine("{0:00} On  " + note, tick);
            ChannelMessageBuilder on = new ChannelMessageBuilder();
            on.MidiChannel = ti.Channel;
            on.Command = ChannelCommand.NoteOn;
            on.Data1 = pitch;
            on.Data2 = Math.Min(127, 2 * noteVolume);
            on.Build();

            ti.Track.Insert(tick, on.Result);
            ti.LastPitch = pitch;
            ti.LastVolume = noteVolume;
        }

        private static void NoteOff(int tick, TrackInfo ti)
        {
            //Console.Out.WriteLine("{0:00} Off " + note, tick);
            ChannelMessageBuilder off = new ChannelMessageBuilder();
            off.MidiChannel = ti.Channel;
            off.Command = ChannelCommand.NoteOff;
            off.Data1 = ti.LastPitch;
            off.Data2 = ti.LastVolume;
            off.Build();

            ti.Track.Insert(tick, off.Result);
            ti.LastPitch = -1;
            ti.LastVolume = -1;
        }

        

    }
}
