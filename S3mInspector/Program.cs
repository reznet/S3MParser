using S3M;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3mInspector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filename = args[0];

            Console.WriteLine("Analyzing '{0}'...", filename);

            S3MFile file = S3MFile.Parse(filename);

            Console.WriteLine("Pattern Count: {0}", file.PatternCount);
            Console.WriteLine("Minimum Channel: {0}", file.Patterns.SelectMany(p => p.Rows.SelectMany(r => r.ChannelEvents)).Min(e => e.ChannelNumber));
            Console.WriteLine("Maximum Channel: {0}", file.Patterns.SelectMany(p => p.Rows.SelectMany(r => r.ChannelEvents)).Max(e => e.ChannelNumber));
        }
    }
}
