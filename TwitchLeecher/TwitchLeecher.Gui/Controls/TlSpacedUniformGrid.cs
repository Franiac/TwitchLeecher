using System;
using System.Windows;
using System.Windows.Controls;

namespace TwitchLeecher.Gui.Controls
{
    public class TlSpacedUniformGrid : Panel
    {
        #region Dependency Properties

        #region Spacing

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
                "Spacing",
                typeof(double),
                typeof(TlSpacedUniformGrid), new FrameworkPropertyMetadata(
                        defaultValue: 10.0,
                        flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double Spacing
        {
            get { return (double)GetValue(SpacingProperty); }
            set { SetValue(SpacingProperty, value); }
        }

        #endregion Spacing

        #endregion Dependency Properties

        #region Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            double maxElementWidth = 0;
            double maxElementHeight = 0;

            foreach (UIElement element in InternalChildren)
            {
                element.Measure(availableSize);

                double desiredWidth = element.DesiredSize.Width;
                double desiredHeight = element.DesiredSize.Height;

                if (desiredWidth > maxElementWidth)
                {
                    maxElementWidth = desiredWidth;
                }

                if (desiredHeight > maxElementHeight)
                {
                    maxElementHeight = desiredHeight;
                }
            }

            double availableWidth = availableSize.Width;

            double spacing = Spacing;

            int childCount = InternalChildren.Count;

            int columnCount = 1;

            if (maxElementWidth > 0)
            {
                columnCount = (int)Math.Floor(availableWidth / maxElementWidth);
            }

            if ((columnCount * maxElementWidth + (columnCount - 1) * spacing) > availableWidth)
            {
                columnCount = Math.Max(1, columnCount - 1);
            }

            columnCount = Math.Max(1, Math.Min(childCount, columnCount));

            int rowCount = (int)Math.Ceiling(childCount / (double)columnCount);

            rowCount = Math.Max(1, rowCount);

            double newWidth = maxElementWidth * columnCount + (columnCount > 1 ? (columnCount - 1) * spacing : 0);
            double newHeight = maxElementHeight * rowCount + (rowCount > 1 ? (rowCount - 1) * spacing : 0);

            newWidth = double.IsPositiveInfinity(newWidth) ? int.MaxValue : newWidth;
            newHeight = double.IsPositiveInfinity(newHeight) ? int.MaxValue : newHeight;

            return new Size(newWidth, newHeight);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            double maxElementWidth = 0;
            double maxElementHeight = 0;

            foreach (UIElement element in InternalChildren)
            {
                double desiredWidth = element.DesiredSize.Width;
                double desiredHeight = element.DesiredSize.Height;

                if (desiredWidth > maxElementWidth)
                {
                    maxElementWidth = desiredWidth;
                }

                if (desiredHeight > maxElementHeight)
                {
                    maxElementHeight = desiredHeight;
                }
            }

            double arrangeWidth = arrangeSize.Width;
            double arrangeHeight = arrangeSize.Height;

            double spacing = Spacing;

            int childCount = InternalChildren.Count;

            int columnCount = 1;

            if (maxElementWidth > 0)
            {
                columnCount = (int)Math.Floor(arrangeWidth / maxElementWidth);
            }

            if ((columnCount * maxElementWidth + (columnCount - 1) * spacing) > arrangeWidth)
            {
                columnCount = Math.Max(1, columnCount - 1);
            }

            columnCount = Math.Max(1, Math.Min(childCount, columnCount));

            int rowCount = (int)Math.Ceiling(childCount / (double)columnCount);

            rowCount = Math.Max(1, rowCount);

            double newChildWidth = (arrangeWidth - (columnCount > 1 ? (columnCount - 1) * spacing : 0)) / columnCount;

            double curX = 0;
            double curY = 0;

            for (int i = 0; i < childCount; i++)
            {
                if (i % columnCount == 0)
                {
                    curX = 0;

                    if (i > columnCount - 1)
                    {
                        curY += spacing + maxElementHeight;
                    }
                }

                if (i % columnCount > 0)
                {
                    curX += spacing;
                }

                Rect rect = new Rect(curX, curY, newChildWidth, maxElementHeight);

                InternalChildren[i].Arrange(rect);

                curX += newChildWidth;
            }

            return arrangeSize;
        }

        #endregion Methods
    }
}