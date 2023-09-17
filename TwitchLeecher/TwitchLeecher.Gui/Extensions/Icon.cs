using System.Windows;

namespace TwitchLeecher.Gui.Extensions
{
    public static class IconExtensions
    {
        public static ImageSource ToImageSource(this System.Drawing.Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}