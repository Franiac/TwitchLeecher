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

        private IKernel _kernel;
        private IEventAggregator _eventAggregator;

        private ViewModelBase _lastView;
        private ViewModelBase _currentView;

        private Dictionary<Type, ViewModelBase> _persistentViews;

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

        public void ShowWelcome()
        {
            Navigate(_kernel.Get<WelcomeViewVM>());
        }

        public void ShowLoading()
        {
            Navigate(_kernel.Get<LoadingViewVM>());
        }

        public void ShowSearch()
        {
            Navigate(_kernel.Get<SearchViewVM>());
        }

        public void ShowSearchResults()
        {
            if (!_persistentViews.TryGetValue(typeof(SearchResultViewVM), out ViewModelBase vm))
            {
                vm = _kernel.Get<SearchResultViewVM>();
                _persistentViews.Add(typeof(SearchResultViewVM), vm);
            }

            Navigate(vm);
        }

        public void ShowDownload(DownloadParameters downloadParams)
        {
            DownloadViewVM vm = _kernel.Get<DownloadViewVM>();
            vm.DownloadParams = downloadParams ?? throw new ArgumentNullException(nameof(downloadParams));

            Navigate(vm);
        }

        public void ShowDownloads()
        {
            if (!_persistentViews.TryGetValue(typeof(DownloadsViewVM), out ViewModelBase vm))
            {
                vm = _kernel.Get<DownloadsViewVM>();
                _persistentViews.Add(typeof(DownloadsViewVM), vm);
            }

            Navigate(vm);
        }

        public void ShowPreferences()
        {
            Navigate(_kernel.Get<PreferencesViewVM>());
        }

        public void ShowInfo()
        {
            Navigate(_kernel.Get<InfoViewVM>());
        }

        public void ShowLog(TwitchVideoDownload download)
        {
            LogViewVM vm = _kernel.Get<LogViewVM>();
            vm.Download = download ?? throw new ArgumentNullException(nameof(download));

            Navigate(vm);
        }

        public void ShowUpdateInfo(UpdateInfo updateInfo)
        {
            UpdateInfoViewVM vm = _kernel.Get<UpdateInfoViewVM>();
            vm.UpdateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));

            Navigate(vm);
        }

        public void NavigateBack()
        {
            if (_lastView != null)
            {
                Navigate(_lastView);
            }
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