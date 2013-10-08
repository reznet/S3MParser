using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

namespace S3MParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //PrintS3MFile("01-v-drac.s3m");
            Console.Out.WriteLine();
            //PrintS3MFile("02-v-bewm.s3m");
            //Console.Out.WriteLine();
            //PrintS3MFile("06-v-cf2.s3m");
            //Console.Out.WriteLine();

            S3MFile file = S3MFile.Parse("01-v-drac.s3m");
            //S3MFile file = S3MFile.Parse("simple.s3m");
            //Console.Out.WriteLine(file.Name);
            PrintPatterns(from p in file.Patterns where p.PatternNumber == 4 select p);

            MidiWriter writer = new MidiWriter(file);
            writer.Save("out.mid");

        }

        private static string ConvertToPdf(string lilyPondFile)
        {
            string pdfFile = Path.ChangeExtension(lilyPondFile, ".pdf");

            Process lilypond = Process.Start(@"C:\Program Files (x86)\LilyPond\usr\bin\lilypond.exe", "\"" + lilyPondFile + "\"");
            lilypond.WaitForExit();

            return pdfFile;
        }

        private static string ConvertToLilyPond(string path)
        {
            string lilyPondFile = Path.ChangeExtension(path, ".ly");

            Process musicxml2ly = Process.Start(@"C:\Program Files (x86)\LilyPond\usr\bin\musicxml2ly.py", "-o \"" + lilyPondFile + "\" \"" + path + "\"");
            musicxml2ly.WaitForExit();

            return lilyPondFile;
        }

        private static void SaveXml(S3MFile file, string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<score-partwise version=\"1.1\"><part-list/></score-partwise>");
            // add part for each instrument
            for (int instrumentIndex = 0; instrumentIndex < file.InstrumentCount; instrumentIndex++)
            {
                XmlElement part = doc.CreateElement("score-part");
                XmlElement partList = (XmlElement)doc.DocumentElement.SelectSingleNode("part-list");
                partList.AppendChild(part);
                part.SetAttribute("id", instrumentIndex.ToString());
                XmlElement partName = doc.CreateElement("part-name");
                partName.InnerText = String.Format("Part {0}", instrumentIndex);
                part.AppendChild(partName);
            }

            foreach (Pattern pattern in file.Patterns)
            {
                foreach (ChannelEvent evt in pattern.EventsByChannel)
                {
                    //Console.Out.WriteLine(evt);
                }
                break;
            }

            Pattern p1 = file.Patterns[0];
            var c = from evt in p1.EventsByChannel
                    where evt.ChannelNumber == 1 && evt.Instrument == 2 && evt.Volume > 20
                    select evt;
            XmlElement part1 = doc.CreateElement("part");
            part1.SetAttribute("id", "1");
            doc.DocumentElement.AppendChild(part1);
            XmlElement measure = doc.CreateElement("measure");
            part1.AppendChild(measure);
            measure.SetAttribute("number", "1");
            measure.SetAttribute("width", "800");
            measure.InnerXml = "      <attributes>        <divisions>32</divisions>        <key>          <fifths>0</fifths>          <mode>major</mode>        </key>        <time>          <beats>4</beats>          <beat-type>4</beat-type>        </time>        <clef>          <sign>G</sign>          <line>2</line>        </clef>        <staff-details print-object=\"no\"/>      </attributes>";
            XmlElement beam = null;
            XmlElement lastNote = null;
            XmlElement firstNote = null;
            foreach (var evt in c)
            {
                Console.Out.WriteLine(evt);
                XmlElement note = doc.CreateElement("note");
                measure.AppendChild(note);
                lastNote = note;
                if (firstNote == null)
                {
                    firstNote = note;
                }
                XmlElement pitch = doc.CreateElement("pitch");
                note.AppendChild(pitch);
                int step = evt.Note & 15;
                int octave = evt.Note >> 4;

                bool alter = Alter[step] != 0;


                pitch.InnerXml = String.Format("<step>{0}</step><alter>{2}</alter><octave>{1}</octave>", Notes[step], octave, Alter[step]);
                Console.Out.WriteLine(pitch.InnerXml);
                XmlElement duration = doc.CreateElement("duration");
                note.AppendChild(duration);
                duration.InnerText = "1";
                XmlElement type = doc.CreateElement("type");
                note.AppendChild(type);
                type.InnerText = "eighth";
                XmlElement stem = doc.CreateElement("stem");
                note.AppendChild(stem);
                stem.InnerText = "up";
                if (beam == null)
                {
                    beam = doc.CreateElement("beam");
                    beam.SetAttribute("number", "1");
                    beam.InnerText = "begin";
                    note.AppendChild(beam);
                }
                else
                {
                    beam = doc.CreateElement("beam");
                    beam.SetAttribute("number", "1");
                    beam.InnerText = "end";
                }
                
                //break;
            }
            if (lastNote != firstNote)
            {
                lastNote.AppendChild(beam);
            }

            //Console.Out.WriteLine(doc.OuterXml);

            doc.Save(path);
        }

        static char[] Notes = { 'C', 'D', 'D', 'E', 'E', 'F', 'G', 'G', 'A', 'A', 'B', 'B' };
        static int[] Alter = { 0, -1, 0, -1, 0, 0, -1, 0, -1, 0, -1, 0};

        static string[] Pitches = { "C-", "C#", "D-", "D#", "E-", "F-", "F#", "G-", "G#", "A-", "A#", "B-" };

        private static void PrintPatterns(IEnumerable<Pattern> patterns)
        {
            foreach (Pattern pattern in patterns)
            {
                Console.Out.WriteLine("Pattern " + pattern.PatternNumber);
                HashSet<int> channels = new HashSet<int>();
                foreach (Row row in pattern.Rows)
                {
                    foreach (ChannelEvent channelEvent in row.ChannelEvents)
                    {
                        if (!channels.Contains(channelEvent.ChannelNumber))
                        {
                            channels.Add(channelEvent.ChannelNumber);
                        }
                    }
                }
                List<int> channelHeaders = new List<int>(channels.ToList());
                channelHeaders.Sort();

                foreach (int channelHeader in channelHeaders)
                {
                    Console.Out.Write("Channel {0,5}|", channelHeader);
                }
                Console.Out.WriteLine();

                foreach (Row row in pattern.Rows)
                {
                    int channelIndex = 0;
                    for(int cIndex = 0; cIndex < row.ChannelEvents.Count; cIndex++)
                    {
                        ChannelEvent evt = row.ChannelEvents[cIndex];
                        while(channelIndex < channelHeaders.Count && channelHeaders[channelIndex] < evt.ChannelNumber)
                        {
                            Console.Out.Write("___ __ __ ___|");
                            channelIndex++;
                        }
                        
                        Console.Out.Write("{0} {1} {2} {3}|", NoteValueToString(evt.Note), InstrumentToString(evt.Instrument), VolumeToString(evt.Volume), CommandValueToString(evt.Command, evt.Data));
                        channelIndex++;
                    }
                    while (channelIndex < channelHeaders.Count)
                    {
                        Console.Out.Write("___ __ __ ___|");
                        channelIndex++;
                    }
                    Console.Out.WriteLine();
                }

                Console.Out.WriteLine();
                Console.Out.WriteLine();
                
            }
        }

        private static object VolumeToString(int volume)
        {
            if (volume == -1) { return "  "; }
            return volume.ToString("00");
        }

        private static object InstrumentToString(int instrument)
        {
            if (instrument == -1) { return "  "; }
            return instrument.ToString("X2");
        }

        private static string NoteValueToString(int note)
        {
            if (note == -1) { return "   "; }
            if (note == 0xFF) { return "nil"; }
            if (note == 0xFE) { return "off"; }
            int step = note & 15;
            int octave = note >> 4;

            return Pitches[step] + (octave+1).ToString();
        }

        private static string CommandValueToString(int command, int data)
        {
            if (command == -1) { return "   "; }
            return ((char)(((int)'A') + (command - 1))).ToString() + data.ToString("X2");
        }

        private static void PrintS3MFile(S3MFile file)
        {
            Type type = typeof(S3MFile);
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                if (fieldInfo.FieldType.IsArray)
                {
                    Array values = (Array)fieldInfo.GetValue(file);
                    StringBuilder sb = new StringBuilder();
                    sb.Append("[");
                    foreach (object obj in values)
                    {
                        sb.AppendFormat("{0};", obj);
                    }
                    sb.Append("]");
                    Console.Out.WriteLine("{0}:{1}", fieldInfo.Name, sb.ToString());
                }
                else
                {
                    Console.Out.WriteLine("{0}:{1}", fieldInfo.Name, fieldInfo.GetValue(file));
                }
            }
        }

        private static void PrintS3MFile(string file)
        {
            Console.Out.Write("      |");
            for (int i = 0; i < 0x10; i++)
            {
                Console.Out.Write("{0:X3}|", i);
            }
            Console.Out.WriteLine();
            using (FileStream stream = new FileStream(file, FileMode.Open))
            {
                int bufferSize = 16;
                int readCount = 0;
                byte[] buffer = new byte[bufferSize];
                int streamIndex = 0;

                while (true)
                {
                    readCount = stream.Read(buffer, 0, bufferSize);

                    Console.Out.Write("{0:X6}|", streamIndex);

                    WriteBufferLineHex(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.Write("      |");
                    WriteBufferLineDecimal(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.Write("      |");
                    WriteBufferLineChar(buffer, readCount);
                    Console.Out.WriteLine();
                    Console.Out.WriteLine();

                    streamIndex += readCount;

                    if (readCount < bufferSize)
                    {
                        break;
                    }
                    if (stream.Position > bufferSize * 4)
                    {
                        //break;
                    }
                }
            }
        }

        private static void WriteBufferLineHex(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                Console.Out.Write("{0:X3}|", buffer[bufferIndex]);
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }

        private static void WriteBufferLineDecimal(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                Console.Out.Write("{0:D3}|", buffer[bufferIndex]);
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }

        private static void WriteBufferLineChar(byte[] buffer, int readCount)
        {
            for (int bufferIndex = 0; bufferIndex < readCount; bufferIndex++)
            {
                char c = (char)buffer[bufferIndex];
                byte b = buffer[bufferIndex];
                if (b < 32 || 126 < b)
                {
                    Console.Out.Write("???|");
                }
                else
                {
                    Console.Out.Write("{0,3}|", c);
                }
            }
            for (int leftOverCount = readCount; leftOverCount < buffer.Length; leftOverCount++)
            {
                Console.Out.Write("  ");
            }
        }

    }
}
