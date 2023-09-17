using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using FontAwesome6;
using TwitchLeecher.Gui.Extensions;
using TwitchLeecher.Shared.Native;
using Button = System.Windows.Controls.Button;

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
            SetButtons(MessageBoxButton.OK);
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

        public MessageBoxWindow(string message, string caption, MessageBoxButton buttons, MessageBoxImage image) : this(
            message, caption, buttons)
        {
            SetIcon(image);
        }

        #endregion Constructors

        #region Properties

        public MessageBoxResult Result { get; set; }

        public string Caption
        {
            get { return txtCaption.Text; }
            set
            {
                txtCaption.Text = value;
                Title = value;
            }
        }

        public string Message
        {
            get { return txtMessage.Text; }
            set { txtMessage.Text = value; }
        }

        public string OkButtonText
        {
            get { return btnOkContent.Text; }
            set { btnOkContent.Text = value; }
        }

        public string CancelButtonText
        {
            get { return btnCancelContent.Text; }
            set { btnCancelContent.Text = value; }
        }

        public string YesButtonText
        {
            get { return btnYesContent.Text; }
            set { btnYesContent.Text = value; }
        }

        public string NoButtonText
        {
            get { return btnNoContent.Text; }
            set { btnNoContent.Text = value; }
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
                    FaIcon.Icon = EFontAwesomeIcon.Solid_TriangleExclamation;
                    break;

                // MessageBoxImage.Hand also has value 16
                // MessageBoxImage.Stop also has value 16
                case MessageBoxImage.Error:
                    FaIcon.Icon = EFontAwesomeIcon.Solid_Ban;
                    break;

                // MessageBoxImage.Asterisk also has value 64
                case MessageBoxImage.Information:
                    FaIcon.Icon = EFontAwesomeIcon.Solid_CircleInfo;
                    break;

                case MessageBoxImage.Question:
                    FaIcon.Icon = EFontAwesomeIcon.Solid_CircleQuestion;
                    break;

                default:
                    FaIcon.Icon = EFontAwesomeIcon.Solid_CircleInfo;
                    break;
            }
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