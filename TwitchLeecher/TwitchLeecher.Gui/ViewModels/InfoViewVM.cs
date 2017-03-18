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

        private string _productName;

        private ICommand _openlinkCommand;
        private ICommand _donateCommand;

        private IDialogService _dialogService;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public InfoViewVM(IDialogService dialogService)
        {
            AssemblyUtil au = AssemblyUtil.Get;

            _productName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            _dialogService = dialogService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public string ProductName
        {
            get
            {
                return _productName;
            }
        }

        public ICommand OpenLinkCommand
        {
            get
            {
                if (_openlinkCommand == null)
                {
                    _openlinkCommand = new DelegateCommand<string>(OpenLink);
                }

                return _openlinkCommand;
            }
        }

        public ICommand DonateCommand
        {
            get
            {
                if (_donateCommand == null)
                {
                    _donateCommand = new DelegateCommand(Donate);
                }

                return _donateCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void OpenLink(string link)
        {
            try
            {
                lock (_commandLockObject)
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
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Donate()
        {
            try
            {
                lock (_commandLockObject)
                {
                    Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WYGSLTBJFMAVE");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}