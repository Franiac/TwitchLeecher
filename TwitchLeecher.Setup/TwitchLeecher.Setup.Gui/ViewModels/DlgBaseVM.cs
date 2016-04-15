using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal abstract class DlgBaseVM : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        protected readonly SetupApplication bootstrapper;

        protected IGuiService guiService;

        protected Dictionary<string, string> currentErrors;

        protected string productNameVersionDisplay;

        #endregion Fields

        #region Constructors

        public DlgBaseVM(SetupApplication bootstrapper, IGuiService guiService)
        {
            if (bootstrapper == null)
            {
                throw new ArgumentNullException("bootstrapper");
            }

            if (guiService == null)
            {
                throw new ArgumentNullException("guiService");
            }

            this.bootstrapper = bootstrapper;
            this.guiService = guiService;

            this.currentErrors = new Dictionary<string, string>();
        }

        #endregion Constructors

        #region Properties

        public SetupApplication Bootstrapper
        {
            get
            {
                return this.bootstrapper;
            }
        }

        public string WizardTitle
        {
            get
            {
                return this.ProductNameVersionDisplay + " Setup";
            }
        }

        public virtual string AdditionalButtonText
        {
            get
            {
                return "Refresh";
            }
        }

        public virtual string NextButtonText
        {
            get
            {
                return "Next";
            }
        }

        public virtual bool IsAdditionalButtonVisible
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsUacShieldVisible
        {
            get
            {
                return false;
            }
        }

        public virtual ICommand AdditionalCommand
        {
            get
            {
                return null;
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
                return this.currentErrors.Count > 0;
            }
        }

        public string ProductNameVersionDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.productNameVersionDisplay))
                {
                    this.productNameVersionDisplay = this.bootstrapper.ProductName + " " + this.bootstrapper.ProductVersionTrimmed;
                }

                return this.productNameVersionDisplay;
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
            this.currentErrors.Clear();

            this.FireErrorsChanged();
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

        public virtual void OnBeforeNextView(CancelEventArgs e)
        {
            this.Validate();

            if (this.HasErrors)
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
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            this.Validate(propertyName);
        }

        protected void FirePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }

        #endregion INotifyPropertyChanged

        #region ErrorsChanged

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(string propertyName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void FireErrorsChanged(string propertyName = null)
        {
            this.OnErrorsChanged(propertyName);
        }

        #endregion ErrorsChanged

        #endregion Events
    }
}