namespace TwitchLeecher.Core.Models
{
    public class CropInfo
    {
        public CropInfo(bool cropStart, bool cropEnd, double start, double length)
        {
            CropStart = cropStart;
            CropEnd = cropEnd;
            Start = start;
            Length = length;
        }

        public bool CropStart { get; private set; }

        public bool CropEnd { get; private set; }

        public double Start { get; private set; }

        public double Length { get; private set; }
    }
}