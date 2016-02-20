using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

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

            this.Message = message;
            this.rowCaption.Visibility = Visibility.Collapsed;
            this.imgIcon.Visibility = Visibility.Collapsed;
            this.SetButtons(MessageBoxButton.OK);
        }

        public MessageBoxWindow(string message, string caption) : this(message)
        {
            this.Caption = caption;
            this.rowCaption.Visibility = Visibility.Visible;
        }

        public MessageBoxWindow(string message, string caption, MessageBoxButton buttons) : this(message, caption)
        {
            this.SetButtons(buttons);
        }

        public MessageBoxWindow(string message, string caption, MessageBoxImage image) : this(message, caption)
        {
            this.SetIcon(image);
        }

        public MessageBoxWindow(string message, string caption, MessageBoxButton buttons, MessageBoxImage image) : this(message, caption, buttons)
        {
            this.SetIcon(image);
        }

        #endregion Constructors

        #region Properties

        public MessageBoxResult Result { get; set; }

        private MessageBoxResult DefaultResult { get; set; }

        public string Caption
        {
            get
            {
                return this.txtCaption.Text;
            }
            set
            {
                this.txtCaption.Text = value;
                this.Title = value;
            }
        }

        public string Message
        {
            get
            {
                return this.txtMessage.Text;
            }
            set
            {
                this.txtMessage.Text = value;
            }
        }

        public string OkButtonText
        {
            get
            {
                return this.btnOkContent.Text;
            }
            set
            {
                this.btnOkContent.Text = value;
            }
        }

        public string CancelButtonText
        {
            get
            {
                return this.btnCancelContent.Text;
            }
            set
            {
                this.btnCancelContent.Text = value;
            }
        }

        public string YesButtonText
        {
            get
            {
                return this.btnYesContent.Text;
            }
            set
            {
                this.btnYesContent.Text = value;
            }
        }

        public string NoButtonText
        {
            get
            {
                return this.btnNoContent.Text;
            }
            set
            {
                this.btnNoContent.Text = value;
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

            this.imgIcon.Source = icon.ToImageSource();
            this.imgIcon.Visibility = Visibility.Visible;
        }

        private void SetButtons(MessageBoxButton buttons)
        {
            List<Button> dialogButtons = new List<Button>() { this.btnYes, this.btnNo, this.btnOk, this.btnCancel };

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
                    this.btnOk.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnOk.Margin = new Thickness(0, 0, 3, 0);
                    this.btnCancel.Margin = new Thickness(3, 0, 0, 0);
                    this.btnCancel.IsDefault = true;
                    this.btnCancel.IsCancel = true;
                    break;

                case MessageBoxButton.YesNo:
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    this.btnYes.Margin = new Thickness(0, 0, 3, 0);
                    this.btnNo.Margin = new Thickness(3, 0, 0, 0);
                    this.btnNo.IsDefault = true;
                    this.btnNo.IsCancel = true;
                    break;

                case MessageBoxButton.YesNoCancel:
                    this.btnYes.Visibility = Visibility.Visible;
                    this.btnNo.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    this.btnYes.Margin = new Thickness(0, 0, 3, 0);
                    this.btnNo.Margin = new Thickness(3, 0, 3, 0);
                    this.btnCancel.Margin = new Thickness(3, 0, 0, 0);
                    this.btnCancel.IsDefault = true;
                    this.btnCancel.IsCancel = true;
                    break;

                default:
                    this.btnOk.Visibility = Visibility.Visible;
                    this.btnOk.IsDefault = true;
                    this.btnOk.IsCancel = true;
                    break;
            }
        }

        #endregion Methods

        #region EventHandlers

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.No;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion EventHandlers
    }
}