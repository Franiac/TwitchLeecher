using System;
using System.IO;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class DownloadParameters : BindableBase
    {
        #region Fields

        private TwitchVideo _video;
        private TwitchVideoQuality _quality;
        private VodAuthInfo _vodAuthInfo;

        private string _folder;
        private string _filename;

        private bool _cropStart;
        private bool _cropEnd;

        private TimeSpan _cropStartTime;
        private TimeSpan _cropEndTime;

        #endregion Fields

        #region Constructors

        public DownloadParameters(TwitchVideo video, TwitchVideoQuality quality, VodAuthInfo vodAuthInfo, string folder, string filename)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            _video = video ?? throw new ArgumentNullException(nameof(video));
            _quality = quality ?? throw new ArgumentNullException(nameof(quality));
            _vodAuthInfo = vodAuthInfo ?? throw new ArgumentNullException(nameof(vodAuthInfo));

            _folder = folder;
            _filename = filename;

            _cropEndTime = video.Length;
        }

        #endregion Constructors

        #region Properties

        public TwitchVideo Video
        {
            get
            {
                return _video;
            }
        }

        public TwitchVideoQuality Quality
        {
            get
            {
                return _quality;
            }
            set
            {
                SetProperty(ref _quality, value, nameof(Quality));
            }
        }

        public VodAuthInfo VodAuthInfo
        {
            get
            {
                return _vodAuthInfo;
            }
        }

        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                SetProperty(ref _folder, value, nameof(Folder));
                FirePropertyChanged(nameof(FullPath));
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                SetProperty(ref _filename, value, nameof(Filename));
                FirePropertyChanged(nameof(FullPath));
            }
        }

        public string FullPath
        {
            get
            {
                return Path.Combine(_folder, _filename);
            }
        }

        public bool CropStart
        {
            get
            {
                return _cropStart;
            }
            set
            {
                SetProperty(ref _cropStart, value, nameof(CropStart));
                FirePropertyChanged(nameof(CroppedLength));
            }
        }

        public TimeSpan CropStartTime
        {
            get
            {
                return _cropStartTime;
            }
            set
            {
                SetProperty(ref _cropStartTime, value, nameof(CropStartTime));
                FirePropertyChanged(nameof(CroppedLength));
            }
        }

        public bool CropEnd
        {
            get
            {
                return _cropEnd;
            }
            set
            {
                SetProperty(ref _cropEnd, value, nameof(CropEnd));
                FirePropertyChanged(nameof(CroppedLength));
            }
        }

        public TimeSpan CropEndTime
        {
            get
            {
                return _cropEndTime;
            }
            set
            {
                SetProperty(ref _cropEndTime, value, nameof(CropEndTime));
                FirePropertyChanged(nameof(CroppedLength));
            }
        }

        public TimeSpan CroppedLength
        {
            get
            {
                if (!_cropStart && !_cropEnd)
                {
                    return _video.Length;
                }
                else if (!_cropStart && _cropEnd)
                {
                    return _cropEndTime;
                }
                else if (_cropStart && !_cropEnd)
                {
                    return _video.Length - _cropStartTime;
                }
                else
                {
                    return _cropEndTime - _cropStartTime;
                }
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(Quality);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_quality == null)
                {
                    AddError(currentProperty, "Please select a quality!");
                }
            }

            currentProperty = nameof(Folder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_folder))
                {
                    AddError(currentProperty, "Please specify a folder!");
                }
                else if (!Directory.Exists(_folder))
                {
                    AddError(currentProperty, "The specified folder does not exist!");
                }
                else if (!FileSystem.HasWritePermission(_folder))
                {
                    AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(Filename);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_filename))
                {
                    AddError(currentProperty, "Please specify a filename!");
                }
                else if (!_filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    AddError(currentProperty, "Filename must end with '.mp4'!");
                }
                else if (FileSystem.FilenameContainsInvalidChars(_filename))
                {
                    AddError(currentProperty, "Filename contains invalid characters!");
                }
            }

            currentProperty = nameof(CropStartTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_cropStart)
                {
                    TimeSpan videoLength = _video.Length;

                    if (_cropStartTime < TimeSpan.Zero || _cropStartTime > videoLength)
                    {
                        AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToString() + "'!");
                    }
                    else if (CroppedLength.TotalSeconds < 5)
                    {
                        AddError(currentProperty, "The cropped video has to be at least 5s long!");
                    }
                }
            }

            currentProperty = nameof(CropEndTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_cropEnd)
                {
                    TimeSpan videoLength = _video.Length;

                    if (_cropEndTime < TimeSpan.Zero || _cropEndTime > videoLength)
                    {
                        AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToString() + "'!");
                    }
                    else if (_cropStart && (_cropEndTime <= _cropStartTime))
                    {
                        AddError(currentProperty, "End time has to be greater than start time!");
                    }
                    else if (CroppedLength.TotalSeconds < 5)
                    {
                        AddError(currentProperty, "The cropped video has to be at least 5s long!");
                    }
                }
            }
        }

        #endregion Methods
    }
}