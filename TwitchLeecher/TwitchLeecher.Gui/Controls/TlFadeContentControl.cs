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
    public class TlFadeContentControl : ContentControl
    {
        #region Fields

        private Shape _paintArea;
        private ContentControl _mainContent;

        private readonly DoubleAnimation _fadeInAnim;
        private readonly DoubleAnimation _fadeOutAnim;

        #endregion Fields

        #region Constructors

        public TlFadeContentControl()
        {
            _fadeInAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            _fadeOutAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));

            _fadeOutAnim.Completed += FadeOutAnim_Completed;
        }

        #endregion Constructors

        #region Methods

        public override void OnApplyTemplate()
        {
            _paintArea = Template.FindName("Tpl_Part_PaintArea", this) as Shape;
            _mainContent = Template.FindName("Tpl_Part_MainContent", this) as ContentControl;

            base.OnApplyTemplate();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (_paintArea != null && _mainContent != null)
            {
                _paintArea.Fill = CreateBrushFromVisual(_mainContent);
                BeginAnimateContentReplacement();
            }

            base.OnContentChanged(oldContent, newContent);
        }

        private void BeginAnimateContentReplacement()
        {
            _paintArea.Visibility = Visibility.Visible;
            _mainContent.Visibility = Visibility.Hidden;
            _paintArea.BeginAnimation(OpacityProperty, _fadeOutAnim);
        }

        private Brush CreateBrushFromVisual(Visual visual)
        {
            if (visual == null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            RenderTargetBitmap target = new RenderTargetBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);

            ImageBrush brush = new ImageBrush(target);
            brush.Freeze();

            return brush;
        }

        #endregion Methods

        #region EventHandlers

        private void FadeOutAnim_Completed(object sender, EventArgs e)
        {
            _paintArea.Visibility = Visibility.Hidden;
            _mainContent.Visibility = Visibility.Visible;
            _mainContent.BeginAnimation(OpacityProperty, _fadeInAnim);
        }

        #endregion EventHandlers
    }
}