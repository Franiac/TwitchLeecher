using System;
using System.Windows;
using System.Windows.Controls;

namespace TwitchLeecher.Gui.Controls
{
    public class TlSpacedUniformGrid : Panel
    {
        #region Fields

        private double newItemWidth = 0;
        private double newItemHeight = 0;

        private int columnCount = 1;
        private int rowCount = 1;

        #endregion Fields

        #region Dependency Properties

        #region Spacing

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
                nameof(Spacing),
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

        #region ItemWidth

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
                nameof(ItemWidth),
                typeof(double),
                typeof(TlSpacedUniformGrid), new FrameworkPropertyMetadata(
                        defaultValue: 320.0,
                        flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        #endregion ItemWidth

        #endregion Dependency Properties

        #region Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            int elementCount = InternalChildren.Count;

            if (elementCount == 0)
            {
                return new Size(0, 0);
            }

            newItemWidth = 0;
            newItemHeight = 0;
            columnCount = 0;
            rowCount = 0;

            UIElementCollection elements = InternalChildren;

            double availableWidth = availableSize.Width;

            double spacing = Spacing;
            double itemWidth = ItemWidth;

            double widthSum = itemWidth;

            while (widthSum < availableWidth)
            {
                columnCount++;

                if (widthSum == itemWidth)
                {
                    widthSum += spacing;
                }

                widthSum += itemWidth;
            }

            columnCount = Math.Max(1, columnCount);

            rowCount = Math.Max(1, (int)Math.Ceiling(elementCount / (double)columnCount));

            double maxElementHeight = 0;

            if (elementCount < columnCount)
            {
                newItemWidth = itemWidth;
            }
            else
            {
                newItemWidth = (availableWidth - (columnCount > 1 ? (columnCount - 1) * spacing : 0)) / columnCount;
            }

            foreach (UIElement element in elements)
            {
                element.Measure(new Size(newItemWidth, double.PositiveInfinity));
                maxElementHeight = Math.Max(maxElementHeight, element.DesiredSize.Height);
            }

            newItemHeight = newItemWidth / (itemWidth / maxElementHeight);

            double newWidth = (newItemWidth * columnCount) + (columnCount > 1 ? (columnCount - 1) * spacing : 0);
            double newHeight = (newItemHeight * rowCount) + (rowCount > 1 ? (rowCount - 1) * spacing : 0);

            newWidth = double.IsPositiveInfinity(newWidth) ? int.MaxValue : newWidth;
            newHeight = double.IsPositiveInfinity(newHeight) ? int.MaxValue : newHeight;

            return new Size(newWidth, newHeight);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            int elementCount = InternalChildren.Count;

            if (elementCount == 0)
            {
                return arrangeSize;
            }

            UIElementCollection elements = InternalChildren;

            double spacing = Spacing;

            double curX = 0;
            double curY = 0;

            for (int i = 0; i < elementCount; i++)
            {
                if (i % columnCount == 0)
                {
                    curX = 0;

                    if (i > columnCount - 1)
                    {
                        curY += spacing + newItemHeight;
                    }
                }

                if (i % columnCount > 0)
                {
                    curX += spacing;
                }

                Rect rect = new Rect(curX, curY, newItemWidth, newItemHeight);

                elements[i].Arrange(rect);

                curX += newItemWidth;
            }

            return arrangeSize;
        }

        #endregion Methods
    }
}