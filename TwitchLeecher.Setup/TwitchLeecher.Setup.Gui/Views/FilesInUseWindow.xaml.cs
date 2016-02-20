using TwitchLeecher.Setup.Gui.ViewModels;
using System.Windows;

namespace TwitchLeecher.Setup.Gui.Views
{
    /// <summary>
    /// Interaction logic for FilesInUseWindow.xaml
    /// </summary>
    public partial class FilesInUseWindow : Window
    {
        public FilesInUseWindow()
        {
            InitializeComponent();

            this.DataContextChanged += FilesInUseWindow_DataContextChanged;
        }

        private void FilesInUseWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FilesInUseWindowVM filesInUseWindowVM = e.NewValue as FilesInUseWindowVM;

            if (filesInUseWindowVM != null)
            {
                filesInUseWindowVM.SetWindowCloseAction(this.Close);
                filesInUseWindowVM.SetSetDialogResultAction((result) => this.DialogResult = result);
            }
        }
    }
}