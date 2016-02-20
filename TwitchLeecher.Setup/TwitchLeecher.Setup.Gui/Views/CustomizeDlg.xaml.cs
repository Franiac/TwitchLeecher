using TwitchLeecher.Setup.Gui.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace TwitchLeecher.Setup.Gui.Views
{
    public partial class CustomizeDlg : UserControl
    {
        public CustomizeDlg()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomizeDlgVM installFolderDlgVM = this.DataContext as CustomizeDlgVM;

                if (installFolderDlgVM == null)
                {
                    throw new ApplicationException("DataContext is not set to an instance of type '" + typeof(CustomizeDlgVM).FullName + "'!");
                }

                using (FolderBrowserDialog folderDlg = new FolderBrowserDialog())
                {
                    folderDlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    folderDlg.Description = "Choose Install Folder";

                    DialogResult dlgRes = folderDlg.ShowDialog();

                    if (dlgRes == DialogResult.OK)
                    {
                        this.txtInstallDir.Text = Path.Combine(folderDlg.SelectedPath, installFolderDlgVM.Bootstrapper.ProductName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
            }
        }
    }
}