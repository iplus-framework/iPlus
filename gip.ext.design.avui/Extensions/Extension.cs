// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using System;
using System.Diagnostics;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Base class for all Extensions.
	/// </summary>
	/// <remarks>
	/// The class design in the gip.ext.design.avui.Extensions namespace was made to match that of Cider
	/// as described in the blog posts:
	/// http://blogs.msdn.com/jnak/archive/2006/04/24/580393.aspx
	/// http://blogs.msdn.com/jnak/archive/2006/08/04/687166.aspx
	/// </remarks>
	public abstract class Extension
	{
        /// <summary>
        /// Gets the value of the <see cref="DisabledExtensionsProperty"/> attached property for an object.
        /// </summary>
        /// <param name="obj">The object from which the property value is read.</param>
        /// <returns>The object's <see cref="DisabledExtensionsProperty"/> property value.</returns>
        public static string GetDisabledExtensions(AvaloniaObject obj)
        {
            if (obj != null)
            {
                return (string)obj.GetValue(DisabledExtensionsProperty);
            }
            return null;
        }

        /// <summary>
        /// Sets the value of the <see cref="DisabledExtensionsProperty"/> attached property for an object. 
        /// </summary>
        /// <param name="obj">The object to which the attached property is written.</param>
        /// <param name="value">The value to set.</param>
        public static void SetDisabledExtensions(AvaloniaObject obj, string value)
        {
            obj.SetValue(DisabledExtensionsProperty, value);
        }

        /// <summary>
        /// Gets or sets a semicolon-separated list with extension names that is disabled for a component's view.
        /// </summary>
        public static readonly AvaloniaProperty DisabledExtensionsProperty =
            AvaloniaProperty.RegisterAttached<Control, string>("DisabledExtensions", typeof(Extension));



        /// <summary>
        /// Gets the value of the <see cref="DisableMouseOverExtensionsProperty"/> attached property for an object.
        /// </summary>
        /// <param name="obj">The object from which the property value is read.</param>
        /// <returns>The object's <see cref="DisableMouseOverExtensionsProperty"/> property value.</returns>
        public static bool GetDisableMouseOverExtensions(AvaloniaObject obj)
        {
            return (bool)obj.GetValue(DisableMouseOverExtensionsProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="DisableMouseOverExtensionsProperty"/> attached property for an object. 
        /// </summary>
        /// <param name="obj">The object to which the attached property is written.</param>
        /// <param name="value">The value to set.</param>
        public static void SetDisableMouseOverExtensions(AvaloniaObject obj, bool value)
        {
            obj.SetValue(DisableMouseOverExtensionsProperty, value);
        }

        /// <summary>
        /// Disables the mouse over Extension for this Element
        /// </summary>
        public static readonly AvaloniaProperty DisableMouseOverExtensionsProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("DisableMouseOverExtensions", typeof(Extension));

    }
}
