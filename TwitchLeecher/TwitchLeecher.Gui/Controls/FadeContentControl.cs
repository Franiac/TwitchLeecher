using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwitchLeecher.Gui.Controls
{
    [TemplatePart(Name = "Tpl_Part_PaintArea", Type = typeof(Shape)),
     TemplatePart(Name = "Tpl_Part_MainContent", Type = typeof(ContentControl))]
    public class FadeContentControl : ContentControl
    {
        #region Fields

        private Shape paintArea;
        private ContentControl mainContent;

        private DoubleAnimation fadeInAnim;
        private DoubleAnimation fadeOutAnim;

        #endregion Fields

        #region Constructors

        public FadeContentControl()
        {
            this.fadeInAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            this.fadeOutAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));

            this.fadeOutAnim.Completed += FadeOutAnim_Completed;
        }

        #endregion Constructors

        #region Methods

        public override void OnApplyTemplate()
        {
            this.paintArea = Template.FindName("Tpl_Part_PaintArea", this) as Shape;
            this.mainContent = Template.FindName("Tpl_Part_MainContent", this) as ContentControl;

            base.OnApplyTemplate();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (this.paintArea != null && this.mainContent != null)
            {
                this.paintArea.Fill = this.CreateBrushFromVisual(this.mainContent);
                this.BeginAnimateContentReplacement();
            }

            base.OnContentChanged(oldContent, newContent);
        }

        private void BeginAnimateContentReplacement()
        {
            this.paintArea.Visibility = Visibility.Visible;
            this.mainContent.Visibility = Visibility.Hidden;
            this.paintArea.BeginAnimation(OpacityProperty, this.fadeOutAnim);
        }

        private Brush CreateBrushFromVisual(Visual visual)
        {
            if (visual == null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            RenderTargetBitmap target = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);

            ImageBrush brush = new ImageBrush(target);
            brush.Freeze();

            return brush;
        }

        #endregion Methods

        #region EventHandlers

        private void FadeOutAnim_Completed(object sender, EventArgs e)
        {
            this.paintArea.Visibility = Visibility.Hidden;
            this.mainContent.Visibility = Visibility.Visible;
            this.mainContent.BeginAnimation(OpacityProperty, this.fadeInAnim);
        }

        #endregion EventHandlers
    }
}