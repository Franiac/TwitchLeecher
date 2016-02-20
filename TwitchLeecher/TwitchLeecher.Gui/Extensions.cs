using Prism.Regions;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TwitchLeecher.Gui
{
    public static class Extensions
    {
        public static object GetView<T>(this IRegion region)
        {
            foreach (object view in region.Views)
            {
                if (view.GetType() == typeof(T))
                {
                    return view;
                }
            }

            return null;
        }

        public static ImageSource ToImageSource(this Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}