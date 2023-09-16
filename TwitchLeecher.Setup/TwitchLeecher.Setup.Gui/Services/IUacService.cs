using System.Windows.Media.Imaging;

namespace TwitchLeecher.Setup.Gui.Services
{
    internal interface IUacService
    {
        bool IsUacEnabled { get; }

        bool IsUserAdmin { get; }

        BitmapImage UacIcon { get; }
    }
}