using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Microsoft.Win32;
using System;
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
    internal class SetupApplication : BootstrapperApplication, IDisposable
    {
        #region Constants

        private const string BA_XML = "BootstrapperApplicationData.xml";

        public const string TL_PACKAGE_ID = "TL";

        #endregion Constants

        #region Fields

        private readonly AutoResetEvent _detectCompleteHandle;
        private readonly AutoResetEvent _applyCompleteHandle;

        private WizardWindow _wizardWindow;
        private WizardWindowVM _wizardWindowVM;
        private Dispatcher _uiThreadDispatcher;
        private IGuiService _guiService;
        private IUacService _uacService;

        private volatile bool _cancelProgressRequested;

        private LaunchAction _launchAction;
        private ActionResult _exitAction;

        private Version _relatedBundleVersion;
        private Version _productVersionPadded;
        private Version _productVersionTrimmed;

        private string _manufacturer;
        private string _productName;
        private string _featureTLSize;

        private bool _filesInUseActive;
        private bool _cancelledByUser;
        private bool _licenseAccepted;
        private bool _disposedValue;

        #region Passed to MSI

        private const string installDirRegValueName = "InstallDir";
        private string installDir;
        private string installDirPersisted;

        private bool deleteUserData;

        #endregion Passed to MSI

        #endregion Fields

        #region Constructors

        public SetupApplication()
        {
            _detectCompleteHandle = new AutoResetEvent(false);
            _applyCompleteHandle = new AutoResetEvent(false);
        }

        #endregion Constructors

        #region Properties

        public LaunchAction LaunchAction
        {
            get
            {
                return _launchAction;
            }
        }

        public ActionResult ExitAction
        {
            get
            {
                if (_cancelledByUser)
                {
                    return ActionResult.UserExit;
                }

                return _exitAction;
            }
        }

        public Version RelatedBundleVersion
        {
            get
            {
                return _relatedBundleVersion;
            }
        }

        public Version ProductVersionPadded
        {
            get
            {
                return _productVersionPadded;
            }
        }

        public Version ProductVersionTrimmed
        {
            get
            {
                return _productVersionTrimmed;
            }
        }

        public string Manufacturer
        {
            get
            {
                return _manufacturer;
            }
        }

        public string ProductName
        {
            get
            {
                return _productName;
            }
        }

        public string FeatureTLSize
        {
            get
            {
                return _featureTLSize;
            }
        }

        public bool IsInstalled
        {
            get
            {
                return Command.Resume == ResumeType.Arp;
            }
        }

        public bool IsUpgrade
        {
            get
            {
                return _relatedBundleVersion != null && _relatedBundleVersion < _productVersionTrimmed;
            }
        }

        public bool HasRelatedBundle
        {
            get
            {
                return _relatedBundleVersion != null;
            }
        }

        public bool IsQuietUninstall
        {
            get
            {
                return Command.Action == LaunchAction.Uninstall && (Command.Display == Display.None || Command.Display == Display.Embedded);
            }
        }

        public bool IsFullUiMode
        {
            get
            {
                return Command.Display == Display.Full;
            }
        }

        public bool CancelledByUser
        {
            get
            {
                return _cancelledByUser;
            }
        }

        public bool CancelProgressRequested
        {
            get
            {
                return _cancelProgressRequested;
            }
            protected set
            {
                _cancelProgressRequested = value;
                FireCancelProgressRequestedChanged();
            }
        }

        public bool LicenseAccepted
        {
            get
            {
                return _licenseAccepted;
            }
            set
            {
                _licenseAccepted = value;
            }
        }

        #region Passed To MSI

        public string InstallDir
        {
            get
            {
                return installDir;
            }
            set
            {
                installDir = value;
            }
        }

        public string InstallDirPersisted
        {
            get
            {
                return installDirPersisted;
            }
        }

        public bool DeleteUserData
        {
            get
            {
                return deleteUserData;
            }
            set
            {
                deleteUserData = value;
            }
        }

        #endregion Passed To MSI

        #endregion Properties

        #region Methods

        protected override void Run()
        {
            try
            {
                _launchAction = Command.Action;

                if (_launchAction != LaunchAction.Install &&
                    _launchAction != LaunchAction.Uninstall)
                {
                    Log("This installer can only run in install or uninstall mode!");
                    Engine.Quit((int)ActionResult.NotExecuted);
                    return;
                }

                if (!IsFullUiMode && !IsQuietUninstall)
                {
                    Log("This installer can only run in full UI mode!");
                    Engine.Quit((int)ActionResult.NotExecuted);
                    return;
                }

                Initialize();

                InvokeDetect();

                if (IsQuietUninstall)
                {
                    Log("Installer is running in quiet uninstall mode");

                    Log("Waiting for detection to complete");
                    _detectCompleteHandle.WaitOne();

                    InvokePlan();

                    Log("Waiting for execution to complete");
                    _applyCompleteHandle.WaitOne();
                }
                else
                {
                    Log("Installer is running in full UI mode");

                    Log("Waiting for detection to complete");
                    _detectCompleteHandle.WaitOne();
                    Log("Detection complete, ready to start UI");

                    _uiThreadDispatcher = Dispatcher.CurrentDispatcher;

                    Log("Setting SynchronizationContext");
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(_uiThreadDispatcher));

                    _guiService = new GuiService(this, _uiThreadDispatcher);
                    _uacService = new UacService();

                    Log("Initializing view model for main application window");
                    _wizardWindowVM = new WizardWindowVM(this, _guiService, _uacService);

                    Log("Initializing main application window");
                    _wizardWindow = new WizardWindow() { DataContext = _wizardWindowVM };

                    _wizardWindow.Closed += (a, b) => _uiThreadDispatcher.InvokeShutdown();

                    Log("Showing main application window");
                    _wizardWindow.Show();

                    Log("Running Dispatcher");
                    Dispatcher.Run();
                }

                Engine.Quit((int)ExitAction);
            }
            catch (Exception ex)
            {
                Log("An error occured while executing the Bootstrapper Application!" + Environment.NewLine + ex.ToString());
                Engine.Quit((int)ActionResult.Failure);
            }
        }

        private void Initialize()
        {
            Log("Starting Initialization of Custom Bootstrapper Application");

            ResolveSource += SetupApplication_ResolveSource;

            InitializeBundleProperties();
            InitializeMsiProperties();
            InitializeFeatureSizes();
        }

        private void InitializeBundleProperties()
        {
            string logName = "Action InitializeBundleProperties: ";

            Log(logName + "Retrieving manufacturer");
            _manufacturer = Engine.StringVariables["BUNDLE_MANUFACTURER"];
            Log(logName + "Manufacturer is '" + _manufacturer + "'");

            Log(logName + "Retrieving product name");
            _productName = Engine.StringVariables["BUNDLE_PRODUCT_NAME"];
            Log(logName + "Product name is '" + _productName + "'");

            Log(logName + "Retrieving product version in padded format");
            _productVersionPadded = Version.Parse(Engine.StringVariables["BUNDLE_PRODUCT_VERSION_PADDED"]);
            Log(logName + "Product version in padded format is '" + _productVersionPadded.ToString() + "'");

            Log(logName + "Retrieving product version in trimmed format");
            _productVersionTrimmed = Version.Parse(Engine.StringVariables["BUNDLE_PRODUCT_VERSION_TRIMMED"]);
            Log(logName + "Product version in trimmed format is '" + _productVersionTrimmed.ToString() + "'");
        }

        private void InitializeMsiProperties()
        {
            string logName = "Action InitializeMsiProperties: ";

            Log(logName + "Retrieving registry values of previous installation");

            using (RegistryKey localMachineKey = RegistryUtil.GetRegistryHiveOnBit(RegistryHive.LocalMachine))
            {
                string baseKeyStr = @"SOFTWARE\" + ProductName;

                Log(logName + @"Opening registry key 'HKEY_LOCAL_MACHINE\" + baseKeyStr);
                using (RegistryKey baseKey = localMachineKey.OpenSubKey(baseKeyStr))
                {
                    if (baseKey != null)
                    {
                        Log(logName + "Key exists");

                        installDirPersisted = GetRegistryValue(baseKey, installDirRegValueName);
                    }
                    else
                    {
                        Log(logName + "Key does not exist");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(installDirPersisted))
            {
                Log(logName + "Setting install directory to value of previous installation ('" + installDirPersisted + "')");
                installDir = installDirPersisted;
            }
            else
            {
                Log(logName + "Setting default install directory");
                installDir = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "Program Files", ProductName);
                Log(logName + "Default install directory is '" + installDir + "'");
            }
        }

        private void InitializeFeatureSizes()
        {
            string logName = "Action InitializeFeatureSizes: ";

            Log(logName + "Retrieving feature sizes from '" + BA_XML + "'");
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), BA_XML);
            Log(logName + "Path of '" + BA_XML + "' is '" + path + "'");

            Log(logName + "Opening '" + path + "'");
            XDocument baDoc = XDocument.Load(path);

            XNamespace ns = "http://schemas.microsoft.com/wix/2010/BootstrapperApplicationData";

            Log(logName + "Retrieving size for package '" + TL_PACKAGE_ID + "'");
            string fsSizeStr = baDoc.Descendants(ns + "WixPackageProperties")
                .Where(e => e.Attribute("Package").Value == TL_PACKAGE_ID)
                .First().Attribute("InstalledSize").Value;
            Log(logName + "Size for package '" + TL_PACKAGE_ID + "' is " + fsSizeStr + " bytes");

            Log(logName + "Converting size to MB value");
            _featureTLSize = Math.Round(double.Parse(fsSizeStr) / 1000 / 1000, 2).ToString(CultureInfo.GetCultureInfo("en-US"));
            Log(logName + "Size of package '" + TL_PACKAGE_ID + "' in MB is '" + _featureTLSize + "'");
        }

        private string GetRegistryValue(RegistryKey key, string regValueName)
        {
            string logName = "Action GetRegistryValue: ";

            string regValue = null;

            if (key == null)
            {
                Log(logName + "Registry key is null!");
            }

            if (string.IsNullOrWhiteSpace(regValueName))
            {
                Log(logName + "Argument 'regValueName' is null or empty!");
            }

            try
            {
                Log(logName + "Trying to get value '" + regValueName + "' from registry key");
                regValue = key.GetValue(regValueName).ToString();
                Log(logName + "Value '" + regValueName + "' from registry key is '" + regValue + "'");
            }
            catch
            {
                Log(logName + "Error while retrieving value '" + regValue + "' from registry key '" + key.Name + "'!");
            }

            return regValue;
        }

        private void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Engine.Log(LogLevel.Standard, message);
        }

        private void InvokeDetect()
        {
            DetectRelatedBundle += SetupApplication_DetectRelatedBundle;
            DetectComplete += SetupApplication_DetectComplete;
            Engine.Detect();
        }

        public void InvokePlan()
        {
            PlanBegin += SetupApplication_PlanBegin;
            PlanComplete += SetupApplication_PlanComplete;
            Engine.Plan(_launchAction);
        }

        public void InvokeApply()
        {
            IntPtr wizardWindowHwnd = IntPtr.Zero;

            if (!IsQuietUninstall)
            {
                wizardWindowHwnd = InvokeOnUiThread(() => { return new WindowInteropHelper(_wizardWindow).Handle; });
            }

            Error += SetupApplication_Error;
            ExecuteFilesInUse += SetupApplication_ExecuteFilesInUse;

            if (!IsQuietUninstall)
            {
                ExecuteProgress += SetupApplication_ExecuteProgress;
                ExecuteMsiMessage += SetupApplication_ExecuteMsiMessage;
            }

            ApplyComplete += SetupApplication_ApplyComplete;
            Engine.Apply(wizardWindowHwnd);
        }

        public TResult InvokeOnUiThread<TResult>(Func<TResult> func)
        {
            if (!_uiThreadDispatcher.CheckAccess())
            {
                return _uiThreadDispatcher.Invoke<TResult>(func);
            }
            else
            {
                return func();
            }
        }

        public void SetCancelledByUser()
        {
            _cancelledByUser = true;
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
                // Format 1
                if (int.TryParse(files[0], out _))
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
                else if (int.TryParse(files[1], out _))
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
            return _guiService.ShowMessageBox("Cancel Installation?", "Cancel", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
        }

        public void RequestCancelProgress()
        {
            CancelProgressRequested = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _detectCompleteHandle.Dispose();
                    _applyCompleteHandle.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion Methods

        #region EventHandlers

        #region Burn Engine

        #region Detection

        private void SetupApplication_DetectRelatedBundle(object sender, DetectRelatedBundleEventArgs e)
        {
            string logName = "Engine hook SetupApplication_DetectRelatedBundle: ";

            Version relatedVersion = e.Version.Trim();
            Log(logName + "Found a related bundle with version '" + relatedVersion + "'");
            _relatedBundleVersion = relatedVersion;
        }

        private void SetupApplication_DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            string logName = "Engine hook SetupApplication_DetectComplete: ";

            DetectRelatedBundle -= SetupApplication_DetectRelatedBundle;
            DetectComplete -= SetupApplication_DetectComplete;

            if (IsInstalled && !HasRelatedBundle)
            {
                Log(logName + "Current bundle is already installed and no related bundle was found. Setting launch action to '" + LaunchAction.Uninstall.ToString() + "'");
                _launchAction = LaunchAction.Uninstall;
            }

            Log(logName + "Set DetectComplete WaitHandle");
            _detectCompleteHandle.Set();
        }

        #endregion Detection

        #region Plan

        private void SetupApplication_PlanBegin(object sender, PlanBeginEventArgs e)
        {
            Engine.StringVariables["TL_INSTALLDIR_REGVALUENAME"] = installDirRegValueName;
            Engine.StringVariables["TL_INSTALLDIR"] = InstallDir;
            Engine.StringVariables["TL_INSTALLDIR_PERSISTED"] = InstallDirPersisted;

            Engine.StringVariables["TL_DELETE_USER_DATA"] = DeleteUserData ? "1" : "0";
        }

        private void SetupApplication_PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            PlanBegin -= SetupApplication_PlanBegin;
            PlanComplete -= SetupApplication_PlanComplete;

            InvokeApply();
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

                Log(sb.ToString());
            }
            catch (Exception ex)
            {
                Log(logName + "Exception occured!" + Environment.NewLine + ex.ToString());
            }
        }

        private void SetupApplication_ExecuteFilesInUse(object sender, ExecuteFilesInUseEventArgs e)
        {
            string logName = "Engine hook SetupApplication_ExecuteFilesInUse: ";

            try
            {
                _filesInUseActive = !_filesInUseActive;

                // It seems that MSI has some weird behavior. Only every 2nd call does really have
                // valuable information. The calls in between can even contain processes that do actually
                // NOT use any of the files to be changed by the installer. So you get false information.
                // In order to prevent this, we're scipping every odd call (1st, 3rd, 5th, ...) and calling
                // "Retry" again.
                if (_filesInUseActive)
                {
                    Log(logName + "Scipping unnecessary MSI callback 'ExecuteFilesInUse'");
                    e.Result = Result.Retry;
                    return;
                }

                IList<string> files = e.Files;

                if (files == null || files.Count == 0)
                {
                    e.Result = Result.Retry;
                    return;
                }

                IList<string> filesFormated = FormatFileInUseList(files);

                bool? showFilesInUseWindow()
                {
                    FilesInUseWindow filesInUseWindow = new FilesInUseWindow() { DataContext = new FilesInUseWindowVM(filesFormated) };

                    return filesInUseWindow.ShowDialog();
                }

                bool? dlgRes = InvokeOnUiThread(showFilesInUseWindow);

                if (dlgRes == true)
                {
                    e.Result = Result.Retry;
                }
                else
                {
                    SetCancelledByUser();
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

            if (_cancelProgressRequested)
            {
                Log(logName + "Cancellation of running progress was requested!");

                if (ShowCloseMessageMox() == MessageBoxResult.OK)
                {
                    SetCancelledByUser();
                    e.Result = Result.Cancel;
                }

                CancelProgressRequested = false;
            }

            _wizardWindowVM.SetProgressValue(e.OverallPercentage);
        }

        private void SetupApplication_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            if (e.MessageType == Microsoft.Tools.WindowsInstallerXml.Bootstrapper.InstallMessage.ActionStart
                    && !string.IsNullOrWhiteSpace(e.Message)
                    && e.Data.Count > 1)
            {
                _wizardWindowVM.SetProgressStatus(e.Data[1]);
            }
        }

        private void SetupApplication_ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            string logName = "Engine hook SetupApplication_ApplyComplete: ";

            Error -= SetupApplication_Error;
            ExecuteFilesInUse -= SetupApplication_ExecuteFilesInUse;
            ExecuteProgress -= SetupApplication_ExecuteProgress;
            ExecuteMsiMessage -= SetupApplication_ExecuteMsiMessage;
            ApplyComplete -= SetupApplication_ApplyComplete;

            if (e.Status >= 0)
            {
                if (!IsQuietUninstall)
                {
                    _wizardWindowVM.ShowFinishedDialog();
                }

                Log(logName + "Execution phase ended successfully. Setting exit action to '" + ActionResult.Success.ToString() + "'");
                _exitAction = ActionResult.Success;
            }
            else
            {
                if (!IsQuietUninstall)
                {
                    if (CancelledByUser)
                    {
                        _wizardWindowVM.ShowUserCancelDialog();
                    }
                    else
                    {
                        _wizardWindowVM.ShowErrorDialog();
                    }
                }

                Log(logName + "Execution phase failed. Setting exit action to '" + ActionResult.Failure.ToString() + "'");
                _exitAction = ActionResult.Failure;
            }

            Log(logName + "Set ApplyComplete WaitHandle");
            _applyCompleteHandle.Set();
        }

        #endregion Apply

        #region ResolveSource

        private void SetupApplication_ResolveSource(object sender, ResolveSourceEventArgs e)
        {
            if (!File.Exists(e.LocalSource) && !string.IsNullOrEmpty(e.DownloadSource))
            {
                e.Result = Result.Download;
            }
        }

        #endregion ResolveSource

        #endregion Burn Engine

        #endregion EventHandlers

        #region Events

        public event EventHandler CancelProgressRequestedChanged;

        protected virtual void OnCancelProgressRequestedChanged(EventArgs e)
        {
            CancelProgressRequestedChanged?.Invoke(this, e);
        }

        private void FireCancelProgressRequestedChanged()
        {
            OnCancelProgressRequestedChanged(EventArgs.Empty);
        }

        #endregion Events
    }
}