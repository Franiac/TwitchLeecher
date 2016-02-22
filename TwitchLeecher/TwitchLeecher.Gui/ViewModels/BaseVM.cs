using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.ViewModels
{
    public abstract class BaseVM : BindableBase, INotifyDataErrorInfo
    {
        #region Fields

        protected Dictionary<string, string> currentErrors;

        protected string productNameVersionDisplay;

        #endregion Fields

        #region Constructors

        public BaseVM()
        {
            this.currentErrors = new Dictionary<string, string>();
        }

        #endregion Constructors

        #region Properties

        public bool HasErrors
        {
            get
            {
                return this.currentErrors.Count > 0;
            }
        }

        #endregion Properties

        #region Methods

        public IEnumerable GetErrors(string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return this.currentErrors.Values.ToList();
            }
            else if (this.currentErrors.ContainsKey(propertyName))
            {
                return new List<string>() { this.currentErrors[propertyName] };
            }

            return null;
        }

        protected void AddError(string propertyName, string error)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentNullException("error");
            }

            if (!this.currentErrors.ContainsKey(propertyName))
            {
                this.currentErrors.Add(propertyName, error);
            }

            this.FireErrorsChanged(propertyName);
        }

        protected void RemoveError(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            this.currentErrors.Remove(propertyName);

            this.FireErrorsChanged(propertyName);
        }

        protected void ClearErrors()
        {
            List<string> errorKeys = this.currentErrors.Keys.ToList();

            foreach (string propertyName in errorKeys)
            {
                this.currentErrors.Remove(propertyName);
                this.FireErrorsChanged(propertyName);
            }
        }

        protected virtual void Validate(string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                this.ClearErrors();
            }
            else
            {
                this.RemoveError(propertyName);
            }
        }

        #endregion Methods

        #region Events

        #region ErrorsChanged

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(string propertyName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void FireErrorsChanged(string propertyName)
        {
            this.OnErrorsChanged(propertyName);
        }

        #endregion ErrorsChanged

        #endregion Events
    }
}