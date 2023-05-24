using System.Diagnostics;
using System.Globalization;
using TwitchLeecher.Gui.Interfaces;

namespace TwitchLeecher.Gui.Services
{
    internal class DonationService : IDonationService
    {
        #region Constants

        private const string donationLink = "https://www.tipeeestream.com/brainyxs/donation";

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