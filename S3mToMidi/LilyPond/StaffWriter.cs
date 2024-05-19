using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace S3mToMidi.LilyPond
{
    internal class StaffWriter
    {
        private readonly TextWriter writer;
        private readonly Pitch pitch = new Pitch();
        private readonly Time time = new Time();

        public StaffWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Write(ImmutableList<Event> events)
        {
            writer.Write("\\new Staff ");

            writer.Write("{ ");

            // hack key signature
            WriteKeySignature("c", "minor");

            WriteClef("bass");
            Clef clef = new Clef("bass");

            int i = 0;
            while(i < events.Count)
            {
                int eventsProcessedCount = ProcessEvent(events, i, clef);
                i += eventsProcessedCount;
            }
            writer.Write("}");
            writer.WriteLine();
        }

        private void WriteClef(string clef)
        {
            writer.WriteLine("\\clef {0}", clef);
        }

        private void WriteKeySignature(string key, string mode)
        {
            writer.WriteLine("\\key {0} \\{1}", key, mode);
        }


        private int ProcessEvent(ImmutableList<Event> events, int eventIndex, Clef clef)
        {
            var e = events[eventIndex];
            if (e is DurationEvent myNote)
            {
                var durationTicks = myNote.Duration;
                Console.Out.WriteLine("Processing note duration {0}", durationTicks);

                for (int tupletDurationIndex = 0; tupletDurationIndex < Time.TupletDurations.Count; tupletDurationIndex++)
                {
                    var tupletBaseDuration = Time.TupletDurations[tupletDurationIndex].Item1;
                    if (durationTicks % tupletBaseDuration == 0 && (durationTicks / tupletBaseDuration) < 3)
                    {
                        // found a tuplet rhythm
                        Console.Out.WriteLine("Duration {0} appears to be part of a tuplet with base duration {1}", durationTicks, tupletBaseDuration);
                        var tupletNotes = new List<DurationEvent>();
                        tupletNotes.Add(myNote);

                        int tupletValue = durationTicks / Time.TupletDurations[tupletDurationIndex].Item1;
                        int tupletDuration = (Time.TupletDurations[tupletDurationIndex].Item1 * 3);
                        int remainingTupletDuration = tupletDuration - durationTicks;

                        for (int tupletIndex = eventIndex + 1; tupletIndex < events.Count && tupletNotes.Sum(t => t.Duration) < tupletDuration; tupletIndex++)
                        {
                            if (events[tupletIndex] is DurationEvent nextNote)
                            {
                                if (nextNote.Duration % tupletBaseDuration != 0)
                                {
                                    //Debug.Fail("needed next note to be part of tuplet but it wasn't");
                                    break;
                                }
                                tupletNotes.Add(nextNote);
                            }
                        }

                        // emit tuplet
                        writer.WriteLine("\\tuplet 3/2 { ");

                        foreach (var tupletNote in tupletNotes)
                        {
                            //writer.WriteLine("\\set fontSize = #-{0}", (64 - tupletNote.NoteOn.Velocity) % (64 / 6));
                            var adjustedTupletDurationForDisplay = tupletNote.Duration * 3 / 2;
                            clef.WriteStaffForChannelPitch(tupletNote.Pitch, writer);
                            writer.WriteLine("{0}{1} ", pitch.ChannelNoteToLilyPondPitch(tupletNote.Pitch), time.ConvertToLilyPondDuration(adjustedTupletDurationForDisplay));
                        }

                        writer.WriteLine(" }");

                        time.AddTime(tupletNotes.Sum(t => t.Duration));

                        return tupletNotes.Count;
                    }
                }

                var durations = time.GetBarlineTies(myNote.Duration);

                if (myNote is RestEvent)
                {
                    foreach (var subDuration in durations)
                    {
                        var rests = time.GetNoteTies(subDuration);
                        foreach (var rest in rests)
                        {
                            writer.WriteLine("r{0} ", time.ConvertToLilyPondDuration(rest));
                            time.AddTime(rest);
                        }
                    }
                }
                else if (myNote is NoteWithDurationEvent noteWithDuration)
                {
                    var allSubdurations = new List<int>();
                    for(int i = 0; i < durations.Length; i++)
                    {
                        var barDuration = durations[i];
                        var subDurations = time.GetNoteTies(barDuration);
                        for(int j = 0; j < subDurations.Length; j++)
                        {
                            var subDuration = subDurations[j];
                            var lilypondDuration = time.ConvertToLilyPondDuration(subDuration);
                            int leftBrace = lilypondDuration.IndexOf('{');
                            int rightBrace = lilypondDuration.IndexOf('}');
                            if (-1 < leftBrace)
                            {
                                writer.Write(lilypondDuration.Substring(0, leftBrace + 1) + " ");
                            }

                            clef.WriteStaffForChannelPitch(noteWithDuration.Pitch, writer);
                            //writer.WriteLine("\\set fontSize = #{0}", GetFontSizeForVelocity(noteWithDuration.Velocity));
                            writer.Write(pitch.ChannelNoteToLilyPondPitch(noteWithDuration.Pitch));                                              

                            if (-1 < leftBrace)
                            {
                                writer.Write(lilypondDuration.Substring(leftBrace + 1, rightBrace - leftBrace - 1));
                            }
                            else
                            {
                                writer.Write(lilypondDuration);
                            }

                            if (j + 1 < subDurations.Length)
                            {
                                writer.Write("~");
                            }

                            writer.Write(" ");

                            if (-1 < rightBrace)
                            {
                                writer.Write(lilypondDuration.Substring(rightBrace));
                            }

                            time.AddTime(subDuration);
                        }
                    
                        
                    }
                    


                    
                    //writer.WriteLine(string.Join("~ ", allSubdurations.Select(time.ConvertToLilyPondDuration)));
                }
            }
            else if (e is NoteEvent note)
            {
                Debug.Fail("should have collapsed note events");
            }
            else if (e is TempoEvent tempoEvent)
            {
                writer.WriteLine(@"\tempo 4 = {0}", tempoEvent.TempoBpm);
            }
            else if (e is TimeSignatureEvent timeSignatureEvent)
            {
                if (time.SetTimeSignature(timeSignatureEvent.BeatsPerBar, timeSignatureEvent.BeatValue))
                {
                    writer.WriteLine(@"\time {0}/{1}", timeSignatureEvent.BeatsPerBar, timeSignatureEvent.BeatValue);
                }
            }
            else
            {
                Debug.Fail("unknown event type " + e.GetType().Name);
                // no-op
            }

            return 1;
        }

        public static int GetFontSizeForVelocity(int velocity)
        {
            return -1 * (8 - Math.Min(8, (velocity + 8) / 8));
        }
    }
}