// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    public class InlineBoolValue : Image, IInlinePropertyValue
    {
        public InlineBoolValue() : base()
        {

        }

        public string VBContent
        {
            get { return ( string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof( string), typeof(InlineBoolValue));

        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(InlineBoolValue));

        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(InlineBoolValue), new PropertyMetadata(false, new PropertyChangedCallback(OnValueChanged)));

        static void OnValueChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs args)
        {
            InlineBoolValue inlineBoolValue = depObject as InlineBoolValue;
            if(args.NewValue is bool && (bool)args.NewValue)
                inlineBoolValue.Source = new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine;component/Images/true.png", UriKind.Absolute));
            else if(args.NewValue is bool)
                inlineBoolValue.Source = new BitmapImage(new Uri("pack://application:,,,/gip.core.layoutengine;component/Images/false.png", UriKind.Absolute));
            inlineBoolValue.MaxHeight = 12;
            inlineBoolValue.MaxWidth = 12;
        }

        public virtual int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty = ReportDocument.MaxLengthProperty.AddOwner(typeof(InlineBoolValue));

        /// <summary>
        /// Gets or sets the Key of ReportData-Dictionary
        /// </summary>
        public string DictKey
        {
            get { return (string)GetValue(DictKeyProperty); }
            set { SetValue(DictKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DictKeyProperty =
            DependencyProperty.Register("DictKey", typeof(string), typeof(InlineBoolValue), new UIPropertyMetadata(null));
    }
}
