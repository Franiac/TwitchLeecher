using System;
using System.Collections.Generic;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Services.Services.Download
{
    public class Download
    {
        #region Fields

        private Guid _id;

        private IList<DownloadFileInfo> _fileInfoList;

        private DownloadState _state;

        private DateTime _startedAt;
        private DateTime _finishedAt;

        private int _priority;
        private int _rate;

        private string _error;

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

        public int Rate => _rate;

        public string Error => _error;

        public DateTime StartedAt => _startedAt;

        public DateTime FinishedAt => _finishedAt;

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

        #endregion

        #region Events

        public event EventHandler<DownloadEventArgs> StateChanged;

        private void FireStateChanged()
        {
            StateChanged?.Invoke(this, new DownloadEventArgs(this));
        }

        #endregion

    }
}