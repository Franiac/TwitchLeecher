using Ninject;
using System;
using System.Collections.Generic;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.Services
{
    internal class NavigationService : INavigationService
    {
        #region Fields

        private readonly IKernel _kernel;
        private readonly IEventAggregator _eventAggregator;

        private ViewModelBase _lastView;
        private ViewModelBase _currentView;

        private readonly Dictionary<Type, ViewModelBase> _persistentViews;

        #endregion Fields

        #region Constructor

        public NavigationService(IKernel kernel, IEventAggregator eventAggregator)
        {
            _kernel = kernel;
            _eventAggregator = eventAggregator;

            _persistentViews = new Dictionary<Type, ViewModelBase>();
        }

        #endregion Constructor

        #region Methods

        public void ShowAuth()
        {
            Navigate(_kernel.Get<AuthViewModel>());
        }


        public void ShowWelcome()
        {
            Navigate(_kernel.Get<WelcomeViewModel>());
        }

        public void ShowLoading()
        {
            Navigate(_kernel.Get<LoadingViewModel>());
        }

        public void ShowSearch()
        {
            Navigate(_kernel.Get<SearchViewModel>());
        }

        public void ShowSearchResults()
        {
            if (!_persistentViews.TryGetValue(typeof(SearchResultViewModel), out ViewModelBase vm))
            {
                vm = _kernel.Get<SearchResultViewModel>();
                _persistentViews.Add(typeof(SearchResultViewModel), vm);
            }

            Navigate(vm);
        }

        public void ShowDownload(DownloadParameters downloadParams)
        {
            DownloadViewModel model = _kernel.Get<DownloadViewModel>();
            model.DownloadParams = downloadParams ?? throw new ArgumentNullException(nameof(downloadParams));

            Navigate(model);
        }

        public void ShowDownloads()
        {
            if (!_persistentViews.TryGetValue(typeof(DownloadsViewModel), out ViewModelBase vm))
            {
                vm = _kernel.Get<DownloadsViewModel>();
                _persistentViews.Add(typeof(DownloadsViewModel), vm);
            }

            Navigate(vm);
        }

        public void ShowPreferences()
        {
            Navigate(_kernel.Get<PreferencesViewModel>());
        }

        public void ShowInfo()
        {
            Navigate(_kernel.Get<InfoViewModel>());
        }

        public void ShowLog(TwitchVideoDownload download)
        {
            LogViewModel model = _kernel.Get<LogViewModel>();
            model.Download = download ?? throw new ArgumentNullException(nameof(download));

            Navigate(model);
        }

        public void NavigateBack()
        {
            if (_lastView != null)
            {
                Navigate(_lastView);
            }
        }

        public void ShowAuthSubOnly()
        {
            Navigate(_kernel.Get<SubOnlyViewModel>());
        }

        private void Navigate(ViewModelBase nextView)
        {
            if (nextView == null || (_currentView != null && _currentView.GetType() == nextView.GetType()))
            {
                return;
            }

            _currentView?.OnBeforeHidden();

            nextView.OnBeforeShown();

            _lastView = _currentView;

            _currentView = nextView;

            _eventAggregator.GetEvent<ShowViewEvent>().Publish(nextView);
        }

        #endregion Methods
    }
}