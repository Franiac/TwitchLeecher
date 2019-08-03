﻿using System;
using System.IO;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class DownloadParameters : BindableBase
    {
        #region Fields

        private readonly TwitchVideo _video;
        private readonly VodAuthInfo _vodAuthInfo;

        private TwitchVideoQuality _quality;

        private string _folder;
        private string _filename;

        private bool _autoSplit;
        private TimeSpan _autoSplitTime;

        private bool _cropStart;
        private bool _cropEnd;
        private bool _disableConversion;

        private TimeSpan _cropStartTime;
        private TimeSpan _cropEndTime;

        #endregion Fields

        #region Constructors

        public DownloadParameters(TwitchVideo video, VodAuthInfo vodAuthInfo, TwitchVideoQuality quality, string folder, string filename, TimeSpan autoTime, bool disableConversion)
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

            if (autoTime.Ticks > 0 && autoTime < video.Length)
            {
                _autoSplit = true;
            }
            _autoSplitTime = autoTime;

            _folder = folder;
            _filename = filename;
            _disableConversion = disableConversion;

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

        public bool DisableConversion
        {
            get
            {
                return _disableConversion;
            }
            set
            {
                SetProperty(ref _disableConversion, value, nameof(DisableConversion));
            }
        }

        public bool AutoSplit
        {
            get
            {
                return _autoSplit;
            }
            set
            {
                SetProperty(ref _autoSplit, value, nameof(AutoSplit));
                FirePropertyChanged(nameof(CroppedLength));
                //if (_cropAuto)
                //{
                //    if (CropEnd) CropEnd = false;
                //    if (CropStart) CropStart = false;
                //}
            }
        }

        public TimeSpan AutoSplitTime
        {
            get
            {
                return _autoSplitTime;
            }
            set
            {
                SetProperty(ref _autoSplitTime, value, nameof(AutoSplitTime));
                FirePropertyChanged(nameof(CroppedLength));
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

        public string VideoLengthStr
        {
            get
            {
                if (_cropStart || _cropEnd)
                    return $"{CroppedLength.ToDaylessString()} ({(_cropStart ? _cropStartTime.ToDaylessString() : "00:00:00")} - {(_cropEnd ? _cropEndTime.ToDaylessString() : Video.Length.ToDaylessString())})";
                else
                    return CroppedLength.ToDaylessString();
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
            }

            currentProperty = nameof(Filename);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_filename))
                {
                    AddError(currentProperty, "Please specify a filename!");
                }
                else if (_disableConversion && !_filename.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                {
                    AddError(currentProperty, "Filename must end with '.ts'!");
                }
                else if (!_disableConversion && !_filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
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
                        AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToDaylessString() + "'!");
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
                        AddError(currentProperty, "Please enter a value between '" + TimeSpan.Zero.ToString() + "' and '" + videoLength.ToDaylessString() + "'!");
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

            currentProperty = nameof(AutoSplitTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_autoSplit)
                {
                    if (_autoSplitTime.TotalSeconds < 60)
                    {
                        AddError(currentProperty, "The split time has to be at least 60s long!");
                    }
                    else if (!_filename.Contains(FilenameWildcards.UNIQNUMBER) && _autoSplitTime.TotalSeconds < _video.Length.TotalSeconds+60)
                    {
                        AddError(currentProperty, $"File name should contains '{FilenameWildcards.UNIQNUMBER}' for auto naming!");
                        AddError(nameof(Filename), $"File name should contains '{FilenameWildcards.UNIQNUMBER}' for auto naming!");
                    }
                }
            }
        }

        #endregion Methods
    }
}