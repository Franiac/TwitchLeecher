using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TwitchLeecher.Setup.Gui.Command;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class WizardWindowVM : INotifyPropertyChanged
    {
        #region Fields

        private readonly SetupApplication _bootstrapper;

        private readonly IGuiService _guiService;
        private readonly IUacService _uacService;

        private Dictionary<Type, DlgBaseVM> _viewModels;

        private DlgBaseVM _currentViewModel;

        private ProgressDlgVM _progressDlgVM;
        private UserCancelDlgVM _userCancelDlgVM;
        private ErrorDlgVM _errorDlgVM;
        private FinishedDlgVM _finishedDlgVM;

        private ICommand _backCommand;
        private ICommand _nextCommand;
        private ICommand _cancelCommand;
        private ICommand _exitCommand;

        private volatile bool _cancelConfirmed;

        #endregion Fields

        public WizardWindowVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException("bootstrapper");
            _guiService = guiService ?? throw new ArgumentNullException("guiService");
            _uacService = uacService ?? throw new ArgumentNullException("uacService");

            _bootstrapper.CancelProgressRequestedChanged += Bootstrapper_CancelProgressRequestedChanged;

            CreateViewModels();
        }

        #region Properties

        public DlgBaseVM CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentViewModel = value;

                FirePropertyChanged(null);
            }
        }

        public string WizardTitle
        {
            get
            {
                return CurrentViewModel.WizardTitle;
            }
        }

        public string NextButtonText
        {
            get
            {
                return CurrentViewModel.NextButtonText;
            }
        }

        public bool IsNextButtonEnabled
        {
            get
            {
                return CurrentViewModel.IsNextButtonEnabled;
            }
        }

        public bool IsBackButtonEnabled
        {
            get
            {
                return CurrentViewModel.IsBackButtonEnabled;
            }
        }

        public bool IsCancelButtonEnabled
        {
            get
            {
                return CurrentViewModel.IsCancelButtonEnabled && !_bootstrapper.CancelProgressRequested;
            }
        }

        public bool IsUacIconVisible
        {
            get
            {
                return CurrentViewModel.IsUacIconVisible;
            }
        }

        public BitmapImage UacIcon
        {
            get
            {
                return _uacService.UacIcon;
            }
        }

        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new DelegateCommand(() =>
                    {
                        _guiService.SetBusy();
                        ShowPreviousView();
                    });
                }

                return _backCommand;
            }
        }

        public ICommand NextCommand
        {
            get
            {
                if (_nextCommand == null)
                {
                    _nextCommand = new DelegateCommand(() =>
                    {
                        _guiService.SetBusy();
                        ShowNextView();
                    });
                }

                return _nextCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(CancelWizard);
                }

                return _cancelCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new DelegateCommand(() => { }, CanExitWizard);
                }

                return _exitCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void CreateViewModels()
        {
            _viewModels = new Dictionary<Type, DlgBaseVM>();

            _progressDlgVM = new ProgressDlgVM(_bootstrapper, _guiService);
            _userCancelDlgVM = new UserCancelDlgVM(_bootstrapper, _guiService);
            _errorDlgVM = new ErrorDlgVM(_bootstrapper, _guiService);
            _finishedDlgVM = new FinishedDlgVM(_bootstrapper, _guiService);

            if (!Environment.Is64BitOperatingSystem)
            {
                BitErrorDlgVM bitErrorDlgVM = new BitErrorDlgVM(_bootstrapper, _guiService);
                _viewModels.Add(typeof(BitErrorDlgVM), bitErrorDlgVM);
                _currentViewModel = bitErrorDlgVM;
            }
            else if (_bootstrapper.IsUpgrade)
            {
                UpgradeDlgVM upgradeDlgVM = new UpgradeDlgVM(_bootstrapper, _guiService, _uacService);
                _viewModels.Add(typeof(UpgradeDlgVM), upgradeDlgVM);
                _currentViewModel = upgradeDlgVM;
            }
            else if (_bootstrapper.HasRelatedBundle)
            {
                DowngradeDlgVM downgradeDlgVM = new DowngradeDlgVM(_bootstrapper, _guiService);
                _viewModels.Add(typeof(DowngradeDlgVM), downgradeDlgVM);
                _currentViewModel = downgradeDlgVM;
            }
            else
            {
                LaunchAction launchAction = _bootstrapper.LaunchAction;

                switch (launchAction)
                {
                    case LaunchAction.Install:
                        _viewModels.Add(typeof(WelcomeDlgVM), new WelcomeDlgVM(_bootstrapper, _guiService));
                        _viewModels.Add(typeof(LicenseDlgVM), new LicenseDlgVM(_bootstrapper, _guiService));
                        _viewModels.Add(typeof(CustomizeDlgVM), new CustomizeDlgVM(_bootstrapper, _guiService));
                        _viewModels.Add(typeof(ReadyDlgVM), new ReadyDlgVM(_bootstrapper, _guiService, _uacService));
                        _currentViewModel = _viewModels[typeof(WelcomeDlgVM)];
                        break;

                    case LaunchAction.Uninstall:
                        UninstallDlgVM uninstallDlgVM = new UninstallDlgVM(_bootstrapper, _guiService, _uacService);
                        _viewModels.Add(typeof(UninstallDlgVM), uninstallDlgVM);
                        _currentViewModel = uninstallDlgVM;
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + _bootstrapper.LaunchAction.ToString() + "'!");
                }
            }
        }

        private void ShowPreviousView()
        {
            LaunchAction launchAction = _bootstrapper.LaunchAction;

            DlgBaseVM currentVM = CurrentViewModel;
            DlgBaseVM previousVM = null;

            if (launchAction == LaunchAction.Install)
            {
                if (currentVM is LicenseDlgVM)
                {
                    previousVM = _viewModels[typeof(WelcomeDlgVM)];
                }
                else if (currentVM is CustomizeDlgVM)
                {
                    previousVM = _viewModels[typeof(LicenseDlgVM)];
                }
                else if (currentVM is ReadyDlgVM)
                {
                    previousVM = _viewModels[typeof(CustomizeDlgVM)];
                }
            }

            if (previousVM != null)
            {
                CurrentViewModel = previousVM;
            }
        }

        private void ShowNextView()
        {
            DlgBaseVM currentVM = CurrentViewModel;

            CancelEventArgs cancelArgs = new CancelEventArgs();

            currentVM.OnBeforeNextView(cancelArgs);

            if (cancelArgs.Cancel)
            {
                return;
            }

            LaunchAction launchAction = _bootstrapper.LaunchAction;

            DlgBaseVM nextVM = null;

            switch (launchAction)
            {
                case LaunchAction.Install:
                    if (currentVM is UpgradeDlgVM)
                    {
                        StartExecute();
                    }
                    else if (currentVM is WelcomeDlgVM)
                    {
                        nextVM = _viewModels[typeof(LicenseDlgVM)];
                    }
                    else if (currentVM is LicenseDlgVM)
                    {
                        nextVM = _viewModels[typeof(CustomizeDlgVM)];
                    }
                    else if (currentVM is CustomizeDlgVM)
                    {
                        nextVM = _viewModels[typeof(ReadyDlgVM)];
                    }
                    else if (currentVM is ReadyDlgVM)
                    {
                        StartExecute();
                    }
                    break;

                case LaunchAction.Uninstall:
                    if (currentVM is UninstallDlgVM)
                    {
                        StartExecute();
                    }
                    break;
            }

            if (currentVM is BitErrorDlgVM)
            {
                FireWiazrdFinished();
            }

            if (currentVM is DowngradeDlgVM)
            {
                FireWiazrdFinished();
            }

            if (currentVM is UserCancelDlgVM)
            {
                FireWiazrdFinished();
            }

            if (currentVM is FinishedDlgVM)
            {
                FireWiazrdFinished();
            }

            if (currentVM is ErrorDlgVM)
            {
                FireWiazrdFinished();
            }

            if (nextVM != null)
            {
                CurrentViewModel = nextVM;
            }
        }

        private void StartExecute()
        {
            CurrentViewModel = _progressDlgVM;
            _bootstrapper.InvokePlan();
        }

        public bool CanExitWizard()
        {
            if (_cancelConfirmed)
            {
                return true;
            }

            DlgBaseVM currentVM = CurrentViewModel;

            if (_currentViewModel is ProgressDlgVM)
            {
                RequestCancelProgress();
                return false;
            }
            else if (currentVM is BitErrorDlgVM)
            {
                return true;
            }
            else if (currentVM is DowngradeDlgVM)
            {
                return true;
            }
            else if (currentVM is FinishedDlgVM)
            {
                return true;
            }
            else if (currentVM is UserCancelDlgVM)
            {
                return true;
            }
            else if (currentVM is ErrorDlgVM)
            {
                return true;
            }
            else
            {
                if (_bootstrapper.ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    _bootstrapper.SetCancelledByUser();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void CancelWizard()
        {
            DlgBaseVM currentVM = CurrentViewModel;

            if (_currentViewModel is ProgressDlgVM)
            {
                RequestCancelProgress();
            }
            else if (currentVM is BitErrorDlgVM)
            {
                FireWiazrdFinished();
            }
            else if (currentVM is DowngradeDlgVM)
            {
                FireWiazrdFinished();
            }
            else if (currentVM is FinishedDlgVM)
            {
                FireWiazrdFinished();
            }
            else if (currentVM is UserCancelDlgVM)
            {
                FireWiazrdFinished();
            }
            else if (currentVM is ErrorDlgVM)
            {
                FireWiazrdFinished();
            }
            else
            {
                if (_bootstrapper.ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    _bootstrapper.SetCancelledByUser();
                    FireWiazrdFinished();
                }
            }
        }

        private void RequestCancelProgress()
        {
            _bootstrapper.RequestCancelProgress();
            FirePropertyChanged("IsCancelButtonEnabled");
        }

        public void SetProgressValue(int value)
        {
            _progressDlgVM.ProgressValue = value;
        }

        public void SetProgressStatus(string status)
        {
            _progressDlgVM.StatusText = status;
        }

        public void ShowErrorDialog()
        {
            CurrentViewModel = _errorDlgVM;
        }

        public void ShowUserCancelDialog()
        {
            CurrentViewModel = _userCancelDlgVM;
        }

        public void ShowFinishedDialog()
        {
            CurrentViewModel = _finishedDlgVM;
        }

        #endregion Methods

        #region EventHandler

        private void Bootstrapper_CancelProgressRequestedChanged(object sender, EventArgs e)
        {
            FirePropertyChanged("IsCancelButtonEnabled");
        }

        #endregion EventHandler

        #region Events

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void FirePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        #endregion PropertyChanged

        #region WiazrdFinished

        public event EventHandler WiazrdFinished;

        protected virtual void OnWiazrdFinished()
        {
            _cancelConfirmed = true;

            WiazrdFinished?.Invoke(this, EventArgs.Empty);
        }

        protected void FireWiazrdFinished()
        {
            OnWiazrdFinished();
        }

        #endregion WiazrdFinished

        #endregion Events
    }
}