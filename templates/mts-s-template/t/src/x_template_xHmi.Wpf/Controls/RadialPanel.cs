using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace x_template_xHmi.Wpf.Controls //toto zmen do aktualneho namespace-u
{
    public class RadialPanel : Panel
    {
        // =====================================================
        // RADIUS
        // =====================================================

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(
                nameof(Radius),
                typeof(double),
                typeof(RadialPanel),
                new FrameworkPropertyMetadata(
                    250.0,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        // =====================================================
        // CLOCKWISE
        // =====================================================

        public static readonly DependencyProperty ClockwiseProperty =
            DependencyProperty.Register(
                nameof(Clockwise),
                typeof(bool),
                typeof(RadialPanel),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool Clockwise
        {
            get => (bool)GetValue(ClockwiseProperty);
            set => SetValue(ClockwiseProperty, value);
        }

        // =====================================================
        // ROTATION
        // =====================================================

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register(
                nameof(RotationOffset),
                typeof(double),
                typeof(RadialPanel),
                new FrameworkPropertyMetadata(
                    -90.0,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public double RotationOffset
        {
            get => (double)GetValue(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        // =====================================================
        // IS CENTER
        // =====================================================

        public static readonly DependencyProperty IsCenterProperty =
            DependencyProperty.RegisterAttached(
                "IsCenter",
                typeof(bool),
                typeof(RadialPanel),
                new FrameworkPropertyMetadata(false));

        public static void SetIsCenter(
            UIElement element,
            bool value)
        {
            element.SetValue(IsCenterProperty, value);
        }

        public static bool GetIsCenter(
            UIElement element)
        {
            return (bool)element.GetValue(IsCenterProperty);
        }

        // =====================================================
        // MEASURE
        // =====================================================

        protected override Size MeasureOverride(Size availableSize)
        {
            double maxChildWidth = 0;
            double maxChildHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);

                // center content neovplyvnuje velkost panelu
                if (GetIsCenter(child))
                    continue;

                maxChildWidth = Math.Max(
                    maxChildWidth,
                    child.DesiredSize.Width);

                maxChildHeight = Math.Max(
                    maxChildHeight,
                    child.DesiredSize.Height);
            }

            double desiredWidth =
                Radius * 2 + maxChildWidth * 2;

            double desiredHeight =
                Radius * 2 + maxChildHeight * 2;

            return new Size(
                desiredWidth,
                desiredHeight);
        }

        // =====================================================
        // ARRANGE
        // =====================================================

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count == 0)
                return finalSize;

            Point center = new Point(
                finalSize.Width / 2,
                finalSize.Height / 2);

            List<UIElement> revolvingChildren =
                new List<UIElement>();

            // =================================================
            // CENTER ELEMENT
            // =================================================

            foreach (UIElement child in InternalChildren)
            {
                if (GetIsCenter(child))
                {
                    Size desired = child.DesiredSize;

                    double x =
                        center.X - desired.Width / 2;

                    double y =
                        center.Y - desired.Height / 2;

                    child.Arrange(
                        new Rect(
                            new Point(x, y),
                            desired));
                }
                else
                {
                    revolvingChildren.Add(child);
                }
            }

            // =================================================
            // REVOLVING ELEMENTS
            // =================================================

            int count = revolvingChildren.Count;

            if (count == 0)
                return finalSize;

            double rotationRad =
                RotationOffset * Math.PI / 180.0;

            for (int i = 0; i < count; i++)
            {
                UIElement child = revolvingChildren[i];

                double step =
                    (2 * Math.PI * i / count);

                if (!Clockwise)
                {
                    step *= -1;
                }

                double angle = step + rotationRad;

                Size desired = child.DesiredSize;

                // radius znamena vzdialenost
                // OD STREDU po OKRAJ elementu
                double effectiveRadius =
                    Radius +
                    Math.Max(
                        desired.Width,
                        desired.Height) / 2;

                double x =
                    center.X +
                    effectiveRadius * Math.Cos(angle) -
                    desired.Width / 2;

                double y =
                    center.Y +
                    effectiveRadius * Math.Sin(angle) -
                    desired.Height / 2;

                child.Arrange(
                    new Rect(
                        new Point(x, y),
                        desired));
            }

            return finalSize;
        }
    }
}
