using System;
using System.Linq;
using CockyGrabber;
using CockyGrabber.Grabbers;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Services.Services
{
    internal class CookieService : ICookieService
    {
        private readonly IRuntimeDataService _runtimeDataService;

        public CookieService(IRuntimeDataService runtimeDataService)
        {
            _runtimeDataService = runtimeDataService;
        }

        public bool GrabTwitchSessionToken()
        {
            var braveToken = GetTokenFromBrave();
            if (braveToken != null)
            {
                _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly = braveToken;
                return true;
            }

            var chromeToken = GetTokenFromChrome();
            if (chromeToken != null)
            {
                _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly = chromeToken;
                return true;
            }

            var operaToken = GetTokenFromOpera();
            if (operaToken != null)
            {
                _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly = operaToken;
                return true;
            }

            var operaGxToken = GetTokenFromOperaGx();
            if (operaGxToken != null)
            {
                _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly = operaGxToken;
                return true;
            }

            var firefoxToken = GetTokenFromFirefox();
            if (firefoxToken != null)
            {
                _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly = firefoxToken;
                return true;
            }

            return false;
        }

        private string GetTokenFromChrome()
        {
            try
            {
                var grabber = new ChromeGrabber();
                return GetCookie(grabber);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetTokenFromFirefox()
        {
            try
            {
                var grabber = new FirefoxGrabber();
                return GetCookie(grabber);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetTokenFromOpera()
        {
            try
            {
                var grabber = new OperaGrabber();
                return GetCookie(grabber);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetTokenFromOperaGx()
        {
            try
            {
                var grabber = new OperaGxGrabber();
                return GetCookie(grabber);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetTokenFromBrave()
        {
            try
            {
                var grabber = new BraveGrabber();
                return GetCookie(grabber);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetCookie(BlinkGrabber grabber)
        {
            return grabber.GetCookiesBy(Blink.Cookie.Header.host_key, ".twitch.tv")
                .Where(cookie => cookie.Name == "auth-token").Select(cookie => cookie.DecryptedValue)
                .FirstOrDefault();
        }

        private string GetCookie(GeckoGrabber grabber)
        {
            return grabber.GetCookiesBy(Gecko.Cookie.Header.host, ".twitch.tv")
                .Where(cookie => cookie.Name == "auth-token").Select(cookie => cookie.Value)
                .FirstOrDefault();
        }
    }
}