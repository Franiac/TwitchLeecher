using System;
using System.Windows;
using TwitchLeecher.Setup.Gui.ViewModels;

namespace TwitchLeecher.Setup.Gui
{
    public partial class WizardWindow : Window
    {
        public WizardWindow()
        {
            InitializeComponent();

            DataContextChanged += WizardWindow_DataContextChanged;
        }

        private void WizardWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is WizardWindowVM oldWizardWindowVM)
            {
                oldWizardWindowVM.WiazrdFinished -= WizardWindowVM_WiazrdFinished;
            }

            if (e.NewValue is WizardWindowVM newWizardWindowVM)
            {
                newWizardWindowVM.WiazrdFinished += WizardWindowVM_WiazrdFinished;
            }
        }

        private void WizardWindowVM_WiazrdFinished(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured!" + Environment.NewLine + Environment.NewLine + ex.ToString(), "Error", MessageBoxButton.OK);
            }
        }
    }
}