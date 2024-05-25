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

        private readonly Notehead notehead= new Notehead();
        private string currentNotehead;

        public StaffWriter(TextWriter writer)
        {
            this.writer = writer;
            this.currentNotehead = this.notehead.Default;
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
            while (i < events.Count)
            {
                int eventsProcessedCount = ProcessEvent(events, i, clef);
                i += eventsProcessedCount;
            }
            writer.Write("}");
            writer.WriteLine();
        }

        private void WriteClef(string clef)
        {
            writer.WriteLine(@"");
            writer.WriteLine("\\clef {0}", clef);
        }

        private void WriteKeySignature(string key, string mode)
        {
            writer.WriteLine(@"");
            writer.WriteLine("\\key {0} \\{1}", key, mode);
        }


        private int ProcessEvent(ImmutableList<Event> events, int eventIndex, Clef clef)
        {
            var e = events[eventIndex];
            if (e is DurationEvent myNote)
            {
                var durationTicks = myNote.Duration;
                Console.Out.WriteLine("Processing note duration {0}", durationTicks);

                var durations = time.GetBarlineTies(myNote.Duration);
                var noteWithDuration = myNote as NoteWithDurationEvent;

                for (int i = 0; i < durations.Length; i++)
                {
                    var durationInBar = durations[i];
                    var subDurations = time.GetNoteTies(durationInBar);
                    for (int j = 0; j < subDurations.Length; j++)
                    {
                        var subDuration = subDurations[j];
                        var lilypondDuration = time.ConvertToLilyPondDuration(subDuration);
                        int leftBrace = lilypondDuration.IndexOf('{');
                        int rightBrace = lilypondDuration.IndexOf('}');
                        if (-1 < leftBrace)
                        {
                            writer.Write(lilypondDuration.Substring(0, leftBrace + 1) + " ");
                        }

                        if (noteWithDuration != null)
                        {
                            clef.WriteStaffForChannelPitch(noteWithDuration.Pitch, writer);
                            clef.WriteVelocity(noteWithDuration.Velocity, writer);
                            string newNotehead = notehead.GetNotehead(noteWithDuration.Instrument);
                            if(newNotehead != currentNotehead)
                            {
                                writer.WriteLine("\\override NoteHead.style = #'" + newNotehead);
                                currentNotehead = newNotehead;
                            }
                            writer.Write(pitch.ChannelNoteToLilyPondPitch(noteWithDuration.Pitch));
                        }
                        else
                        {
                            writer.Write("r");
                        }


                        if (-1 < leftBrace)
                        {
                            writer.Write(lilypondDuration.Substring(leftBrace + 1, rightBrace - leftBrace - 1));
                        }
                        else
                        {
                            writer.Write(lilypondDuration);
                        }

                        if (noteWithDuration != null && j + 1 < subDurations.Length)
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

                    if ( i + 1 < durations.Length )
                    {
                        // add ties across bar lines
                        writer.Write(" ~ ");
                    }
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
                    writer.WriteLine(@"");
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
    }
}