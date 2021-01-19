using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gip.core.reporthandler.Flowdoc
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
            bool valueBool = false;
            if (value is bool)
                valueBool = (bool)value;
            else if (value is string)
                bool.TryParse(value as string, out valueBool);

            System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image();
            if (valueBool)
                wpfImage.Source = new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine;component/Images/true.png", UriKind.Absolute));
            else
                wpfImage.Source = new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine;component/Images/false.png", UriKind.Absolute));
            wpfImage.MaxHeight = MaxHeight > 0.1 ? MaxHeight : 12;
            wpfImage.MaxWidth = MaxWidth > 0.1 ? MaxWidth : 12;
            this.Child = wpfImage;
        }
    }
}
