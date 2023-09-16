using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SubOnlyViewVM : ViewModelBase
    {
        private readonly IAuthListener _authListener;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;

        public SubOnlyViewVM(IAuthListener authListener, IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            _authListener = authListener;
            _eventAggregator = eventAggregator;
            _navigationService = navigationService;
        }

        public override void OnBeforeShown()
        {
            _eventAggregator.GetEvent<SubOnlyAuthChangedEvent>().Subscribe(Changed);
            _authListener.StartListenForToken();
            base.OnBeforeShown();
        }

        public override void OnBeforeHidden()
        {
            _eventAggregator.GetEvent<SubOnlyAuthChangedEvent>().Unsubscribe(Changed);
            base.OnBeforeHidden();
        }

        private void Changed(bool obj)
        {
            _navigationService.NavigateBack();
        }
    }
}