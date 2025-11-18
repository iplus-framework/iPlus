// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using gip.ext.design.avui;
using Avalonia.Data.Converters;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace gip.ext.designer.avui.Converters
{
	public class IntFromEnumConverter : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly IntFromEnumConverter Instance = new IntFromEnumConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.ToObject(targetType, (int)value);
		}
	}

    public class StringFromEnumConverter : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly StringFromEnumConverter Instance = new StringFromEnumConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return Enum.Parse(targetType, parameter.ToString());
        }
    }

    public class BoolInverter : IValueConverter
    {
        public static readonly BoolInverter Instance = new BoolInverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class HiddenWhenFalse : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly HiddenWhenFalse Instance = new HiddenWhenFalse();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? true : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class CollapsedWhenFalse : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly CollapsedWhenFalse Instance = new CollapsedWhenFalse();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? true : false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class LevelConverter : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly LevelConverter Instance = new LevelConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new Thickness(2 + 14 * (int)value, 0, 0, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class CollapsedWhenZero : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly CollapsedWhenZero Instance = new CollapsedWhenZero();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || (int)value == 0) {
				return false;
			}			
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

    public class CollapsedWhenNotNull : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly CollapsedWhenNotNull Instance = new CollapsedWhenNotNull();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CollapsedWhenNull : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly CollapsedWhenNull Instance = new CollapsedWhenNull();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FalseWhenNull : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly FalseWhenNull Instance = new FalseWhenNull();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class BoldWhenTrue : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly BoldWhenTrue Instance = new BoldWhenTrue();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? FontWeight.Bold : FontWeight.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	// Boxed int throw exception without converter (wpf bug?)
	public class DummyConverter : IValueConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
		public static readonly DummyConverter Instance = new DummyConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}

    public class ControlToRealWidthConverter : IMultiValueConverter
    {
        public static readonly ControlToRealWidthConverter Instance = new ControlToRealWidthConverter();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return PlacementOperation.GetRealElementSize((Control)values[0]).Width;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ControlToRealHeightConverter : IMultiValueConverter
    {
        public static readonly ControlToRealHeightConverter Instance = new ControlToRealHeightConverter();

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            return PlacementOperation.GetRealElementSize((Control)values[0]).Height;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FormatDoubleConverter : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly FormatDoubleConverter Instance=new FormatDoubleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Round((double)value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
    }

    public class DoubleOffsetConverter : IValueConverter
    {
        public double Offset { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + Offset;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - Offset;
        }
    }

    public class BlackWhenTrue : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly BlackWhenTrue Instance = new BlackWhenTrue();

        private Brush black;

        public BlackWhenTrue()
        {
            black = new SolidColorBrush(Colors.Black);
            //black.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? black : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumBoolean : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly EnumBoolean Instance = new EnumBoolean();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return AvaloniaProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }

    public class EnumVisibility : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly EnumVisibility Instance = new EnumVisibility();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return AvaloniaProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }

    public class EnumCollapsed : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly EnumCollapsed Instance = new EnumCollapsed();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return AvaloniaProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value) ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return AvaloniaProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }

    public class InvertedZoomConverter : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly InvertedZoomConverter Instance = new InvertedZoomConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.0 / ((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.0 / ((double)value);
        }
    }

    public sealed class PointConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            // Ensure all bindings are provided and attached to correct target type
            if (values == null || values.Count < 1 || !targetType.IsAssignableFrom(typeof(Point)))
                throw new NotSupportedException();
            if (values[0] is Point p)
                return new Point(p.X, p.Y);

            return new Point((double)values[0], (double)values[1]);
        }
    }
}
