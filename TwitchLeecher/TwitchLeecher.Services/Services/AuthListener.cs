using System.Net;
using System.Text;
using System.Threading.Tasks;
using TwitchLeecher.Core.Constants;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Services.Services
{
    internal class AuthListener : IAuthListener
    {
        private readonly IAuthService _authService;
        private readonly IEventAggregator _eventAggregator;

        public AuthListener(IAuthService authService, IEventAggregator eventAggregator)
        {
            _authService = authService;
            _eventAggregator = eventAggregator;
        }

        public async Task StartListenForToken()
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add(Constants.RedirectUrl + "/");
            httpListener.Start();
            while (true)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.QueryString.ContainsKey("access_token"))
                {
                    var accessToken = context.Request.QueryString["access_token"];
                    context.Response.StatusCode = 200;
                    var buffer =
                        Encoding.UTF8.GetBytes(
                            "Token recieved, you can close this window!");
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.Close();
                    if (_authService.ValidateAuthentication(accessToken, false))
                    {
                        FireSubOnlyAuthenticationSuccess();
                    }

                    httpListener.Stop();
                }
                else
                {
                    context.Response.StatusCode = 200;
                    var buffer =
                        Encoding.UTF8.GetBytes(
                            "<script>let url = window.location.href.split(\"#\");window.location.href=url[0] + \"?\" + url[1]</script>");
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
            }
        }

        private void FireSubOnlyAuthenticationSuccess()
        {
            _eventAggregator.GetEvent<AuthResultEvent>().Publish(true);
        }
    }
}