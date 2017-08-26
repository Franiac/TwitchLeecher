using System.Windows.Controls;

namespace TwitchLeecher.Gui.Controls
{
    public class TlScrollingTextBox : TextBox
    {
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            CaretIndex = Text.Length;
            ScrollToEnd();
        }
    }
}