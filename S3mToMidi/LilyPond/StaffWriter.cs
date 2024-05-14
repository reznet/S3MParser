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

            for (int i = 0; i < events.Count; i++)
            {
                ProcessEvent(events, i, clef);
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


        private void ProcessEvent(ImmutableList<Event> events, int eventIndex, Clef clef)
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

                        for (int tupletIndex = eventIndex; tupletIndex < events.Count && tupletNotes.Sum(t => t.Tick) < tupletDuration; tupletIndex++)
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

                        time.AddTime(myNote.Duration);

                        return;
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
                        }
                    }
                }
                else if (myNote is NoteWithDurationEvent noteWithDuration)
                {
                    clef.WriteStaffForChannelPitch(noteWithDuration.Pitch, writer);
                    writer.WriteLine("\\set fontSize = #{0}", GetFontSizeForVelocity(noteWithDuration.Velocity));
                    writer.Write(pitch.ChannelNoteToLilyPondPitch(noteWithDuration.Pitch));
                    writer.WriteLine(string.Join("~ ", durations.SelectMany(time.GetNoteTies).Select(time.ConvertToLilyPondDuration)));
                }

                time.AddTime(myNote.Duration);
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
        }

        public static int GetFontSizeForVelocity(int velocity)
        {
            return -1 * (8 - Math.Min(8, (velocity + 8) / 8));
        }
    }
}