using TwitchLeecher.Setup.Gui.ViewModels;
using System;
using System.Windows;

namespace TwitchLeecher.Setup.Gui
{
    public partial class WizardWindow : Window
    {
        public WizardWindow()
        {
            InitializeComponent();

            this.DataContextChanged += WizardWindow_DataContextChanged;
        }

        private void WizardWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            WizardWindowVM oldWizardWindowVM = e.OldValue as WizardWindowVM;

            if (oldWizardWindowVM != null)
            {
                oldWizardWindowVM.WiazrdFinished -= wizardWindowVM_WiazrdFinished;
            }

            WizardWindowVM newWizardWindowVM = e.NewValue as WizardWindowVM;

            if (newWizardWindowVM != null)
            {
                newWizardWindowVM.WiazrdFinished += wizardWindowVM_WiazrdFinished;
            }
        }

        private void wizardWindowVM_WiazrdFinished(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured!" + Environment.NewLine + Environment.NewLine + ex.ToString(), "Error", MessageBoxButton.OK);
            }
        }
    }
}