using Ninject;
using System;
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

        private IKernel kernel;
        private IEventAggregator eventAggregator;

        private ViewModelBase lastView;
        private ViewModelBase currentView;

        #endregion Fields

        #region Constructor

        public NavigationService(IKernel kernel, IEventAggregator eventAggregator)
        {
            this.kernel = kernel;
            this.eventAggregator = eventAggregator;
        }

        #endregion Constructor

        #region Methods

        public void ShowWelcome()
        {
            this.Navigate(this.kernel.Get<WelcomeViewVM>());
        }

        public void ShowLoading()
        {
            this.Navigate(this.kernel.Get<LoadingViewVM>());
        }

        public void ShowSearch()
        {
            this.Navigate(this.kernel.Get<SearchViewVM>());
        }

        public void ShowSearchResults()
        {
            this.Navigate(this.kernel.Get<SearchResultViewVM>());
        }

        public void ShowDownload(DownloadParameters downloadParams)
        {
            if (downloadParams == null)
            {
                throw new ArgumentNullException(nameof(downloadParams));
            }

            DownloadViewVM vm = this.kernel.Get<DownloadViewVM>();
            vm.DownloadParams = downloadParams;

            this.Navigate(vm);
        }

        public void ShowDownloads()
        {
            this.Navigate(this.kernel.Get<DownloadsViewVM>());
        }

        public void ShowAuthorize()
        {
            this.Navigate(this.kernel.Get<AuthorizeViewVM>());
        }

        public void ShowRevokeAuthorization()
        {
            this.Navigate(this.kernel.Get<RevokeAuthorizationViewVM>());
        }

        public void ShowTwitchConnect()
        {
            this.Navigate(this.kernel.Get<TwitchConnectViewVM>());
        }

        public void ShowPreferences()
        {
            this.Navigate(this.kernel.Get<PreferencesViewVM>());
        }

        public void ShowInfo()
        {
            this.Navigate(this.kernel.Get<InfoViewVM>());
        }

        public void ShowLog(TwitchVideoDownload download)
        {
            if (download == null)
            {
                throw new ArgumentNullException(nameof(download));
            }

            LogViewVM vm = this.kernel.Get<LogViewVM>();
            vm.Download = download;

            this.Navigate(vm);
        }

        public void ShowUpdateInfo(UpdateInfo updateInfo)
        {
            if (updateInfo == null)
            {
                throw new ArgumentNullException(nameof(updateInfo));
            }

            UpdateInfoViewVM vm = this.kernel.Get<UpdateInfoViewVM>();
            vm.UpdateInfo = updateInfo;

            this.Navigate(vm);
        }

        public void NavigateBack()
        {
            if (this.lastView != null)
            {
                this.Navigate(this.lastView);
            }
        }

        private void Navigate(ViewModelBase nextView)
        {
            if (nextView == null || (this.currentView != null && this.currentView.GetType() == nextView.GetType()))
            {
                return;
            }

            this.currentView?.OnBeforeHidden();

            nextView.OnBeforeShown();

            this.lastView = this.currentView;

            this.currentView = nextView;

            this.eventAggregator.GetEvent<ShowViewEvent>().Publish(nextView);
        }

        #endregion Methods
    }
}