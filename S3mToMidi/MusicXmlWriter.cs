using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace S3MParser
{
    class MusicXmlWriter
    {
        static int[] Alter = { 0, -1, 0, -1, 0, 0, -1, 0, -1, 0, -1, 0 };
        static char[] Notes = { 'C', 'D', 'D', 'E', 'E', 'F', 'G', 'G', 'A', 'A', 'B', 'B' };

        public static void SaveXml(S3MFile file, string path)
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
    }
}
