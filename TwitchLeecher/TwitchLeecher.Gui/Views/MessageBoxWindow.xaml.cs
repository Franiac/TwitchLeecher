using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;
using TwitchLeecher.Gui.Extensions;
using TwitchLeecher.Shared.Native;

namespace TwitchLeecher.Gui.Views
{
    public partial class MessageBoxWindow : Window
    {
        #region Constructors

        public MessageBoxWindow(string message)
        {
            InitializeComponent();

            WindowChrome windowChrome = new WindowChrome()
            {
                CaptionHeight = 51,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                NonClientFrameEdges = NonClientFrameEdges.None,
                ResizeBorderThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };

            WindowChrome.SetWindowChrome(this, windowChrome);

            Message = message;
            rowCaption.Visibility = Visibility.Collapsed;
            imgIcon.Visibility = Visibility.Collapsed;
            SetButtons(MessageBoxButton.OK);

            NativeDisplay display = NativeDisplay.GetDisplayFromWindow(new WindowInteropHelper(this).Handle);

            MaxWidth = display.WorkingArea.Width * 0.9;
            MaxHeight = display.WorkingArea.Height * 0.9;
        }

        public MessageBoxWindow(string message, string caption) : this(message)
        {
            Caption = caption;
            rowCaption.Visibility = Visibility.Visible;
        }

        public MessageBoxWindow(string message, string caption, MessageBoxButton buttons) : this(message, caption)
        {
            SetButtons(buttons);
        }

        public MessageBoxWindow(string message, string caption, MessageBoxImage image) : this(message, caption)
        {
            SetIcon(image);
        }

        public MessageBoxWindow(string message, string caption, MessageBoxButton buttons, MessageBoxImage image) : this(message, caption, buttons)
        {
            SetIcon(image);
        }

        #endregion Constructors

        #region Properties

        public MessageBoxResult Result { get; set; }

        public string Caption
        {
            get
            {
                return txtCaption.Text;
            }
            set
            {
                txtCaption.Text = value;
                Title = value;
            }
        }

        public string Message
        {
            get
            {
                return txtMessage.Text;
            }
            set
            {
                txtMessage.Text = value;
            }
        }

        public string OkButtonText
        {
            get
            {
                return btnOkContent.Text;
            }
            set
            {
                btnOkContent.Text = value;
            }
        }

        public string CancelButtonText
        {
            get
            {
                return btnCancelContent.Text;
            }
            set
            {
                btnCancelContent.Text = value;
            }
        }

        public string YesButtonText
        {
            get
            {
                return btnYesContent.Text;
            }
            set
            {
                btnYesContent.Text = value;
            }
        }

        public string NoButtonText
        {
            get
            {
                return btnNoContent.Text;
            }
            set
            {
                btnNoContent.Text = value;
            }
        }

        #endregion Properties

        #region Methods

        private void SetIcon(MessageBoxImage image)
        {
            Icon icon;

            switch (image)
            {
                // MessageBoxImage.Exclamation also has value 48
                case MessageBoxImage.Warning:
                    icon = SystemIcons.Exclamation;
                    break;

                // MessageBoxImage.Hand also has value 16
                // MessageBoxImage.Stop also has value 16
                case MessageBoxImage.Error:
                    icon = SystemIcons.Hand;
                    break;

                // MessageBoxImage.Asterisk also has value 64
                case MessageBoxImage.Information:
                    icon = SystemIcons.Information;
                    break;

                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;

                default:
                    icon = SystemIcons.Information;
                    break;
            }

            imgIcon.Source = icon.ToImageSource();
            imgIcon.Visibility = Visibility.Visible;
        }

        private void SetButtons(MessageBoxButton buttons)
        {
            List<Button> dialogButtons = new List<Button>() { btnYes, btnNo, btnOk, btnCancel };

            foreach (Button button in dialogButtons)
            {
                button.Visibility = Visibility.Collapsed;
                button.Margin = new Thickness(0);
                button.IsDefault = false;
                button.IsCancel = false;
            }

            switch (buttons)
            {
                case MessageBoxButton.OKCancel:
                    btnOk.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnOk.Margin = new Thickness(0, 0, 3, 0);
                    btnCancel.Margin = new Thickness(3, 0, 0, 0);
                    btnCancel.IsDefault = true;
                    btnCancel.IsCancel = true;
                    break;

                case MessageBoxButton.YesNo:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    btnYes.Margin = new Thickness(0, 0, 3, 0);
                    btnNo.Margin = new Thickness(3, 0, 0, 0);
                    btnNo.IsDefault = true;
                    btnNo.IsCancel = true;
                    break;

                case MessageBoxButton.YesNoCancel:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    btnYes.Margin = new Thickness(0, 0, 3, 0);
                    btnNo.Margin = new Thickness(3, 0, 3, 0);
                    btnCancel.Margin = new Thickness(3, 0, 0, 0);
                    btnCancel.IsDefault = true;
                    btnCancel.IsCancel = true;
                    break;

                default:
                    btnOk.Visibility = Visibility.Visible;
                    btnOk.IsDefault = true;
                    btnOk.IsCancel = true;
                    break;
            }
        }

        #endregion Methods

        #region EventHandlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Application.Current.MainWindow;

            WindowState mainWindowState = mainWindow.WindowState;

            if (mainWindowState == WindowState.Maximized)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                int x = (int)Math.Round(mainWindow.Left + mainWindow.Width / 2, 0);
                int y = (int)Math.Round(mainWindow.Top + mainWindow.Height / 2, 0);

                int width = (int)Math.Round(Width, 0);
                int height = (int)Math.Round(Height, 0);

                Left = x - width / 2;
                Top = y - height / 2;
            }
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        #endregion EventHandlers
    }
}