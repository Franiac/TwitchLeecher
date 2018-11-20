using System;
using System.Xml.Linq;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Core.Models
{
    public class MainWindowInfo
    {
        #region Constants

        public const string MAINWINDOW_EL = "MainWindow";

        private const string MAINWINDOW_WIDTH_EL = "Width";
        private const string MAINWINDOW_HEIGHT_EL = "Height";
        private const string MAINWINDOW_TOP_EL = "Top";
        private const string MAINWINDOW_LEFT_EL = "Left";
        private const string MAINWINDOW_ISMAXIMIZED_EL = "IsMaximized";

        #endregion Constants

        #region Properties

        public double Width { get; set; }

        public double Height { get; set; }

        public double Top { get; set; }

        public double Left { get; set; }

        public bool IsMaximized { get; set; }

        #endregion Properties

        #region Methods

        public XElement GetXml()
        {
            XElement mainWindowInfoEl = new XElement(MAINWINDOW_EL);

            XElement widthEl = new XElement(MAINWINDOW_WIDTH_EL);
            widthEl.SetValue(Math.Round(Width));
            mainWindowInfoEl.Add(widthEl);

            XElement heightEl = new XElement(MAINWINDOW_HEIGHT_EL);
            heightEl.SetValue(Math.Round(Height));
            mainWindowInfoEl.Add(heightEl);

            XElement topEl = new XElement(MAINWINDOW_TOP_EL);
            topEl.SetValue(Math.Round(Top));
            mainWindowInfoEl.Add(topEl);

            XElement leftEl = new XElement(MAINWINDOW_LEFT_EL);
            leftEl.SetValue(Math.Round(Left));
            mainWindowInfoEl.Add(leftEl);

            XElement isMaximizedEl = new XElement(MAINWINDOW_ISMAXIMIZED_EL);
            isMaximizedEl.SetValue(IsMaximized);
            mainWindowInfoEl.Add(isMaximizedEl);

            return mainWindowInfoEl;
        }

        #endregion Methods

        #region Static Methods

        public static MainWindowInfo GetFromXml(XElement mainWindowInfoEl)
        {
            MainWindowInfo mainWindowInfo = new MainWindowInfo();

            if (mainWindowInfoEl != null)
            {
                XElement widthEl = mainWindowInfoEl.Element(MAINWINDOW_WIDTH_EL);

                if (widthEl != null)
                {
                    try
                    {
                        mainWindowInfo.Width = (int)Math.Round(widthEl.GetValueAsDouble());
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
                else
                {
                    // Malformed XML
                    return null;
                }

                XElement heightEl = mainWindowInfoEl.Element(MAINWINDOW_HEIGHT_EL);

                if (heightEl != null)
                {
                    try
                    {
                        mainWindowInfo.Height = (int)Math.Round(heightEl.GetValueAsDouble());
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
                else
                {
                    // Malformed XML
                    return null;
                }

                XElement topEl = mainWindowInfoEl.Element(MAINWINDOW_TOP_EL);

                if (topEl != null)
                {
                    try
                    {
                        mainWindowInfo.Top = (int)Math.Round(topEl.GetValueAsDouble());
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
                else
                {
                    // Malformed XML
                    return null;
                }

                XElement leftEl = mainWindowInfoEl.Element(MAINWINDOW_LEFT_EL);

                if (leftEl != null)
                {
                    try
                    {
                        mainWindowInfo.Left = (int)Math.Round(leftEl.GetValueAsDouble());
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
                else
                {
                    // Malformed XML
                    return null;
                }

                XElement isMaximizedEl = mainWindowInfoEl.Element(MAINWINDOW_ISMAXIMIZED_EL);

                if (isMaximizedEl != null)
                {
                    try
                    {
                        mainWindowInfo.IsMaximized = isMaximizedEl.GetValueAsBool();
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
                else
                {
                    // Malformed XML
                    return null;
                }
            }

            return mainWindowInfo;
        }

        #endregion Static Methods
    }
}