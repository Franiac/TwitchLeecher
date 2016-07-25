using System;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class InfoViewVM : ViewModelBase
    {
        #region Fields

        private string productName;

        private ICommand openlinkCommand;
        private ICommand donateCommand;

        private IDialogService dialogService;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public InfoViewVM(IDialogService dialogService)
        {
            AssemblyUtil au = AssemblyUtil.Get;

            this.productName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.dialogService = dialogService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public string ProductName
        {
            get
            {
                return this.productName;
            }
        }

        public ICommand OpenLinkCommand
        {
            get
            {
                if (this.openlinkCommand == null)
                {
                    this.openlinkCommand = new DelegateCommand<string>(this.OpenLink);
                }

                return this.openlinkCommand;
            }
        }

        public ICommand DonateCommand
        {
            get
            {
                if (this.donateCommand == null)
                {
                    this.donateCommand = new DelegateCommand(this.Donate);
                }

                return this.donateCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void OpenLink(string link)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (string.IsNullOrWhiteSpace(link))
                    {
                        throw new ArgumentNullException(nameof(link));
                    }

                    Process.Start(link);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Donate()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WYGSLTBJFMAVE");
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}