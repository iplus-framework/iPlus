using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace gip.ext.design.avui.UIExtensions
{
 
    /// <summary>
    /// Extension methods for brush classes.
    /// </summary>
    public static class BrushExtensions
    {
        /// <summary>
        /// Converts a collection of gradient stops to a new GradientStops instance.
        /// </summary>
        /// <param name="gradientStops">The gradient stops to convert.</param>
        /// <returns>A new GradientStops instance containing the gradient stops.</returns>
        private static GradientStops ToGradientStops(this IReadOnlyList<IGradientStop> gradientStops)
        {
            var result = new GradientStops();
            foreach (var stop in gradientStops)
            {
                result.Add(stop as GradientStop);
            }
            return result;
        }

        /// <summary>
        /// Converts a brush to an immutable brush.
        /// </summary>
        /// <param name="brush">The brush.</param>
        /// <returns>
        /// The result of calling <see cref="IMutableBrush.ToImmutable"/> if the brush is mutable,
        /// otherwise <paramref name="brush"/>.
        /// </returns>
        public static IBrush ToImmutable(this IBrush brush)
        {
            _ = brush ?? throw new ArgumentNullException(nameof(brush));

            return (brush as IBrush)?.ToImmutable() ?? (IImmutableBrush)brush;
        }

        /// <summary>
        /// Creates a clone of the brush by converting it to an immutable brush and back to a mutable brush if needed.
        /// </summary>
        /// <param name="brush">The brush to clone.</param>
        /// <returns>A cloned instance of the brush as <see cref="IBrush"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="brush"/> is null.</exception>
        /// <exception cref="NotSupportedException">Thrown when the brush type is not supported for cloning.</exception>
        public static IBrush Clone(this IBrush brush)
        {
            _ = brush ?? throw new ArgumentNullException(nameof(brush));

            return brush switch
            {
                // Handle mutable brushes by creating new instances with the same properties
                SolidColorBrush solidColor => new SolidColorBrush(solidColor.Color, solidColor.Opacity)
                {
                    Transform = solidColor.Transform,
                    TransformOrigin = solidColor.TransformOrigin
                },

                LinearGradientBrush linearGradient => new LinearGradientBrush
                {
                    StartPoint = linearGradient.StartPoint,
                    EndPoint = linearGradient.EndPoint,
                    GradientStops = linearGradient.GradientStops.ToGradientStops(),
                    SpreadMethod = linearGradient.SpreadMethod,
                    Opacity = linearGradient.Opacity,
                    Transform = linearGradient.Transform,
                    TransformOrigin = linearGradient.TransformOrigin
                },

                RadialGradientBrush radialGradient => new RadialGradientBrush
                {
                    Center = radialGradient.Center,
                    GradientOrigin = radialGradient.GradientOrigin,
                    RadiusX = radialGradient.RadiusX,
                    RadiusY = radialGradient.RadiusY,
                    GradientStops = radialGradient.GradientStops.ToGradientStops(),
                    SpreadMethod = radialGradient.SpreadMethod,
                    Opacity = radialGradient.Opacity,
                    Transform = radialGradient.Transform,
                    TransformOrigin = radialGradient.TransformOrigin
                },

                ConicGradientBrush conicGradient => new ConicGradientBrush
                {
                    Center = conicGradient.Center,
                    Angle = conicGradient.Angle,
                    GradientStops = conicGradient.GradientStops.ToGradientStops(),
                    SpreadMethod = conicGradient.SpreadMethod,
                    Opacity = conicGradient.Opacity,
                    Transform = conicGradient.Transform,
                    TransformOrigin = conicGradient.TransformOrigin
                },

                ImageBrush imageBrush => new ImageBrush(imageBrush.Source)
                {
                    AlignmentX = imageBrush.AlignmentX,
                    AlignmentY = imageBrush.AlignmentY,
                    DestinationRect = imageBrush.DestinationRect,
                    SourceRect = imageBrush.SourceRect,
                    Stretch = imageBrush.Stretch,
                    TileMode = imageBrush.TileMode,
                    Opacity = imageBrush.Opacity,
                    Transform = imageBrush.Transform,
                    TransformOrigin = imageBrush.TransformOrigin
                },

                VisualBrush visualBrush => new VisualBrush(visualBrush.Visual)
                {
                    AlignmentX = visualBrush.AlignmentX,
                    AlignmentY = visualBrush.AlignmentY,
                    DestinationRect = visualBrush.DestinationRect,
                    SourceRect = visualBrush.SourceRect,
                    Stretch = visualBrush.Stretch,
                    TileMode = visualBrush.TileMode,
                    Opacity = visualBrush.Opacity,
                    Transform = visualBrush.Transform,
                    TransformOrigin = visualBrush.TransformOrigin
                },

                DrawingBrush drawingBrush => new DrawingBrush(drawingBrush.Drawing)
                {
                    AlignmentX = drawingBrush.AlignmentX,
                    AlignmentY = drawingBrush.AlignmentY,
                    DestinationRect = drawingBrush.DestinationRect,
                    SourceRect = drawingBrush.SourceRect,
                    Stretch = drawingBrush.Stretch,
                    TileMode = drawingBrush.TileMode,
                    Opacity = drawingBrush.Opacity,
                    Transform = drawingBrush.Transform,
                    TransformOrigin = drawingBrush.TransformOrigin
                },

                // Handle immutable brushes by creating new mutable instances
                ImmutableSolidColorBrush immutableSolid => new SolidColorBrush(immutableSolid.Color, immutableSolid.Opacity)
                {
                    Transform = immutableSolid.Transform,
                    TransformOrigin = immutableSolid.TransformOrigin
                },

                ImmutableLinearGradientBrush immutableLinear => new LinearGradientBrush
                {
                    StartPoint = immutableLinear.StartPoint,
                    EndPoint = immutableLinear.EndPoint,
                    GradientStops = immutableLinear.GradientStops.ToGradientStops(),
                    SpreadMethod = immutableLinear.SpreadMethod,
                    Opacity = immutableLinear.Opacity,
                    Transform = immutableLinear.Transform,
                    TransformOrigin = immutableLinear.TransformOrigin
                },

                ImmutableRadialGradientBrush immutableRadial => new RadialGradientBrush
                {
                    Center = immutableRadial.Center,
                    GradientOrigin = immutableRadial.GradientOrigin,
                    RadiusX = immutableRadial.RadiusX,
                    RadiusY = immutableRadial.RadiusY,
                    GradientStops = immutableRadial.GradientStops.ToGradientStops(),
                    SpreadMethod = immutableRadial.SpreadMethod,
                    Opacity = immutableRadial.Opacity,
                    Transform = immutableRadial.Transform,
                    TransformOrigin = immutableRadial.TransformOrigin
                },

                ImmutableConicGradientBrush immutableConic => new ConicGradientBrush
                {
                    Center = immutableConic.Center,
                    Angle = immutableConic.Angle,
                    GradientStops = immutableConic.GradientStops.ToGradientStops(),
                    SpreadMethod = immutableConic.SpreadMethod,
                    Opacity = immutableConic.Opacity,
                    Transform = immutableConic.Transform,
                    TransformOrigin = immutableConic.TransformOrigin
                },

                //ImmutableImageBrush immutableImage => new ImageBrush(immutableImage.Source)
                //{
                //    AlignmentX = immutableImage.AlignmentX,
                //    AlignmentY = immutableImage.AlignmentY,
                //    DestinationRect = immutableImage.DestinationRect,
                //    SourceRect = immutableImage.SourceRect,
                //    Stretch = immutableImage.Stretch,
                //    TileMode = immutableImage.TileMode,
                //    Opacity = immutableImage.Opacity,
                //    Transform = immutableImage.Transform,
                //    TransformOrigin = immutableImage.TransformOrigin
                //},

                // Fallback for any other IBrush implementation - try to use ToImmutable if available
                IBrush mutableBrush => mutableBrush.ToImmutable(),

                // For already immutable brushes that don't have a direct mutable equivalent, return as-is
                _ => brush
            };
        }

        /// <summary>
        /// Converts a dash style to an immutable dash style.
        /// </summary>
        /// <param name="style">The dash style.</param>
        /// <returns>
        /// The result of calling <see cref="DashStyle.ToImmutable"/> if the style is mutable,
        /// otherwise <paramref name="style"/>.
        /// </returns>
        public static ImmutableDashStyle ToImmutable(this IDashStyle style)
        {
            _ = style ?? throw new ArgumentNullException(nameof(style));

            return style as ImmutableDashStyle ?? ((DashStyle)style).ToImmutable();
        }

        /// <summary>
        /// Converts a pen to an immutable pen.
        /// </summary>
        /// <param name="pen">The pen.</param>
        /// <returns>
        /// The result of calling <see cref="Pen.ToImmutable"/> if the brush is mutable,
        /// otherwise <paramref name="pen"/>.
        /// </returns>
        public static ImmutablePen ToImmutable(this IPen pen)
        {
            _ = pen ?? throw new ArgumentNullException(nameof(pen));

            return pen as ImmutablePen ?? ((Pen)pen).ToImmutable();
        }
    }

}
