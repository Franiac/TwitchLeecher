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
     TemplatePart(Name = "Tpl_Part_MainContent", Type = typeof(ContentPresenter))]
    public class AnimatedContentControl : ContentControl
    {
        #region Fields

        private Shape paintArea;
        private ContentPresenter mainContent;

        #endregion Fields

        #region Methods

        public override void OnApplyTemplate()
        {
            paintArea = Template.FindName("Tpl_Part_PaintArea", this) as Shape;
            mainContent = Template.FindName("Tpl_Part_MainContent", this) as ContentPresenter;

            base.OnApplyTemplate();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (paintArea != null && mainContent != null)
            {
                paintArea.Fill = this.CreateBrushFromVisual(mainContent);
                BeginAnimateContentReplacement();
            }

            base.OnContentChanged(oldContent, newContent);
        }

        private void BeginAnimateContentReplacement()
        {
            TranslateTransform newContentTransform = new TranslateTransform();
            TranslateTransform oldContentTransform = new TranslateTransform();

            paintArea.RenderTransform = oldContentTransform;
            mainContent.RenderTransform = newContentTransform;

            paintArea.Visibility = Visibility.Visible;

            newContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(this.ActualWidth, 0));
            oldContentTransform.BeginAnimation(TranslateTransform.XProperty, CreateAnimation(0, -this.ActualWidth, (s, e) => paintArea.Visibility = Visibility.Hidden));
        }

        private AnimationTimeline CreateAnimation(double from, double to, EventHandler whenDone = null)
        {
            IEasingFunction ease = new CubicEase()
            {
                EasingMode = EasingMode.EaseOut
            };

            Duration duration = new Duration(TimeSpan.FromSeconds(0.25));

            DoubleAnimation doubleAnim = new DoubleAnimation(from, to, duration)
            {
                EasingFunction = ease
            };

            if (whenDone != null)
            {
                doubleAnim.Completed += whenDone;
            }

            doubleAnim.Freeze();

            return doubleAnim;
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
    }
}