using System;
using System.Collections.Generic;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Services.Services.Download
{
    public class Download
    {
        #region Fields

        private Guid _id;

        private readonly IList<DownloadFileInfo> _fileInfoList;

        private readonly DownloadState _state;

        private int _priority;

        #endregion Fields

        #region Constructors

        public Download(Guid id, IList<DownloadFileInfo> fileInfoList, int priority)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Empty Guid is not allowed", nameof(id));
            }

            if (fileInfoList == null)
            {
                throw new ArgumentNullException(nameof(fileInfoList));
            }

            if (fileInfoList.Count == 0)
            {
                throw new ArgumentException("List must contain at least one item", nameof(fileInfoList));
            }

            CheckPriority(priority);

            _id = id;
            _fileInfoList = fileInfoList;
            _state = DownloadState.Queued;
            _priority = priority;
        }

        #endregion Constructors

        #region Properties

        public int Priority
        {
            get => _priority;
            set
            {
                CheckPriority(value);
                _priority = value;
            }
        }

        public DownloadState State => _state;

        public int Rate { get; set; }

        #endregion Properties

        #region Methods

        private void CheckPriority(int priority)
        {
            if (priority < 0)
            {
                throw new ArgumentException("Negative value is not allowed", nameof(priority));
            }
        }

        public void Start()
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void Cancel()
        {
        }

        #endregion Methods

        #region Events

        public event EventHandler<DownloadEventArgs> StateChanged;

        private void FireStateChanged()
        {
            StateChanged?.Invoke(this, new DownloadEventArgs(this));
        }

        #endregion Events
    }
}