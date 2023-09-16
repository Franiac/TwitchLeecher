using System.Threading.Tasks;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IAuthListener
    {
        Task StartListenForToken();
    }
}