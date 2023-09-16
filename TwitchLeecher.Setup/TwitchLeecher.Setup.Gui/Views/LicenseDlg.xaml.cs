using System.Windows;
using System.Windows.Controls;

namespace TwitchLeecher.Setup.Gui.Views
{
    public partial class LicenseDlg : UserControl
    {
        public LicenseDlg()
        {
            InitializeComponent();

            Loaded += LicenseDlg_Loaded;
        }

        private void LicenseDlg_Loaded(object sender, RoutedEventArgs e)
        {
            txtLicense.Text = Properties.Resources.LICENSE;
        }
    }
}