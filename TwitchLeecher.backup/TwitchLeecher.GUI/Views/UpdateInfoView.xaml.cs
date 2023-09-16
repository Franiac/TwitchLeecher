using System.Windows;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.Views
{
    public partial class UpdateInfoView : Window
    {
        public UpdateInfoView()
        {
            InitializeComponent();

            AssemblyUtil au = AssemblyUtil.Get;

            Title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }
    }
}