// Copyright (c) 2019 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Security.Cryptography;
using Avalonia;
using Avalonia.Controls;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Helper Class for the Markup Compatibility Properties used by VS and Blend
	/// </summary>
	public class MarkupCompatibilityProperties : Control
    {
		#region Ignorable

		/// <summary>
		/// Getter for the <see cref="IgnorableProperty"/>
		/// </summary>
		public static string GetIgnorable(AvaloniaObject obj)
		{
			return (string)obj.GetValue(IgnorableProperty);
		}

		/// <summary>
		/// Setter for the <see cref="IgnorableProperty"/>
		/// </summary>
		public static void SetIgnorable(AvaloniaObject obj, string value)
		{
			obj.SetValue(IgnorableProperty, value);
		}

		/// <summary>
		/// Gets/Sets whether a XAML namespace may be ignored by the XAML parser.
		/// </summary>
		public static readonly AvaloniaProperty IgnorableProperty =
			AvaloniaProperty.RegisterAttached<Control, string>("Ignorable", typeof(MarkupCompatibilityProperties));
		
		#endregion
	}
}