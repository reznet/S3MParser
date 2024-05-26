using System.Text;

namespace S3mToMidi.LilyPond
{
    public class LilyPondTextWriter
    {
        private readonly List<Placeholder> texts = new List<Placeholder>();

        public void Write(string text)
        {
            Write(new Placeholder() { Text = text });
        }

        public void WriteLine(string text)
        {
            Write(new Placeholder() { Text = text });
            Write(new Placeholder() { Text = Environment.NewLine });
        }

        public void WriteLine()
        {
            Write(new Placeholder() { Text = Environment.NewLine });
        }

        public void Write(Placeholder placeholder)
        {
            texts.Add(placeholder);
        }

        public Placeholder AppendPlaceholder()
        {
            var placeholder = new Placeholder();
            texts.Add(placeholder);
            return placeholder;
        }

        public void Flush(TextWriter textWriter)
        {
            foreach (var text in texts)
            {
                textWriter.Write(text);
            }
        }
    }

    public class Placeholder
    {
        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return Text;
        }
    }
}