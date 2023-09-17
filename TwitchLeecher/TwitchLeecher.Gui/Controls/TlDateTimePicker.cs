using Avalonia.Controls;

namespace TwitchLeecher.Gui.Controls
{
    public class TlDateTimePicker : DatePicker
    {
        #region Methods
        protected override void OnIncrement()
        {
            base.OnIncrement();

            if (!TextBox.IsFocused)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }

        protected override void OnDecrement()
        {
            base.OnDecrement();

            if (!TextBox.IsFocused)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox.Text))
            {
                // UI does not refresh when leaving the control with an emtpy text
                // We have to force a refresh by setting 'Value' to null first
                Value = null;
                Value = DefaultValue;
            }

            base.OnLostKeyboardFocus(e);
        }

        #endregion Methods
    }
}