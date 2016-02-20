using System;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoResolution
    {
        #region Constatnts

        private const string UNKNOWN_FPS = "Unknown";

        #endregion Constatnts

        #region Fields

        private VideoQuality videoQuality;

        private string fps;
        private string resolution;
        private string resolutionFps;

        #endregion Fields

        #region Constructors

        public TwitchVideoResolution(VideoQuality videoQuality, string resolution, string fps)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            this.videoQuality = videoQuality;
            this.resolution = resolution;

            if (string.IsNullOrWhiteSpace(fps))
            {
                this.fps = UNKNOWN_FPS;
                this.resolutionFps = resolution;
            }
            else
            {
                this.fps = fps;
                this.resolutionFps = resolution + "@" + fps + "fps";
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