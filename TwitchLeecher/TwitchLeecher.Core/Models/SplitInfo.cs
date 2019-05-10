namespace TwitchLeecher.Core.Models
{
    public class SplitInfo
    {
        public SplitInfo(bool splitVideo, double splitLength)
        {
            SplitVideo = splitVideo;
            SplitLength = splitLength;
        }

        public bool SplitVideo { get; private set; }

        public double SplitLength { get; private set; }
    }
}