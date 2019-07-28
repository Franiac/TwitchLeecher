using System.Windows;
using TwitchLeecher.Setup.Gui.ViewModels;

namespace TwitchLeecher.Setup.Gui.Views
{
    public partial class FilesInUseWindow : Window
    {
        public FilesInUseWindow()
        {
            InitializeComponent();

            DataContextChanged += FilesInUseWindow_DataContextChanged;
        }

        private void FilesInUseWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FilesInUseWindowVM filesInUseWindowVM)
            {
                filesInUseWindowVM.SetWindowCloseAction(Close);
                filesInUseWindowVM.SetSetDialogResultAction((result) => DialogResult = result);
            }
        }
    }
}