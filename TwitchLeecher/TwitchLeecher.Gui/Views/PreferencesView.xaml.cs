using System.Windows.Controls;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Views
{
    public partial class PreferencesView : UserControl
    {
        public PreferencesView()
        {
            InitializeComponent();

            cmbLoadLimit.ItemsSource = Preferences.GetLoadLimits();
        }
    }
}