using System.Windows.Controls;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Views
{
    public partial class PreferencesView : UserControl
    {
        public PreferencesView()
        {
            InitializeComponent();

            this.cmbLoadLimit.ItemsSource = Preferences.GetLoadLimits();
        }
    }
}