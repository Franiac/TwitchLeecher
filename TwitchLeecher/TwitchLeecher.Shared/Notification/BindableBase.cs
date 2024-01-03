using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace TwitchLeecher.Shared.Notification
{
    public abstract class BindableBase : ReactiveObject, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        protected Dictionary<string, string> _currentErrors;

        #endregion Fields

        #region Constructors

        public BindableBase()
        {
            _currentErrors = new Dictionary<string, string>();
        }

        #endregion Constructors

        #region Properties

        public bool HasErrors
        {
            get
            {
                return _currentErrors.Count > 0;
            }
        }

        #endregion Properties

        #region Methods

        public IEnumerable GetErrors(string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return _currentErrors.Values.ToList();
            }
            else if (_currentErrors.ContainsKey(propertyName))
            {
                return new List<string>() { _currentErrors[propertyName] };
            }

            return null;
        }

        public void AddError(string propertyName, string error)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentNullException("error");
            }

            if (!_currentErrors.ContainsKey(propertyName))
            {
                _currentErrors.Add(propertyName, error);
            }

            FireErrorsChanged(propertyName);
        }

        protected void RemoveError(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            _currentErrors.Remove(propertyName);

            FireErrorsChanged(propertyName);
        }

        protected void ClearErrors()
        {
            List<string> errorKeys = _currentErrors.Keys.ToList();

            foreach (string propertyName in errorKeys)
            {
                _currentErrors.Remove(propertyName);
                FireErrorsChanged(propertyName);
            }
        }

        public virtual void Validate(string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                ClearErrors();
            }
            else
            {
                RemoveError(propertyName);
            }
        }

        #endregion Methods

        #region Events

        #region ErrorsChanged

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void FireErrorsChanged(string propertyName)
        {
            OnErrorsChanged(propertyName);
        }

        #endregion ErrorsChanged

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;

            FirePropertyChanged(propertyName);

            return true;
        }

        protected virtual void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanged

        #endregion Events
    }
}