using System;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Notification;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class InfoViewVM : BindableBase
    {
        #region Fields

        private string productName;

        private ICommand linkCommand;
        private ICommand donateCommand;

        private IGuiService guiService;

        #endregion Fields

        #region Constructors

        public InfoViewVM(IGuiService guiService)
        {
            AssemblyUtil au = AssemblyUtil.Get;

            this.productName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.guiService = guiService;
        }

        #endregion Constructors

        #region Properties

        public string ProductName
        {
            get
            {
                return this.productName;
            }
        }

        public ICommand LinkCommand
        {
            get
            {
                if (this.linkCommand == null)
                {
                    this.linkCommand = new DelegateCommand<string>(link =>
                    {
                        try
                        {
                            Process.Start(link);
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.linkCommand;
            }
        }

        public ICommand DonateCommand
        {
            get
            {
                if (this.donateCommand == null)
                {
                    this.donateCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=WYGSLTBJFMAVE");
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.donateCommand;
            }
        }

        #endregion Properties
    }
}