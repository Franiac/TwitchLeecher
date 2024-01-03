using System.Windows;
using Avalonia.Controls;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.Views
{
    public partial class UpdateInfoView : Window
    {
        public UpdateInfoView()
        {
            AssemblyUtil au = AssemblyUtil.Get;

            Title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }
    }
}