using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml.Linq;
using TwitchLeecher.Setup.Gui.Services;
using TwitchLeecher.Setup.Gui.ViewModels;
using TwitchLeecher.Setup.Gui.Views;

namespace TwitchLeecher.Setup.Gui
{
    internal class SetupApplication : BootstrapperApplication
    {
        #region Constants

        private const string BA_XML = "BootstrapperApplicationData.xml";

        public const string TL_PACKAGE_ID = "TL";

        #endregion Constants

        #region Fields

        private ConcurrentDictionary<string, BootstrapperProcess> runningProcesses = new ConcurrentDictionary<string, BootstrapperProcess>();

        private WizardWindow wizardWindow;
        private WizardWindowVM wizardWindowVM;
        private Dispatcher uiThreadDispatcher;
        private IGuiService guiService;
        private IUacService uacService;

        private AutoResetEvent detectCompleteHandle;
        private AutoResetEvent applyCompleteHandle;

        private volatile bool cancelProgressRequested;

        private LaunchAction launchAction;
        private ActionResult exitAction;

        private Version relatedBundleVersion;
        private Version productVersionPadded;
        private Version productVersionTrimmed;

        private string architecture;
        private string manufacturer;
        private string productName;
        private string featureTLSize;

        private bool filesInUseActive;
        private bool cancelledByUser;
        private bool licenseAccepted;

        #region Passed to MSI

        private const string installDirRegValueName = "InstallDir";
        private string installDir;
        private string installDirPersisted;

        private bool deleteUserData;

        #endregion Passed to MSI

        #endregion Fields

        #region Properties

        public LaunchAction LaunchAction
        {
            get
            {
                return this.launchAction;
            }
        }

        public ActionResult ExitAction
        {
            get
            {
                if (this.cancelledByUser)
                {
                    return ActionResult.UserExit;
                }

                return this.exitAction;
            }
        }

        public Version RelatedBundleVersion
        {
            get
            {
                return this.relatedBundleVersion;
            }
        }

        public Version ProductVersionPadded
        {
            get
            {
                return this.productVersionPadded;
            }
        }

        public Version ProductVersionTrimmed
        {
            get
            {
                return this.productVersionTrimmed;
            }
        }

        public bool IsBundle64Bit
        {
            get
            {
                return this.architecture == "x64";
            }
        }

        public bool OSAndBundleBitMatch
        {
            get
            {
                return (this.IsBundle64Bit && Environment.Is64BitOperatingSystem) || (!this.IsBundle64Bit && !Environment.Is64BitOperatingSystem);
            }
        }

        public string Manufacturer
        {
            get
            {
                return this.manufacturer;
            }
        }

        public string ProductName
        {
            get
            {
                return this.productName;
            }
        }

        public string FeatureTLSize
        {
            get
            {
                return this.featureTLSize;
            }
        }

        public bool IsInstalled
        {
            get
            {
                return this.Command.Resume == ResumeType.Arp;
            }
        }

        public bool IsUpgrade
        {
            get
            {
                return this.relatedBundleVersion != null && this.relatedBundleVersion < this.productVersionTrimmed;
            }
        }

        public bool HasRelatedBundle
        {
            get
            {
                return this.relatedBundleVersion != null;
            }
        }

        public bool IsQuietUninstall
        {
            get
            {
                return this.Command.Action == LaunchAction.Uninstall && (this.Command.Display == Display.None || this.Command.Display == Display.Embedded);
            }
        }

        public bool IsFullUiMode
        {
            get
            {
                return this.Command.Display == Display.Full;
            }
        }

        public bool CancelledByUser
        {
            get
            {
                return this.cancelledByUser;
            }
        }

        public bool CancelProgressRequested
        {
            get
            {
                return this.cancelProgressRequested;
            }
            protected set
            {
                this.cancelProgressRequested = value;
                this.FireCancelProgressRequestedChanged();
            }
        }

        public bool LicenseAccepted
        {
            get
            {
                return this.licenseAccepted;
            }
            set
            {
                this.licenseAccepted = value;
            }
        }

        #region Passed To MSI

        public string InstallDir
        {
            get
            {
                return this.installDir;
            }
            set
            {
                this.installDir = value;
            }
        }

        public string InstallDirPersisted
        {
            get
            {
                return this.installDirPersisted;
            }
        }

        public bool DeleteUserData
        {
            get
            {
                return this.deleteUserData;
            }
            set
            {
                this.deleteUserData = value;
            }
        }

        #endregion Passed To MSI

        #endregion Properties

        #region Methods

