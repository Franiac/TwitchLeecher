using System;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SearchWindowVM : DialogVM<SearchParameters>
    {
        #region Fields

        private string username;
        private VideoType videoType;
        private int loadLimit;

        private ICommand searchCommand;
        private ICommand cancelCommand;

        private ITwitchService twitchService;
        private IDialogService dialogService;

        private object searchLock = new object();

        #endregion Fields

        #region Constructors

        public SearchWindowVM(ITwitchService twitchService, IDialogService dialogService)
        {
            this.twitchService = twitchService;
            this.dialogService = dialogService;
        }

        #endregion Constructors

        #region Properties

        public string Username
        {
            get
            {
                return this.username;
            }
            set
            {
                this.SetProperty(ref this.username, value, nameof(this.Username));
            }
        }

        public VideoType VideoType
        {
            get
            {
                return this.videoType;
            }
            set
            {
                this.SetProperty(ref this.videoType, value, nameof(this.VideoType));
            }
        }

        public int LoadLimit
        {
            get
            {
                return this.loadLimit;
            }
            set
            {
                this.SetProperty(ref this.loadLimit, value, nameof(this.LoadLimit));
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (this.searchCommand == null)
                {
                    this.searchCommand = new DelegateCommand<Window>(this.Search);
                }

                return this.searchCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand<Window>(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Search(Window window)
        {
            try
            {
                this.dialogService.SetBusy();
                this.Validate();

                if (!this.HasErrors)
                {
                    this.ResultObject = new SearchParameters(this.username, this.videoType, this.loadLimit);
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel(Window window)
        {
            try
            {
                this.ResultObject = null;
                window.DialogResult = false;
                window.Close();
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Username);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.username))
                {
                    this.AddError(currentProperty, "Please specify a username!");
                }
                else if (!this.twitchService.UserExists(this.username))
                {
                    this.AddError(currentProperty, "The specified username does not exist on Twitch!");
                }
            }
        }

        #endregion Methods
    }
}