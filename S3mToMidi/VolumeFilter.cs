using S3M;

namespace S3mToMidi
{
    internal class VolumeFilter
    {
        public int VolumeThreshold { get; set; }

        public void Apply(S3MFile file)
        {
            foreach (var pattern in file.Patterns)
            {
                foreach (var row in pattern.Rows)
                {
                    foreach (ChannelEvent channelEvent in row.ChannelEvents)
                    {
                        if (channelEvent.HasVolume && channelEvent.Volume < VolumeThreshold)
                        {
                            channelEvent.Volume = 0;
                        }
                    }
                }
            }
        }
    }
}
