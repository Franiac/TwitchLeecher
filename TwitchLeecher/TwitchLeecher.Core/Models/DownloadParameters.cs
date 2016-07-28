using System;
using System.IO;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class DownloadParameters : BindableBase
    {
        #region Fields

        private TwitchVideo video;
        private TwitchVideoResolution resolution;
        private VodAuthInfo vodAuthInfo;

        private string folder;
        private string filename;

        private bool cropStart;
        private bool cropEnd;

        private TimeSpan cropStartTime;
        private TimeSpan cropEndTime;

        #endregion Fields

        #region Constructors

        public DownloadParameters(TwitchVideo video, TwitchVideoResolution resolution, VodAuthInfo vodAuthInfo, string folder, string filename)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            if (vodAuthInfo == null)
            {
                throw new ArgumentNullException(nameof(vodAuthInfo));
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            this.video = video;
            this.resolution = resolution;
            this.vodAuthInfo = vodAuthInfo;
            this.folder = folder;
            this.filename = filename;

            this.CropEndTime = video.Length;
        }

        #endregion Constructors

        #region Properties

        public TwitchVideo Video
        {
            get
            {
                return this.video;
            }
        }

        public TwitchVideoResolution Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                this.SetProperty(ref this.resolution, value, nameof(this.Resolution));
            }
        }

        public VodAuthInfo VodAuthInfo
        {
            get
            {
                return this.vodAuthInfo;
            }
        }

        public string Folder
        {
            get
            {
                return this.folder;
            }
            set
            {
                this.SetProperty(ref this.folder, value, nameof(this.Folder));
                this.FirePropertyChanged(nameof(this.FullPath));
            }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.SetProperty(ref this.filename, value, nameof(this.Filename));
                this.FirePropertyChanged(nameof(this.FullPath));
            }
        }

        public string FullPath
        {
            get
            {
                return Path.Combine(this.folder, this.filename);
            }
        }

        public bool CropStart
        {
            get
            {
                return this.cropStart;
            }
            set
            {
                this.SetProperty(ref this.cropStart, value, nameof(this.CropStart));
                this.FirePropertyChanged(nameof(this.CroppedLength));
            }
        }

        public TimeSpan CropStartTime
        {
            get
            {
                return this.cropStartTime;
            }
            set
            {
                this.SetProperty(ref this.cropStartTime, value, nameof(this.CropStartTime));
                this.FirePropertyChanged(nameof(this.CroppedLength));
            }
        }

        public bool CropEnd
        {
            get
            {
                return this.cropEnd;
            }
            set
            {
                this.SetProperty(ref this.cropEnd, value, nameof(this.CropEnd));
                this.FirePropertyChanged(nameof(this.CroppedLength));
            }
        }

        public TimeSpan CropEndTime
        {
            get
            {
                return this.cropEndTime;
            }
            set
            {
                this.SetProperty(ref this.cropEndTime, value, nameof(this.CropEndTime));
                this.FirePropertyChanged(nameof(this.CroppedLength));
            }
        }

        public TimeSpan CroppedLength
        {
            get
            {
                if (!this.cropStart && !this.cropEnd)
                {
                    return this.video.Length;
                }
                else if (!this.cropStart && this.cropEnd)
                {
                    return this.cropEndTime;
                }
                else if (this.cropStart && !this.cropEnd)
                {
                    return this.video.Length - this.cropStartTime;
                }
                else
                {
                    return this.cropEndTime - this.cropStartTime;
                }
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Resolution);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.resolution == null)
                {
                    this.AddError(currentProperty, "Please select a quality!");
                }
            }

            currentProperty = nameof(this.Folder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.folder))
                {
                    this.AddError(currentProperty, "Please specify a folder!");
                }
                else if (!Directory.Exists(this.folder))
                {
                    this.AddError(currentProperty, "The specified folder does not exist!");
                }
                else if (!FileSystem.HasWritePermission(this.folder))
                {
                    this.AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(this.Filename);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.filename))
                {
                    this.AddError(currentProperty, "Please specify a filename!");
                }
                else if (!this.filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddError(currentProperty, "Filename must end with '.mp4'!");
                }
                else if (FileSystem.FilenameContainsInvalidChars(this.filename))
                {
                    this.AddError(currentProperty, "Filename contains invalid characters!");
                }
            }

            currentProperty = nameof(this.CropStartTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.cropStart)
                {
                    TimeSpan videoLength = this.video.Length;

                    if (this.cropStartTime < TimeSpan.Zero || this.cropStartTime > videoLength)
                    {
                        this.AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToString() + "'!");
                    }
                    else if (this.CroppedLength.TotalSeconds < 5)
                    {
                        this.AddError(currentProperty, "The cropped video has to be at least 5s long!");
                    }
                }
            }

            currentProperty = nameof(this.CropEndTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.cropEnd)
                {
                    TimeSpan videoLength = this.video.Length;

                    if (this.cropEndTime < TimeSpan.Zero || this.cropEndTime > videoLength)
                    {
                        this.AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToString() + "'!");
                    }
                    else if (this.cropStart && (this.cropEndTime <= this.cropStartTime))
                    {
                        this.AddError(currentProperty, "End time has to be greater than start time!");
                    }
                    else if (this.CroppedLength.TotalSeconds < 5)
                    {
                        this.AddError(currentProperty, "The cropped video has to be at least 5s long!");
                    }
                }
            }
        }

        #endregion Methods
    }
}