using System.Windows;
using System.Windows.Controls;

namespace TwitchLeecher.Gui.Decorators
{
    public class NoWidthDecorator : Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            Size baseSize = base.MeasureOverride(constraint);

            return new Size(0, baseSize.Height);
        }
    }
}