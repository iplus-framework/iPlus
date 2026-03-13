// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Markup.Xaml.Converters;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// Registry of Avalonia-specific type converters that support both serialization (ConvertTo string)
    /// and deserialization (ConvertFrom string).
    ///
    /// Mirrors the type-to-converter mapping maintained by Avalonia's internals
    /// (AvaloniaXamlIlLanguage.AttributeResolver), but each converter here is extended to also
    /// handle the reverse direction so that the designer's XAML serializer
    /// (XamlDocument.CreatePropertyValue) and the designer's XAML parser
    /// (XamlPropertyInfo.GetCustomTypeConverter) can round-trip values correctly.
    ///
    /// To add support for additional types, append a new entry in the static constructor below.
    /// </summary>
    internal static class AvaloniaConverterRegistry
    {
        // Ordered list of (predicate, bidirectional-converter) pairs.
        private static readonly List<(Func<Type, bool> Match, TypeConverter Converter)> _entries;

        static AvaloniaConverterRegistry()
        {
            _entries = new()
            {
                // Avalonia.Media.Color -> "#AARRGGBB"
                (t => t == typeof(Avalonia.Media.Color),
                    ColorBidirectionalConverter.Instance),

                // Avalonia.RelativePoint -> "x%, y%" or "x, y"
                (t => t == typeof(RelativePoint),
                    RelativePointBidirectionalConverter.Instance),

                // Avalonia.RelativeScalar -> "100%" or "12.5"
                (t => t == typeof(RelativeScalar),
                    RelativeScalarBidirectionalConverter.Instance),

                // IList<Point> / List<Point> / Avalonia.Points (AvaloniaList<Point>) → "x1,y1 x2,y2 ..."
                (t => typeof(IList<Point>).IsAssignableFrom(t),
                    PointsListBidirectionalConverter.Instance),
            };
        }

        /// <summary>
        /// Returns a bidirectional <see cref="TypeConverter"/> for <paramref name="type"/> when one
        /// is registered; otherwise <c>null</c>.
        /// </summary>
        public static TypeConverter GetConverter(Type type)
        {
            foreach (var (match, converter) in _entries)
            {
                if (match(type))
                    return converter;
            }
            return null;
        }

        // ----- Private bidirectional converter implementations -----

        /// <summary>
        /// Bidirectional converter for Avalonia Color values.
        /// Uses #AARRGGBB to keep round-trippable, explicit alpha output.
        /// </summary>
        private sealed class ColorBidirectionalConverter : TypeConverter
        {
            public static readonly ColorBidirectionalConverter Instance = new();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string s)
                    return Avalonia.Media.Color.Parse(s);

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destinationType)
            {
                if (destinationType == typeof(string) && value is Avalonia.Media.Color color)
                {
                    return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Bidirectional converter for RelativePoint values.
        /// </summary>
        private sealed class RelativePointBidirectionalConverter : TypeConverter
        {
            public static readonly RelativePointBidirectionalConverter Instance = new();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string s)
                    return RelativePoint.Parse(s);

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destinationType)
            {
                if (destinationType == typeof(string) && value is RelativePoint p)
                    return p.ToString();

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Bidirectional converter for RelativeScalar values.
        /// </summary>
        private sealed class RelativeScalarBidirectionalConverter : TypeConverter
        {
            public static readonly RelativeScalarBidirectionalConverter Instance = new();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
                => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string s)
                    return RelativeScalar.Parse(s);

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destinationType)
            {
                if (destinationType == typeof(string) && value is RelativeScalar scalar)
                    return scalar.ToString();

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        /// <summary>
        /// Extends Avalonia's <see cref="PointsListTypeConverter"/> (which only parses) with
        /// serialization support.
        /// Serialization format: "x1,y1 x2,y2 ..." using invariant culture, space-separated pairs
        /// and a comma between X and Y within each pair — identical to WPF's Polyline Points format
        /// and what Avalonia's own XAML compiler expects to round-trip.
        /// </summary>
        private sealed class PointsListBidirectionalConverter : PointsListTypeConverter
        {
            public static readonly PointsListBidirectionalConverter Instance = new();

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

            public override object ConvertTo(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destinationType)
            {
                if (destinationType == typeof(string) && value is IList<Point> points)
                {
                    if (points.Count == 0)
                        return string.Empty;

                    var sb = new StringBuilder(points.Count * 14);
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (i > 0) sb.Append(' ');
                        sb.Append(points[i].X.ToString(CultureInfo.InvariantCulture));
                        sb.Append(',');
                        sb.Append(points[i].Y.ToString(CultureInfo.InvariantCulture));
                    }
                    return sb.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
