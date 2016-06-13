using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TwitchLeecher.Gui.Views
{
    public partial class NotificationStrip : UserControl
    {
        public NotificationStrip()
        {
            InitializeComponent();
        }

        public void ShowNotification(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                this.txtNotification.Text = text;
                Storyboard storyBoard = (Storyboard)this.FindResource("NotificationStoryboard");
                this.BeginStoryboard(storyBoard);
            }
        }
    }
}