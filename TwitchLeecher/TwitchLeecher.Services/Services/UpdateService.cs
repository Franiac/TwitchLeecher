using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Text;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class UpdateService : IUpdateService
    {
        #region Constants

        private const string LatestReleaseUrl = "https://github.com/schneidermanuel/TwitchLeecher.Old-dx/releases/tag/v{0}";
        private const string ReleasesApiUrl = "https://api.github.com/repos/schneidermanuel/TwitchLeecher.Old-dx/releases";

        #endregion Constants

        #region Methods

        public UpdateInfo CheckForUpdate()
        {
            try
            {
                using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "TwitchLeecher.Old");

                    string result = webClient.DownloadString(ReleasesApiUrl);

                    JToken releasesJson = JToken.Parse(result);

                    foreach (JToken releaseJson in releasesJson)
                    {
                        bool draft = releaseJson.Value<bool>("draft");
                        bool prerelease = releaseJson.Value<bool>("prerelease");

                        if (!draft && !prerelease)
                        {
                            string tagStr = releaseJson.Value<string>("tag_name");
                            string releasedStr = releaseJson.Value<string>("published_at");
                            string infoStr = releaseJson.Value<string>("body");

                            Version releaseVersion = Version.Parse(tagStr.Substring(1)).Pad();
                            Version localVersion = AssemblyUtil.Get.GetAssemblyVersion().Pad();

                            DateTime released = DateTime.Parse(releasedStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                            if (releaseVersion > localVersion)
                            {
                                return new UpdateInfo(releaseVersion, released, string.Format(LatestReleaseUrl, releaseVersion.ToString(3)), infoStr);
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Update check should not distract the application
            }

            return null;
        }

        #endregion Methods
    }
}