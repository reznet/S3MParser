namespace S3mToMidi
{
    /// <summary>
    /// How to output instruments in a tracker channel.
    /// </summary>
    internal enum ChannelInstrumentOutputBehavior
    {
        /// <summary>
        /// Indicates that all instruments on a single tracker channel are output to the same midi channel.
        /// </summary>
        Collapse,

        /// <summary>
        /// Indicates that each instrument in a tracker channel gets its own midi channel.
        /// </summary>
        Explode
    }
}