using CefSharp;
using System.Collections.Generic;

namespace TwitchLeecher.Gui.Browser
{
    public class DeleteCookieVisitor : ICookieVisitor
    {
        private static readonly List<string> keep = new List<string>()
        {
            "api_token",
            "server_session_id",
            "twitch.lohp.countryCode",
            "unique_id",
            "unique_id_durable"
        };

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            if (!keep.Contains(cookie.Name))
            {
                deleteCookie = true;
            }

            return true;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}