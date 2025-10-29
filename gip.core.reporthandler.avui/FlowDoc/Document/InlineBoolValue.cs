// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia.Media.Imaging;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Represents the control for boolean value in reports.
    /// </summary>
    /// <summary>
    /// Stellt das Steuerelement für boolesche Werte in Berichten dar.
    /// </summary>
    public class InlineBoolValue : InlineUIValueBase
    {
        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public override object Value
        {
            get
            {
                return (object)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                RenderBoolValue(value);
            }
        }

        private void RenderBoolValue(object value)
        {
            if (value == null)
                return;
            //bool valueBool = false;
            //if (value is bool)
            //    valueBool = (bool)value;
            //else if (value is string)
            //    bool.TryParse(value as string, out valueBool);

            //Image wpfImage = new Image();
            //if (valueBool)
            //    wpfImage.Source = new Bitmap(new Uri("pack://application:,,,/gip.core.layoutengine.avui;component/Images/true.png", UriKind.Absolute));
            //else
            //    wpfImage.Source = new Bitmap(new Uri("pack://application:,,,/gip.core.layoutengine.avui;component/Images/false.png", UriKind.Absolute));
            //wpfImage.MaxHeight = MaxHeight > 0.1 ? MaxHeight : 12;
            //wpfImage.MaxWidth = MaxWidth > 0.1 ? MaxWidth : 12;
            //this.Child = wpfImage;
        }
    }
}
