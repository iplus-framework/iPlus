// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.designer.avui.Controls
{
    public class EnterTextBox : TextBox
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                if (b != null)
                {
                    b.UpdateSource();
                }
                SelectAll();
            }
            else if (e.Key == Key.Escape)
            {
                var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                if (b != null)
                {
                    b.UpdateTarget();
                }
            }
        }
    }
}
