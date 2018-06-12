using System.Diagnostics;
using System.Globalization;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Services
{
    internal class DonationService : IDonationService
    {
        #region Constants

        private const string donationLink = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=R43T6CB8SG2HL";

        #endregion Constants

        #region Methods

        public void OpenDonationPage()
        {
            Process.Start(GetDonationLink());
        }

        private string GetDonationLink()
        {
            CultureInfo ci = CultureInfo.CurrentUICulture;

            string culture = ci == null ? "en_US" : ci.Name.Replace("-", "_");

            return donationLink + "&lc=" + culture;
        }

        #endregion Methods
    }
}