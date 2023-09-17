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

        private ICommand _openlinkCommand;
        private ICommand _donateCommand;

        private readonly IDialogService _dialogService;
        private readonly IDonationService _donationService;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public InfoViewVM(IDialogService dialogService, IDonationService donationService)
        {
            AssemblyUtil au = AssemblyUtil.Get;

            ProductName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            _dialogService = dialogService;
            _donationService = donationService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public string ProductName { get; }

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

                    var psi = new ProcessStartInfo(link)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(psi);
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
                    _donationService.OpenDonationPage();
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