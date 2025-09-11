// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using Avalonia;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Helper Class for the Design Time Properties used by VS and Blend
	/// </summary>
	//public static class DesignTimeProperties
	//{
	//	#region IsHidden
		
	//	/// <summary>
	//	/// Getter for <see cref="IsHiddenProperty"/>
	//	/// </summary>
	//	public static bool GetIsHidden(AvaloniaObject obj)
	//	{
	//		return (bool)obj.GetValue(IsHiddenProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="IsHiddenProperty"/>
	//	/// </summary>
	//	public static void SetIsHidden(AvaloniaObject obj, bool value)
	//	{
	//		obj.SetValue(IsHiddenProperty, value);
	//	}
		
	//	/// <summary>
	//	/// Design-time IsHidden property
	//	/// </summary>
	//	public static readonly DependencyProperty IsHiddenProperty =
	//		DependencyProperty.RegisterAttached("IsHidden", typeof(bool), typeof(DesignTimeProperties), new PropertyMetadata(new PropertyChangedCallback(OnIsHiddenPropertyChanged)));

	//	static void OnIsHiddenPropertyChanged(AvaloniaObject d, DependencyPropertyChangedEventArgs e)
	//	{
	//		var dpd = DependencyPropertyDescriptor.FromProperty(UIElement.VisibilityProperty, d.GetType());
			
	//		if ((bool)e.NewValue) {
	//			EnsureHidden(d);
	//			dpd.AddValueChanged(d, OnVisibilityPropertyChanged);
	//		} else {
	//			dpd.RemoveValueChanged(d, OnVisibilityPropertyChanged);
	//			d.InvalidateProperty(UIElement.VisibilityProperty);
	//		}
	//	}

	//	static void OnVisibilityPropertyChanged(object sender, EventArgs e)
	//	{
	//		var d = sender as AvaloniaObject;
	//		if (d != null && GetIsHidden(d)) {
	//			EnsureHidden(d);
	//		}
	//	}
		
	//	static void EnsureHidden(AvaloniaObject d)
	//	{
	//		if (Visibility.Visible.Equals(d.GetValue(UIElement.VisibilityProperty))) {
	//			d.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Hidden);
	//		}
	//	}
		
	//	#endregion

	//	#region IsLocked

	//	/// <summary>
	//	/// Getter for <see cref="IsLockedProperty"/>
	//	/// </summary>
	//	public static bool GetIsLocked(AvaloniaObject obj)
	//	{
	//		return (bool)obj.GetValue(IsLockedProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="IsLockedProperty"/>
	//	/// </summary>
	//	public static void SetIsLocked(AvaloniaObject obj, bool value)
	//	{
	//		obj.SetValue(IsLockedProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time IsLocked property.
	//	/// </summary>
	//	public static readonly DependencyProperty IsLockedProperty =
	//		DependencyProperty.RegisterAttached("IsLocked", typeof(bool), typeof(DesignTimeProperties));

	//	#endregion

	//	#region DataContext
	//	/// <summary>
	//	/// Getter for <see cref="DataContextProperty"/>
	//	/// </summary>
	//	public static object GetDataContext(AvaloniaObject obj)
	//	{
	//		return (object)obj.GetValue(DataContextProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="DataContextProperty"/>
	//	/// </summary>
	//	public static void SetDataContext(AvaloniaObject obj, bool value)
	//	{
	//		obj.SetValue(DataContextProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time data context
	//	/// </summary>
	//	public static readonly DependencyProperty DataContextProperty =
	//		DependencyProperty.RegisterAttached("DataContext", typeof(object), typeof(DesignTimeProperties));

	//	#endregion

	//	#region DesignSource
	//	/// <summary>
	//	/// Getter for <see cref="DesignSourceProperty"/>
	//	/// </summary>
	//	public static object GetDesignSource(AvaloniaObject obj)
	//	{
	//		return (object)obj.GetValue(DesignSourceProperty);
	//	}
		
	//	/// <summary>
	//	/// Setter for <see cref="DesignSourceProperty"/>
	//	/// </summary>
	//	public static void SetDesignSource(AvaloniaObject obj, bool value)
	//	{
	//		obj.SetValue(DesignSourceProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time design source
	//	/// </summary>
	//	public static readonly DependencyProperty DesignSourceProperty =
	//		DependencyProperty.RegisterAttached("DesignSource", typeof(object), typeof(DesignTimeProperties));

	//	#endregion

	//	#region DesignWidth
	//	/// <summary>
	//	/// Getter for <see cref="DesignWidthProperty"/>
	//	/// </summary>
	//	public static double GetDesignWidth(AvaloniaObject obj)
	//	{
	//		return (double)obj.GetValue(DesignWidthProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="DesignWidthProperty"/>
	//	/// </summary>
	//	public static void SetDesignWidth(AvaloniaObject obj, double value)
	//	{
	//		obj.SetValue(DesignWidthProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time width
	//	/// </summary>
	//	public static readonly DependencyProperty DesignWidthProperty =
	//		DependencyProperty.RegisterAttached("DesignWidth", typeof(double), typeof(DesignTimeProperties));
	//	#endregion

	//	#region DesignHeight
	//	/// <summary>
	//	/// Getter for <see cref="DesignHeightProperty"/>
	//	/// </summary>
	//	public static double GetDesignHeight(AvaloniaObject obj)
	//	{
	//		return (double)obj.GetValue(DesignHeightProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="DesignHeightProperty"/>
	//	/// </summary>
	//	public static void SetDesignHeight(AvaloniaObject obj, double value)
	//	{
	//		obj.SetValue(DesignHeightProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time height
	//	/// </summary>
	//	public static readonly DependencyProperty DesignHeightProperty =
	//		DependencyProperty.RegisterAttached("DesignHeight", typeof(double), typeof(DesignTimeProperties));
	//	#endregion

	//	#region LayoutOverrides
	//	/// <summary>
	//	/// Getter for <see cref="LayoutOverridesProperty"/>
	//	/// </summary>
	//	public static string GetLayoutOverrides(AvaloniaObject obj)
	//	{
	//		return (string)obj.GetValue(LayoutOverridesProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="LayoutOverridesProperty"/>
	//	/// </summary>
	//	public static void SetLayoutOverrides(AvaloniaObject obj, string value)
	//	{
	//		obj.SetValue(LayoutOverridesProperty, value);
	//	}

	//	/// <summary>
	//	/// Layout-Overrides
	//	/// </summary>
	//	public static readonly DependencyProperty LayoutOverridesProperty =
	//		DependencyProperty.RegisterAttached("LayoutOverrides", typeof(string), typeof(DesignTimeProperties));
	//	#endregion

	//	#region LayoutRounding
	//	/// <summary>
	//	/// Getter for <see cref="LayoutRoundingProperty"/>
	//	/// </summary>
	//	public static bool GetLayoutRounding(AvaloniaObject obj)
	//	{
	//		return (bool)obj.GetValue(LayoutRoundingProperty);
	//	}

	//	/// <summary>
	//	/// Setter for <see cref="LayoutRoundingProperty"/>
	//	/// </summary>
	//	public static void SetLayoutRounding(AvaloniaObject obj, bool value)
	//	{
	//		obj.SetValue(LayoutRoundingProperty, value);
	//	}

	//	/// <summary>
	//	/// Design-time layout rounding
	//	/// </summary>
	//	public static readonly DependencyProperty LayoutRoundingProperty =
	//		DependencyProperty.RegisterAttached("LayoutRounding", typeof(bool), typeof(DesignTimeProperties));
	//	#endregion
	//}
}
