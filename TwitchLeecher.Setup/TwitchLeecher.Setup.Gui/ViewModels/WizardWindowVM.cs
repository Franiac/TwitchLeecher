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

        private SetupApplication bootstrapper;

        private IGuiService guiService;
        private IUacService uacService;

        private Dictionary<Type, DlgBaseVM> viewModels;

        private DlgBaseVM currentViewModel;

        private ProgressDlgVM progressDlgVM;
        private UserCancelDlgVM userCancelDlgVM;
        private ErrorDlgVM errorDlgVM;
        private FinishedDlgVM finishedDlgVM;

        private ICommand backCommand;
        private ICommand nextCommand;
        private ICommand cancelCommand;
        private ICommand exitCommand;

        private volatile bool cancelConfirmed;

        #endregion Fields

        public WizardWindowVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
        {
            if (bootstrapper == null)
            {
                throw new ArgumentNullException("bootstrapper");
            }

            if (guiService == null)
            {
                throw new ArgumentNullException("guiService");
            }

            if (uacService == null)
            {
                throw new ArgumentNullException("uacService");
            }

            this.bootstrapper = bootstrapper;
            this.guiService = guiService;
            this.uacService = uacService;

            this.bootstrapper.CancelProgressRequestedChanged += bootstrapper_CancelProgressRequestedChanged;

            this.CreateViewModels();
        }

        #region Properties

        public DlgBaseVM CurrentViewModel
        {
            get
            {
                return this.currentViewModel;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                this.currentViewModel = value;

                this.FirePropertyChanged(null);
            }
        }

        public string WizardTitle
        {
            get
            {
                return this.CurrentViewModel.WizardTitle;
            }
        }

        public string NextButtonText
        {
            get
            {
                return this.CurrentViewModel.NextButtonText;
            }
        }

        public bool IsNextButtonEnabled
        {
            get
            {
                return this.CurrentViewModel.IsNextButtonEnabled;
            }
        }

        public bool IsBackButtonEnabled
        {
            get
            {
                return this.CurrentViewModel.IsBackButtonEnabled;
            }
        }

        public bool IsCancelButtonEnabled
        {
            get
            {
                return this.CurrentViewModel.IsCancelButtonEnabled && !this.bootstrapper.CancelProgressRequested;
            }
        }

        public bool IsUacIconVisible
        {
            get
            {
                return this.CurrentViewModel.IsUacIconVisible;
            }
        }

        public BitmapImage UacIcon
        {
            get
            {
                return this.uacService.UacIcon;
            }
        }

        public ICommand BackCommand
        {
            get
            {
                if (this.backCommand == null)
                {
                    this.backCommand = new DelegateCommand(() =>
                    {
                        this.guiService.SetBusy();
                        this.ShowPreviousView();
                    });
                }

                return this.backCommand;
            }
        }

        public ICommand NextCommand
        {
            get
            {
                if (this.nextCommand == null)
                {
                    this.nextCommand = new DelegateCommand(() =>
                    {
                        this.guiService.SetBusy();
                        this.ShowNextView();
                    });
                }

                return this.nextCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(this.CancelWizard);
                }

                return this.cancelCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (this.exitCommand == null)
                {
                    this.exitCommand = new DelegateCommand(() => { }, this.CanExitWizard);
                }

                return this.exitCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void CreateViewModels()
        {
            this.viewModels = new Dictionary<Type, DlgBaseVM>();

            this.progressDlgVM = new ProgressDlgVM(this.bootstrapper, this.guiService);
            this.userCancelDlgVM = new UserCancelDlgVM(this.bootstrapper, this.guiService);
            this.errorDlgVM = new ErrorDlgVM(this.bootstrapper, this.guiService);
            this.finishedDlgVM = new FinishedDlgVM(this.bootstrapper, this.guiService);

            if (!this.bootstrapper.OSAndBundleBitMatch)
            {
                BitErrorDlgVM bitErrorDlgVM = new BitErrorDlgVM(this.bootstrapper, this.guiService);
                this.viewModels.Add(typeof(BitErrorDlgVM), bitErrorDlgVM);
                this.CurrentViewModel = bitErrorDlgVM;
            }
            else if (this.bootstrapper.IsUpgrade)
            {
                UpgradeDlgVM upgradeDlgVM = new UpgradeDlgVM(this.bootstrapper, this.guiService, this.uacService);
                this.viewModels.Add(typeof(UpgradeDlgVM), upgradeDlgVM);
                this.CurrentViewModel = upgradeDlgVM;
            }
            else if (this.bootstrapper.HasRelatedBundle)
            {
                DowngradeDlgVM downgradeDlgVM = new DowngradeDlgVM(this.bootstrapper, this.guiService);
                this.viewModels.Add(typeof(DowngradeDlgVM), downgradeDlgVM);
                this.CurrentViewModel = downgradeDlgVM;
            }
            else
            {
                LaunchAction launchAction = this.bootstrapper.LaunchAction;

                switch (launchAction)
                {
                    case LaunchAction.Install:
                        this.viewModels.Add(typeof(WelcomeDlgVM), new WelcomeDlgVM(this.bootstrapper, this.guiService));
                        this.viewModels.Add(typeof(LicenseDlgVM), new LicenseDlgVM(this.bootstrapper, this.guiService));
                        this.viewModels.Add(typeof(CustomizeDlgVM), new CustomizeDlgVM(this.bootstrapper, this.guiService));
                        this.viewModels.Add(typeof(ReadyDlgVM), new ReadyDlgVM(this.bootstrapper, this.guiService, this.uacService));
                        this.CurrentViewModel = this.viewModels[typeof(WelcomeDlgVM)];
                        break;

                    case LaunchAction.Uninstall:
                        UninstallDlgVM uninstallDlgVM = new UninstallDlgVM(this.bootstrapper, this.guiService, this.uacService);
                        this.viewModels.Add(typeof(UninstallDlgVM), uninstallDlgVM);
                        this.CurrentViewModel = uninstallDlgVM;
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + this.bootstrapper.LaunchAction.ToString() + "'!");
                }
            }
        }

        private void ShowPreviousView()
        {
            LaunchAction launchAction = this.bootstrapper.LaunchAction;

            DlgBaseVM currentVM = this.CurrentViewModel;
            DlgBaseVM previousVM = null;

            if (launchAction == LaunchAction.Install)
            {
                if (currentVM is LicenseDlgVM)
                {
                    previousVM = this.viewModels[typeof(WelcomeDlgVM)];
                }
                else if (currentVM is CustomizeDlgVM)
                {
                    previousVM = this.viewModels[typeof(LicenseDlgVM)];
                }
                else if (currentVM is ReadyDlgVM)
                {
                    previousVM = this.viewModels[typeof(CustomizeDlgVM)];
                }
            }

            if (previousVM != null)
            {
                this.CurrentViewModel = previousVM;
            }
        }

        private void ShowNextView()
        {
            DlgBaseVM currentVM = this.CurrentViewModel;

            CancelEventArgs cancelArgs = new CancelEventArgs();

            currentVM.OnBeforeNextView(cancelArgs);

            if (cancelArgs.Cancel)
            {
                return;
            }

            LaunchAction launchAction = this.bootstrapper.LaunchAction;

            DlgBaseVM nextVM = null;

            switch (launchAction)
            {
                case LaunchAction.Install:
                    if (currentVM is UpgradeDlgVM)
                    {
                        this.StartExecute();
                    }
                    else if (currentVM is WelcomeDlgVM)
                    {
                        nextVM = this.viewModels[typeof(LicenseDlgVM)];
                    }
                    else if (currentVM is LicenseDlgVM)
                    {
                        nextVM = this.viewModels[typeof(CustomizeDlgVM)];
                    }
                    else if (currentVM is CustomizeDlgVM)
                    {
                        nextVM = this.viewModels[typeof(ReadyDlgVM)];
                    }
                    else if (currentVM is ReadyDlgVM)
                    {
                        this.StartExecute();
                    }
                    break;

                case LaunchAction.Uninstall:
                    if (currentVM is UninstallDlgVM)
                    {
                        this.StartExecute();
                    }
                    break;
            }

            if (currentVM is BitErrorDlgVM)
            {
                this.FireWiazrdFinished();
            }

            if (currentVM is DowngradeDlgVM)
            {
                this.FireWiazrdFinished();
            }

            if (currentVM is UserCancelDlgVM)
            {
                this.FireWiazrdFinished();
            }

            if (currentVM is FinishedDlgVM)
            {
                this.FireWiazrdFinished();
            }

            if (currentVM is ErrorDlgVM)
            {
                this.FireWiazrdFinished();
            }

            if (nextVM != null)
            {
                this.CurrentViewModel = nextVM;
            }
        }

        private void StartExecute()
        {
            this.CurrentViewModel = this.progressDlgVM;
            this.bootstrapper.InvokePlan();
        }

        public bool CanExitWizard()
        {
            if (this.cancelConfirmed)
            {
                return true;
            }

            DlgBaseVM currentVM = this.CurrentViewModel;

            if (currentViewModel is ProgressDlgVM)
            {
                this.RequestCancelProgress();
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
                if (this.bootstrapper.ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    this.bootstrapper.SetCancelledByUser();
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
            DlgBaseVM currentVM = this.CurrentViewModel;

            if (currentViewModel is ProgressDlgVM)
            {
                this.RequestCancelProgress();
            }
            else if (currentVM is BitErrorDlgVM)
            {
                this.FireWiazrdFinished();
            }
            else if (currentVM is DowngradeDlgVM)
            {
                this.FireWiazrdFinished();
            }
            else if (currentVM is FinishedDlgVM)
            {
                this.FireWiazrdFinished();
            }
            else if (currentVM is UserCancelDlgVM)
            {
                this.FireWiazrdFinished();
            }
            else if (currentVM is ErrorDlgVM)
            {
                this.FireWiazrdFinished();
            }
            else
            {
                if (this.bootstrapper.ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    this.bootstrapper.SetCancelledByUser();
                    this.FireWiazrdFinished();
                }
            }
        }

        private void RequestCancelProgress()
        {
            this.bootstrapper.RequestCancelProgress();
            this.FirePropertyChanged("IsCancelButtonEnabled");
        }

        public void SetProgressValue(int value)
        {
            this.progressDlgVM.ProgressValue = value;
        }

        public void SetProgressStatus(string status)
        {
            this.progressDlgVM.StatusText = status;
        }

        public void ShowErrorDialog()
        {
            this.CurrentViewModel = this.errorDlgVM;
        }

        public void ShowUserCancelDialog()
        {
            this.CurrentViewModel = this.userCancelDlgVM;
        }

        public void ShowFinishedDialog()
        {
            this.CurrentViewModel = this.finishedDlgVM;
        }

        #endregion Methods

        #region EventHandler

        private void bootstrapper_CancelProgressRequestedChanged(object sender, EventArgs e)
        {
            this.FirePropertyChanged("IsCancelButtonEnabled");
        }

        #endregion EventHandler

        #region Events

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void FirePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }

        #endregion PropertyChanged

        #region WiazrdFinished

        public event EventHandler WiazrdFinished;

        protected virtual void OnWiazrdFinished()
        {
            this.cancelConfirmed = true;

            if (this.WiazrdFinished != null)
            {
                this.WiazrdFinished(this, EventArgs.Empty);
            }
        }

        protected void FireWiazrdFinished()
        {
            this.OnWiazrdFinished();
        }

        #endregion WiazrdFinished

        #endregion Events
    }
}