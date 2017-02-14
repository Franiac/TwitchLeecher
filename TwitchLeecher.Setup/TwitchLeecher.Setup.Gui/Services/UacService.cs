using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Media.Imaging;
using static TwitchLeecher.Setup.Gui.NativeMethods;

namespace TwitchLeecher.Setup.Gui.Services
{
    internal class UacService : IUacService
    {
        #region Fields

        private bool isUacEnabled;
        private bool isUserAdmin;

        private BitmapImage uacIcon;
        private object uacIconLock;

        #endregion Fields

        #region Constructors

        public UacService()
        {
            this.uacIconLock = new object();
            this.isUacEnabled = this.GetIsUacEnabled();
            this.isUserAdmin = this.GetIsUserAdmin();
        }

        #endregion Constructors

        #region Properties

        public bool IsUacEnabled
        {
            get
            {
                return this.isUacEnabled;
            }
        }

        public bool IsUserAdmin
        {
            get
            {
                return this.isUserAdmin;
            }
        }

        public BitmapImage UacIcon
        {
            get
            {
                if (this.uacIcon == null)
                {
                    this.CreateUacIcon();
                }

                return this.uacIcon;
            }
        }

        #endregion Properties

        #region Methods

        private bool GetIsUacEnabled()
        {
            using (RegistryKey localMachineKey = RegistryUtil.GetRegistryHiveOnBit(RegistryHive.LocalMachine))
            {
                using (RegistryKey systemKey = localMachineKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (systemKey != null)
                    {
                        try
                        {
                            int value = int.Parse(systemKey.GetValue("EnableLUA").ToString());

                            return value > 0;
                        }
                        catch
                        {
                            // Could not read value
                        }
                    }
                }
            }

            return true;
        }

        private bool GetIsUserAdmin()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void CreateUacIcon()
        {
            lock (this.uacIconLock)
            {
                if (this.uacIcon == null)
                {
                    SHSTOCKICONINFO iconResult = new SHSTOCKICONINFO();
                    iconResult.cbSize = (uint)Marshal.SizeOf(iconResult);

                    SHGetStockIconInfo(SHSTOCKICONID.SIID_SHIELD, SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON, ref iconResult);

                    Bitmap bmp = Bitmap.FromHicon(iconResult.hIcon);
                    bmp.MakeTransparent();

                    BitmapImage bmpImg = new BitmapImage();

                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Png);

                    bmpImg.BeginInit();
                    bmpImg.StreamSource = ms;
                    bmpImg.EndInit();

                    this.uacIcon = bmpImg;
                }
            }
        }

        #endregion Methods
    }
}