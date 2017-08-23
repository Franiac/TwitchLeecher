using Xceed.Wpf.Toolkit;

namespace TwitchLeecher.Gui.Controls
{
    public class TlDateTimePicker : DateTimePicker
    {
        #region Methods

        protected override void OnSpin(SpinEventArgs e)
        {
            try
            {
                base.OnSpin(e);
            }
            catch
            {
                // Caused if non-spinable part of the string is selected
            }
        }

        #endregion Methods
    }
}