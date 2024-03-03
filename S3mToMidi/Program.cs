using S3M;
using S3MParser;

string filename = args[0];
S3MFile file = S3MFile.Parse(filename);

MidiWriter2.Save(NoteEventGenerator.Generate(file).ToList(), Path.GetFileName(Path.ChangeExtension(filename, ".mid")));
