using System;

namespace TwitchLeecher.Services.Services
{
    internal class ThemeService : IThemeService
    {
        private string _name = "Original";
        public event EventHandler StyleChanged;

        public void SetTheme(string name)
        {
            _name = name;
            StyleChanged?.Invoke(this, null);
        }

        public string GetTheme()
        {
            return _name;
        }
    }
}