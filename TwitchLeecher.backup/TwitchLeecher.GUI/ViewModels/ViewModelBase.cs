using System.Collections.Generic;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.ViewModels
{
    public abstract class ViewModelBase : BindableBase
    {
        #region Fields

        private List<MenuCommand> _menuCommands;

        #endregion Fields

        #region Properties

        public bool HasMenu
        {
            get
            {
                List<MenuCommand> menuCommands = MenuCommands;

                return menuCommands != null && menuCommands.Count > 0;
            }
        }

        public List<MenuCommand> MenuCommands
        {
            get
            {
                if (_menuCommands == null)
                {
                    List<MenuCommand> menuCommands = BuildMenu();

                    if (menuCommands == null)
                    {
                        menuCommands = new List<MenuCommand>();
                    }

                    _menuCommands = menuCommands;
                }

                return _menuCommands;
            }
        }

        #endregion Properties

        #region Methods

        public virtual void OnBeforeShown()
        {
        }

        public virtual void OnBeforeHidden()
        {
        }

        protected virtual List<MenuCommand> BuildMenu()
        {
            return new List<MenuCommand>();
        }

        #endregion Methods
    }
}