        protected override void Run()
        {
            try
            {
                this.launchAction = this.Command.Action;

                if (this.launchAction != LaunchAction.Install &&
                    this.launchAction != LaunchAction.Uninstall)
                {
                    this.Log("This installer can only run in install or uninstall mode!");
                    this.Engine.Quit((int)ActionResult.NotExecuted);
                    return;
                }

                if (!this.IsFullUiMode && !this.IsQuietUninstall)
                {
                    this.Log("This installer can only run in full UI mode!");
                    this.Engine.Quit((int)ActionResult.NotExecuted);
                    return;
                }

                this.Initialize();

                this.detectCompleteHandle = new AutoResetEvent(false);
                this.applyCompleteHandle = new AutoResetEvent(false);

                this.InvokeDetect();

                if (this.IsQuietUninstall)
                {
                    this.Log("Installer is running in quiet uninstall mode");

                    this.Log("Waiting for detection to complete");
                    detectCompleteHandle.WaitOne();

                    this.InvokePlan();

                    this.Log("Waiting for execution to complete");
                    applyCompleteHandle.WaitOne();
                }
                else
                {
                    this.Log("Installer is running in full UI mode");

                    this.Log("Waiting for detection to complete");
                    detectCompleteHandle.WaitOne();
                    this.Log("Detection complete, ready to start UI");

                    this.uiThreadDispatcher = Dispatcher.CurrentDispatcher;

                    this.Log("Setting SynchronizationContext");
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this.uiThreadDispatcher));

                    this.guiService = new GuiService(this, this.uiThreadDispatcher);
                    this.uacService = new UacService();

                    this.Log("Initializing view model for main application window");
                    this.wizardWindowVM = new WizardWindowVM(this, this.guiService, this.uacService);

                    this.Log("Initializing main application window");
                    this.wizardWindow = new WizardWindow() { DataContext = this.wizardWindowVM };

                    this.wizardWindow.Closed += (a, b) => this.uiThreadDispatcher.InvokeShutdown();

                    this.Log("Showing main application window");
                    this.wizardWindow.Show();

                    this.Log("Running Dispatcher");
                    Dispatcher.Run();
                }

