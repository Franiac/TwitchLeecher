using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal abstract class DlgBaseVM : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        protected readonly SetupApplication _bootstrapper;

        protected IGuiService _guiService;

        protected Dictionary<string, string> _currentErrors;

        protected string _productNameVersionDisplay;

        #endregion Fields

        #region Constructors

        public DlgBaseVM(SetupApplication bootstrapper, IGuiService guiService)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException("bootstrapper");
            _guiService = guiService ?? throw new ArgumentNullException("guiService");

            _currentErrors = new Dictionary<string, string>();
        }

        #endregion Constructors

        #region Properties

        public SetupApplication Bootstrapper
        {
            get
            {
                return _bootstrapper;
            }
        }

        public string WizardTitle
        {
            get
            {
                return ProductNameVersionDisplay + " Setup";
            }
        }

        public virtual string NextButtonText
        {
            get
            {
                return "Next";
            }
        }

        public virtual bool IsUacIconVisible
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsBackButtonEnabled
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsNextButtonEnabled
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsCancelButtonEnabled
        {
            get
            {
                return true;
            }
        }

        public bool HasErrors
        {
            get
            {
                return _currentErrors.Count > 0;
            }
        }

        public string ProductNameVersionDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_productNameVersionDisplay))
                {
                    _productNameVersionDisplay = _bootstrapper.ProductName + " " + _bootstrapper.ProductVersionTrimmed;
                }

                return _productNameVersionDisplay;
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
            _currentErrors.Clear();

            FireErrorsChanged();
        }

        protected virtual void Validate(string propertyName = null)
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

        public virtual void OnBeforeNextView(CancelEventArgs e)
        {
            Validate();

            if (HasErrors)
            {
                e.Cancel = true;
            }
        }

        #endregion Methods

        #region Events

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            Validate(propertyName);
        }

        protected void FirePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        #endregion INotifyPropertyChanged

        #region ErrorsChanged

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void FireErrorsChanged(string propertyName = null)
        {
            OnErrorsChanged(propertyName);
        }

        #endregion ErrorsChanged

        #endregion Events
    }
}