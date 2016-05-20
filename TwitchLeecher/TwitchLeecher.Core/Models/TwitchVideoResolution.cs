using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoResolution
    {
        #region Constants

        private const string UNKNOWN = "Unknown";

        #endregion Constants

        #region Fields

        private VideoQuality videoQuality;

        private string fps;
        private string resolution;
        private string resolutionFps;

        #endregion Fields

        #region Constructors

        public TwitchVideoResolution(VideoQuality videoQuality, string resolution, string fps)
        {
            this.videoQuality = videoQuality;
            this.resolution = string.IsNullOrWhiteSpace(resolution) ? UNKNOWN : resolution;
            this.fps = string.IsNullOrWhiteSpace(fps) ? UNKNOWN : fps;
            
            if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
            {
                this.resolutionFps = resolution + "@" + fps + "fps";                
            }
            else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
            {
                this.resolutionFps = resolution;
            }
            else
            {
                this.resolutionFps = UNKNOWN;
            }
        }

        #endregion Constructors

        #region Properties

        public VideoQuality VideoQuality
        {
            get
            {
                return this.videoQuality;
            }
        }

        public string Resolution
        {
            get
            {
                return this.resolution;
            }
        }

        public string Fps
        {
            get
            {
                return this.fps;
            }
        }

        public string ResolutionFps
        {
            get
            {
                return this.resolutionFps;
            }
        }

        #endregion Properties
    }
}