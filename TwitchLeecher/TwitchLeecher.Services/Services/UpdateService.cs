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
        private const string latestReleaseUrl = "https://github.com/Franiac/TwitchLeecher/releases/tag/v{0}";
        private const string releasesApiUrl = "https://api.github.com/repos/Franiac/TwitchLeecher/releases";

        public bool CheckForUpdate(out UpdateInfo updateInfo)
        {
            try
            {
                using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "TwitchLeecher");

                    string result = webClient.DownloadString(releasesApiUrl);

                    JToken releasesJson = JToken.Parse(result);

                    JToken latestReleaseJson = releasesJson.First;

                    string tagStr = latestReleaseJson.Value<string>("tag_name");
                    string releasedStr = latestReleaseJson.Value<string>("published_at");
                    string infoStr = latestReleaseJson.Value<string>("body");

                    Version releaseVersion = Version.Parse(tagStr.Substring(1));

                    Version localVersion = AssemblyUtil.Get.GetAssemblyVersion();

                    DateTime released = DateTime.Parse(releasedStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

                    string downloadUrl = string.Format(latestReleaseUrl, releaseVersion.Trim().ToString());

                    updateInfo = new UpdateInfo(releaseVersion, released, downloadUrl, infoStr);

                    return releaseVersion > localVersion;
                }
            }
            catch
            {
                updateInfo = null;
            }

            return false;
        }
    }
}