                this.Engine.Quit((int)this.ExitAction);
            }
            catch (Exception ex)
            {
                this.Log("An error occured while executing the Bootstrapper Application!" + Environment.NewLine + ex.ToString());
                this.Engine.Quit((int)ActionResult.Failure);
            }
        }

        private void Initialize()
        {
            this.Log("Starting Initialization of Custom Bootstrapper Application");

            this.InitializeBundleProperties();
            this.InitializeMsiProperties();
            this.InitializeFeatureSizes();
        }

        private void InitializeBundleProperties()
        {
            string logName = "Action InitializeBundleProperties: ";

            this.Log(logName + "Retrieving architecture");
            this.architecture = this.Engine.StringVariables["BUNDLE_ARCHITECTURE"];
            this.Log(logName + "Architecture is '" + this.architecture + "'");

            this.Log(logName + "Retrieving manufacturer");
            this.manufacturer = this.Engine.StringVariables["BUNDLE_MANUFACTURER"];
            this.Log(logName + "Manufacturer is '" + this.manufacturer + "'");

            this.Log(logName + "Retrieving product name");
            this.productName = this.Engine.StringVariables["BUNDLE_PRODUCT_NAME"];
            this.Log(logName + "Product name is '" + this.productName + "'");

            this.Log(logName + "Retrieving product version in padded format");
            this.productVersionPadded = Version.Parse(this.Engine.StringVariables["BUNDLE_PRODUCT_VERSION_PADDED"]);
            this.Log(logName + "Product version in padded format is '" + this.productVersionPadded.ToString() + "'");

            this.Log(logName + "Retrieving product version in trimmed format");
            this.productVersionTrimmed = Version.Parse(this.Engine.StringVariables["BUNDLE_PRODUCT_VERSION_TRIMMED"]);
            this.Log(logName + "Product version in trimmed format is '" + this.productVersionTrimmed.ToString() + "'");
        }

        private void InitializeMsiProperties()
        {
            string logName = "Action InitializeMsiProperties: ";

            this.Log(logName + "Retrieving registry values of previous installation");

            using (RegistryKey localMachineKey = RegistryUtil.GetRegistryHiveOnBit(RegistryHive.LocalMachine))
            {
                string baseKeyStr = @"SOFTWARE\" + this.ProductName;

                this.Log(logName + @"Opening registry key 'HKEY_LOCAL_MACHINE\" + baseKeyStr);
                using (RegistryKey baseKey = localMachineKey.OpenSubKey(baseKeyStr))
                {
                    if (baseKey != null)
                    {
                        this.Log(logName + "Key exists");

                        this.installDirPersisted = this.GetRegistryValue(baseKey, installDirRegValueName);
                    }
                    else
                    {
                        this.Log(logName + "Key does not exist");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(this.installDirPersisted))
            {
                this.Log(logName + "Setting install directory to value of previous installation ('" + this.installDirPersisted + "')");
                this.installDir = this.installDirPersisted;
            }
            else
            {
                this.Log(logName + "Setting default install directory");
                this.installDir = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Program Files", this.ProductName);
                this.Log(logName + "Default install directory is '" + this.installDir + "'");
            }
        }

        private void InitializeFeatureSizes()
        {
            string logName = "Action InitializeFeatureSizes: ";

            this.Log(logName + "Retrieving feature sizes from '" + BA_XML + "'");
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BA_XML);
            this.Log(logName + "Path of '" + BA_XML + "' is '" + path + "'");

            this.Log(logName + "Opening '" + path + "'");
            XDocument baDoc = XDocument.Load(path);

            XNamespace ns = "http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";

            this.Log(logName + "Retrieving size for package '" + TL_PACKAGE_ID + "'");
            string fsSizeStr = baDoc.Descendants(ns + "WixPackageProperties")
                .Where(e => e.Attribute("Package").Value == TL_PACKAGE_ID)
                .First().Attribute("InstalledSize").Value;
            this.Log(logName + "Size for package '" + TL_PACKAGE_ID + "' is " + fsSizeStr + " bytes");

            this.Log(logName + "Converting size to MB value");
            this.featureTLSize = Math.Round(double.Parse(fsSizeStr) / 1000 / 1000, 2).ToString(CultureInfo.GetCultureInfo("en-US"));
            this.Log(logName + "Size of package '" + TL_PACKAGE_ID + "' in MB is '" + this.featureTLSize + "'");
        }

        private string GetRegistryValue(RegistryKey key, string regValueName)
        {
            string logName = "Action GetRegistryValue: ";

            string regValue = null;

            if (key == null)
            {
                this.Log(logName + "Registry key is null!");
            }

            if (string.IsNullOrWhiteSpace(regValueName))
            {
                this.Log(logName + "Argument 'regValueName' is null or empty!");
            }

            try
            {
                this.Log(logName + "Trying to get value '" + regValueName + "' from registry key");
                regValue = key.GetValue(regValueName).ToString();
                this.Log(logName + "Value '" + regValueName + "' from registry key is '" + regValue + "'");
            }
            catch
            {
                this.Log(logName + "Error while retrieving value '" + regValue + "' from registry key '" + key.Name + "'!");
            }

            return regValue;
        }

        private void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            this.Engine.Log(LogLevel.Standard, message);
        }

        private void InvokeDetect()
        {
            this.DetectRelatedBundle += SetupApplication_DetectRelatedBundle;
            this.DetectComplete += SetupApplication_DetectComplete;
            this.Engine.Detect();
        }

        public void InvokePlan()
        {
            this.PlanBegin += SetupApplication_PlanBegin;
            this.PlanComplete += SetupApplication_PlanComplete;
            this.Engine.Plan(this.launchAction);
        }

        public void InvokeApply()
        {
            IntPtr wizardWindowHwnd = IntPtr.Zero;

            if (!this.IsQuietUninstall)
            {
                wizardWindowHwnd = this.InvokeOnUiThread<IntPtr>(() => { return new WindowInteropHelper(this.wizardWindow).Handle; });
            }

            this.Error += SetupApplication_Error;
            this.ExecuteFilesInUse += SetupApplication_ExecuteFilesInUse;

            if (!this.IsQuietUninstall)
            {
                this.ExecuteProgress += SetupApplication_ExecuteProgress;
                this.ExecuteMsiMessage += SetupApplication_ExecuteMsiMessage;
            }

            this.ApplyComplete += SetupApplication_ApplyComplete;
            this.Engine.Apply(wizardWindowHwnd);
        }

        public TResult InvokeOnUiThread<TResult>(Func<TResult> func)
        {
            if (!this.uiThreadDispatcher.CheckAccess())
            {
                return this.uiThreadDispatcher.Invoke<TResult>(func);
            }
            else
            {
                return func();
            }
        }

        public void SetCancelledByUser()
        {
            this.cancelledByUser = true;
        }

        /// <summary>
        /// The MSI ExecuteFilesInUse callback returns the file list in at least three different formats!
        /// It's absolutely not predictable when MSI will use which format!
        ///
        /// Format 1:
        ///
        /// MSI returns a list by alternating process id and program name whilst padding empty entries to 2^x.
        ///
        /// [0] - 9876
        /// [1] - Twitch Leecher
        /// [2] - 6789
        /// [3] - dotPeek
        /// [4] -
        /// [5] -
        /// [6] -
        /// [7] -
        ///
        /// Format 2:
        ///
        /// MSI returns a list by alternating program name and process id without padding the list.
        ///
        /// [0] - Twitch Leecher
        /// [1] - 9876
        /// [2] - dotPeek
        /// [3] - 6789
        ///
        /// Format 3:
        ///
        /// MSI returns a list by alternating program name and program name with process id without padding the list.
        ///
        /// [0] - Twitch Leecher
        /// [1] - Twitch Leecher (Process Id: 9876)
        /// [2] - dotPeek
        /// [3] - dotPeek (Process Id: 6789)
        /// </summary>
        private IList<string> FormatFileInUseList(IList<string> files)
        {
            List<string> newList = new List<string>();

            if (files != null || files.Count > 0)
            {
                int tryInt;

                // Format 1
                if (int.TryParse(files[0], out tryInt))
                {
                    for (int i = 0; i < files.Count; i += 2)
                    {
                        if (string.IsNullOrWhiteSpace(files[i]))
                        {
                            break;
                        }

                        newList.Add(files[i + 1] + " (Process Id: " + files[i] + ")");
                    }
                }
                // Format 2
                else if (int.TryParse(files[1], out tryInt))
                {
                    for (int i = 0; i < files.Count; i += 2)
                    {
                        newList.Add(files[i] + " (Process Id: " + files[i + 1] + ")");
                    }
                }
                // Format 3
                else
                {
                    for (int i = 0; i < files.Count; i += 2)
                    {
                        newList.Add(files[i + 1]);
                    }
                }
            }

            return newList;
        }

        public MessageBoxResult ShowCloseMessageMox()
        {
            return this.guiService.ShowMessageBox("Cancel Installation?", "Cancel", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

        public void RequestCancelProgress()
        {
            this.CancelProgressRequested = true;
        }

        #endregion Methods

        #region EventHandlers

        #region Burn Engine

        #region Detection

        private void SetupApplication_DetectRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
        {
            string logName = "Engine hook SetupApplication_DetectRelatedBundle: ";

            Version relatedVersion = e.Version.Trim();
            this.Log(logName + "Found a related bundle with version '" + relatedVersion + "'");
            this.relatedBundleVersion = relatedVersion;
        }

        private void SetupApplication_DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            string logName = "Engine hook SetupApplication_DetectComplete: ";

            this.DetectRelatedBundle -= SetupApplication_DetectRelatedBundle;
            this.DetectComplete -= SetupApplication_DetectComplete;

            if (this.IsInstalled && !this.HasRelatedBundle)
            {
                this.Log(logName + "Current bundle is already installed and no related bundle was found. Setting launch action to '" + LaunchAction.Uninstall.ToString() + "'");
                this.launchAction = LaunchAction.Uninstall;
            }

            this.Log(logName + "Set DetectComplete WaitHandle");
            this.detectCompleteHandle.Set();
        }

        #endregion Detection

        #region Plan

        private void SetupApplication_PlanBegin(object sender, PlanBeginEventArgs e)
        {
            this.Engine.StringVariables["TL_INSTALLDIR_REGVALUENAME"] = installDirRegValueName;
            this.Engine.StringVariables["TL_INSTALLDIR"] = this.InstallDir;
            this.Engine.StringVariables["TL_INSTALLDIR_PERSISTED"] = this.InstallDirPersisted;

            this.Engine.StringVariables["TL_DELETE_USER_DATA"] = this.DeleteUserData ? "1" : "0";
        }

        private void SetupApplication_PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            this.PlanBegin -= SetupApplication_PlanBegin;
            this.PlanComplete -= SetupApplication_PlanComplete;

            this.InvokeApply();
        }

        #endregion Plan

        #region Apply

        private void SetupApplication_Error(object sender, Microsoft.Tools.WindowsInstallerXml.Bootstrapper.ErrorEventArgs e)
        {
            string logName = "Engine hook SetupApplication_Error: ";

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("ERROR DURING INSTALL EXECUTION!");
                sb.Append(" Message: ");
                sb.Append("\"" + e.ErrorMessage + "\"");
                sb.Append(", Type: ");
                sb.Append(e.ErrorType.ToString());
                sb.Append(", Error Code: ");
                sb.Append(e.ErrorCode.ToString());
                sb.Append(", Package ID: ");
                sb.Append(e.PackageId);

                IList<string> data = e.Data;

                if (data != null && data.Count > 0)
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        sb.Append(", Data [" + i.ToString() + "]: ");
                        sb.Append(data[i]);
                    }
                }

                this.Log(sb.ToString());
            }
            catch (Exception ex)
            {
                this.Log(logName + "Exception occured!" + Environment.NewLine + ex.ToString());
            }
        }

        private void SetupApplication_ExecuteFilesInUse(object sender, ExecuteFilesInUseEventArgs e)
        {
            string logName = "Engine hook SetupApplication_ExecuteFilesInUse: ";

            try
            {
                this.filesInUseActive = !this.filesInUseActive;

                // It seems that MSI has some weird behavior. Only every 2nd call does really have
                // valuable information. The calls in between can even contain processes that do actually
                // NOT use any of the files to be changed by the installer. So you get false information.
                // In order to prevent this, we're scipping every odd call (1st, 3rd, 5th, ...) and calling
                // "Retry" again.
                if (this.filesInUseActive)
                {
                    this.Log(logName + "Scipping unnecessary MSI callback 'ExecuteFilesInUse'");
                    e.Result = Result.Retry;
                    return;
                }

                IList<string> files = e.Files;

                if (files == null || files.Count == 0)
                {
                    e.Result = Result.Retry;
                    return;
                }

                IList<string> filesFormated = this.FormatFileInUseList(files);

                Func<bool?> showFilesInUseWindow = () =>
                {
                    FilesInUseWindow filesInUseWindow = new FilesInUseWindow() { DataContext = new FilesInUseWindowVM(filesFormated) };

                    return filesInUseWindow.ShowDialog();
                };

                bool? dlgRes = this.InvokeOnUiThread<bool?>(showFilesInUseWindow);

                if (dlgRes == true)
                {
                    e.Result = Result.Retry;
                }
                else
                {
                    this.SetCancelledByUser();
                    e.Result = Result.Cancel;
                }
            }
            catch
            {
                e.Result = Result.Cancel;
                throw;
            }
        }

        private void SetupApplication_ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            string logName = "Engine hook SetupApplication_ExecuteProgress: ";

            if (this.cancelProgressRequested)
            {
                this.Log(logName + "Cancellation of running progress was requested!");

                if (this.ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    this.SetCancelledByUser();
                    e.Result = Result.Cancel;
                }

                this.CancelProgressRequested = false;
            }

            this.wizardWindowVM.SetProgressValue(e.OverallPercentage);
        }

        private void SetupApplication_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            if (e.MessageType == Microsoft.Tools.WindowsInstallerXml.Bootstrapper.InstallMessage.ActionStart
                    && !string.IsNullOrWhiteSpace(e.Message)
                    && e.Data.Count > 1)
            {
                this.wizardWindowVM.SetProgressStatus(e.Data[1]);
            }
        }

        private void SetupApplication_ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            string logName = "Engine hook SetupApplication_ApplyComplete: ";

            this.Error -= SetupApplication_Error;
            this.ExecuteFilesInUse -= SetupApplication_ExecuteFilesInUse;
            this.ExecuteProgress -= SetupApplication_ExecuteProgress;
            this.ExecuteMsiMessage -= SetupApplication_ExecuteMsiMessage;
            this.ApplyComplete -= SetupApplication_ApplyComplete;

            if (e.Status >= 0)
            {
                if (!this.IsQuietUninstall)
                {
                    this.wizardWindowVM.ShowFinishedDialog();
                }

                this.Log(logName + "Execution phase ended successfully. Setting exit action to '" + ActionResult.Success.ToString() + "'");
                this.exitAction = ActionResult.Success;
            }
            else
            {
                if (!this.IsQuietUninstall)
                {
                    if (this.CancelledByUser)
                    {
                        this.wizardWindowVM.ShowUserCancelDialog();
                    }
                    else
                    {
                        this.wizardWindowVM.ShowErrorDialog();
                    }
                }

                this.Log(logName + "Execution phase failed. Setting exit action to '" + ActionResult.Failure.ToString() + "'");
                this.exitAction = ActionResult.Failure;
            }

            this.Log(logName + "Set ApplyComplete WaitHandle");
            this.applyCompleteHandle.Set();
        }

        #endregion Apply

        #endregion Burn Engine

        #endregion EventHandlers

        #region Events

        public event EventHandler CancelProgressRequestedChanged;

        protected virtual void OnCancelProgressRequestedChanged(EventArgs e)
        {
            if (this.CancelProgressRequestedChanged != null)
            {
                this.CancelProgressRequestedChanged(this, e);
            }
        }

        private void FireCancelProgressRequestedChanged()
        {
            this.OnCancelProgressRequestedChanged(EventArgs.Empty);
        }

        #endregion Events
    }
}