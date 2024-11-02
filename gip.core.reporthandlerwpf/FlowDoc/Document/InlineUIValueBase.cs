// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Windows;
using System.Windows.Documents;
using System.Globalization;
using gip.core.datamodel;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Abstract class for fillable run values
    /// </summary>
    public abstract class InlineUIValueBase : InlineUIContainer, IInlinePropertyValue
    {
        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(InlineUIValueBase));

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly DependencyProperty CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner(typeof(InlineUIValueBase));

        public virtual int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty = ReportDocument.MaxLengthProperty.AddOwner(typeof(InlineUIValueBase));

        public virtual int Truncate
        {
            get { return (int)GetValue(TruncateProperty); }
            set { SetValue(TruncateProperty, value); }
        }
        public static readonly DependencyProperty TruncateProperty = ReportDocument.TruncateProperty.AddOwner(typeof(InlineUIValueBase));

        public virtual double MaxWidth
        {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }
        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), typeof(InlineUIValueBase), new UIPropertyMetadata(0.0));

        public virtual double MaxHeight
        {
            get { return (double)GetValue(MaxHeightProperty); }
            set { SetValue(MaxHeightProperty, value); }
        }
        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register("MaxHeight", typeof(double), typeof(InlineUIValueBase), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public virtual object Value
        {
            get
            {
                return (object)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(Const.Value, typeof(object), typeof(InlineUIValueBase), new UIPropertyMetadata(null));

        ///// <summary>
        ///// Identifies the ValueChanged routed event.
        ///// </summary>
        //public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        //    "ValueChanged", RoutingStrategy.Bubble,
        //    typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(InlineUIValueBase));

        ///// <summary>
        ///// Raises the ValueChanged event.
        ///// </summary>
        ///// <param name="args">Arguments associated with the ValueChanged event.</param>
        //protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
        //{
        //    RaiseEvent(args);
        //}


        /// <summary>
        /// Gets or sets the Key of ReportData-Dictionary
        /// </summary>
        public virtual string DictKey
        {
            get { return (string)GetValue(DictKeyProperty); }
            set { SetValue(DictKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DictKeyProperty =
            DependencyProperty.Register("DictKey", typeof(string), typeof(InlineUIValueBase), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the property name
        /// </summary>
        public virtual string VBContent
        {
            get
            {
                return (string)GetValue(VBContentProperty);
            }
            set
            {
                SetValue(VBContentProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VBContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof(string), typeof(InlineUIValueBase), new UIPropertyMetadata(null));

        private string _aggregateGroup = null;
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        public string AggregateGroup
        {
            get { return _aggregateGroup; }
            set { _aggregateGroup = value; }
        }
    }
}
