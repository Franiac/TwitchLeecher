using System.Xml.Linq;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Core.Models
{
    public class RuntimeData
    {
        #region Constants

        private const string RUNTIMEDATA_EL = "RuntimeData";

        private const string AUTH_EL = "Authorization";
        private const string AUTH_ACCESSTOKEN_EL = "AccessToken";

        private const string APP_EL = "Application";

        #endregion Constants

        #region Properties

        public string AccessToken { get; set; }

        public MainWindowInfo MainWindowInfo { get; set; }

        #endregion Properties

        #region Methods

        public XElement GetXml()
        {
            XElement runtimeDataEl = new XElement(RUNTIMEDATA_EL);

            if (!string.IsNullOrWhiteSpace(this.AccessToken))
            {
                XElement authEl = new XElement(AUTH_EL);
                runtimeDataEl.Add(authEl);

                XElement accessTokenEl = new XElement(AUTH_ACCESSTOKEN_EL);
                accessTokenEl.SetValue(this.AccessToken);
                authEl.Add(accessTokenEl);
            }

            if (this.MainWindowInfo != null)
            {
                XElement mainWindowInfoEl = this.MainWindowInfo.GetXml();

                if (mainWindowInfoEl.HasElements)
                {
                    XElement applicationEl = new XElement(APP_EL);
                    applicationEl.Add(mainWindowInfoEl);
                    runtimeDataEl.Add(applicationEl);
                }
            }

            return runtimeDataEl;
        }

        #endregion Methods

        #region Static Methods

        public static RuntimeData GetFromXml(XElement runtimedataEl)
        {
            RuntimeData runtimeData = new RuntimeData();

            if (runtimedataEl != null)
            {
                XElement authEl = runtimedataEl.Element(AUTH_EL);

                if (authEl != null)
                {
                    XElement accessTokenEl = authEl.Element(AUTH_ACCESSTOKEN_EL);

                    if (accessTokenEl != null)
                    {
                        try
                        {
                            runtimeData.AccessToken = accessTokenEl.GetValueAsString();
                        }
                        catch
                        {
                            // Value from config file could not be loaded, use default value
                        }
                    }
                }

                XElement applicationEl = runtimedataEl.Element(APP_EL);

                if (applicationEl != null)
                {
                    XElement mainWindowInfoEl = applicationEl.Element(MainWindowInfo.MAINWINDOW_EL);

                    if (mainWindowInfoEl != null)
                    {
                        try
                        {
                            runtimeData.MainWindowInfo = MainWindowInfo.GetFromXml(mainWindowInfoEl);
                        }
                        catch
                        {
                            // Value from config file could not be loaded, use default value
                        }
                    }
                }
            }

            return runtimeData;
        }

        #endregion Static Methods
    }
}