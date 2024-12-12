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
    public abstract class InlineValueBase : Run, IInlineValue
    {
        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(InlineValueBase));

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly DependencyProperty CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner(typeof(InlineValueBase));

        public virtual int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty = ReportDocument.MaxLengthProperty.AddOwner(typeof(InlineValueBase));

        public virtual int Truncate
        {
            get { return (int)GetValue(TruncateProperty); }
            set { SetValue(TruncateProperty, value); }
        }
        public static readonly DependencyProperty TruncateProperty = ReportDocument.TruncateProperty.AddOwner(typeof(InlineValueBase));

        public int FontWidth
        {
            get { return (int)GetValue(FontWidthProperty); }
            set { SetValue(FontWidthProperty, value); }
        }

        public static readonly DependencyProperty FontWidthProperty =
            DependencyProperty.Register("FontWidth", typeof(int), typeof(InlineValueBase), new PropertyMetadata(0));



        /// <summary>
        /// Gets or sets the object value
        /// </summary>
        public virtual object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set 
            { 
                SetValue(ValueProperty, value);
                if (value is InlineUIContainer && this.Parent is Paragraph)
                    ((Paragraph)this.Parent).Inlines.Add(value as InlineUIContainer);
                else
                {
                    if (!string.IsNullOrEmpty(Text))
                    {
                        Type valueType = value.GetType();
                        if (!valueType.IsPrimitive)
                            return;
                    }
                    Text = FormatValue(value, StringFormat, CultureInfo, MaxLength, Truncate);
                }
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(Const.Value, typeof(object), typeof(InlineValueBase), new UIPropertyMetadata(null));

        ///// <summary>
        ///// Identifies the ValueChanged routed event.
        ///// </summary>
        //public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        //    "ValueChanged", RoutingStrategy.Bubble,
        //    typeof(RoutedPropertyChangedEventHandler<decimal>), typeof(InlineValueBase));

        ///// <summary>
        ///// Raises the ValueChanged event.
        ///// </summary>
        ///// <param name="args">Arguments associated with the ValueChanged event.</param>
        //protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<decimal> args)
        //{
        //    RaiseEvent(args);
        //}

        /// <summary>
        /// Formats a value for output
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="format">format</param>
        /// <returns></returns>
        public static string FormatValue(object value, string format, string cultureInfoString, int maxLength, int truncate)
        {
            if (value == null)
                return "";
            string result = "";
            if (String.IsNullOrEmpty(format))
            {
                result = value.ToString();
                if (truncate > 0)
                {
                    if (result.Length > truncate)
                        result = result.Substring(truncate);
                }
                else if (truncate < 0)
                {
                    truncate = Math.Abs(truncate);
                    if (result.Length > truncate)
                        result = result.Substring(0, result.Length - truncate);
                }

                if (maxLength > 0)
                {
                    if (result.Length > maxLength)
                        result = result.Substring(0, maxLength);
                }
                else if (maxLength < 0)
                {
                    maxLength = Math.Abs(maxLength);
                    if (result.Length > maxLength)
                        result = result.Substring(result.Length - maxLength, maxLength);
                }
                return result;
            }
            System.Globalization.CultureInfo cultureInfo = null;
            if (!String.IsNullOrEmpty(cultureInfoString))
            {
                try
                {
                    cultureInfo = System.Globalization.CultureInfo.GetCultureInfo(cultureInfoString);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("InlineValueBase", "FormatValue", msg);
                }
            }

            Type type = value.GetType();

            if (type == typeof(DateTime))
            {
                if (cultureInfo != null)
                    result = ((DateTime)value).ToString(format, cultureInfo);
                else
                    result = ((DateTime)value).ToString(format);
            }
            else if (type == typeof(TimeSpan))
            {
                if (cultureInfo != null)
                    result = new DateTime(((TimeSpan)value).Ticks).ToString(format, cultureInfo);
                else
                    result = new DateTime(((TimeSpan)value).Ticks).ToString(format);
            }
            else if (type == typeof(decimal))
            {
                if (cultureInfo != null)
                    result = ((decimal)value).ToString(format, cultureInfo);
                else
                    result = ((decimal)value).ToString(format);
            }
            else if (type == typeof(double))
            {
                if (cultureInfo != null)
                    result = ((double)value).ToString(format, cultureInfo);
                else
                    result = ((double)value).ToString(format);
            }
            else if (type == typeof(float))
            {
                if (cultureInfo != null)
                    result = ((float)value).ToString(format, cultureInfo);
                else
                    result = ((float)value).ToString(format);
            }
            else if (type == typeof(int))
            {
                if (cultureInfo != null)
                    result = ((int)value).ToString(format, cultureInfo);
                else
                    result = ((int)value).ToString(format);
            }
            else if (type == typeof(long))
            {
                if (cultureInfo != null)
                    result = ((long)value).ToString(format, cultureInfo);
                else
                    result = ((long)value).ToString(format);
            }
            else if (type == typeof(short))
            {
                if (cultureInfo != null)
                    result = ((short)value).ToString(format, cultureInfo);
                else
                    result = ((short)value).ToString(format);
            }
            else if (type == typeof(uint))
            {
                if (cultureInfo != null)
                    result = ((uint)value).ToString(format, cultureInfo);
                else
                    result = ((uint)value).ToString(format);
            }
            else if (type == typeof(ulong))
            {
                if (cultureInfo != null)
                    result = ((ulong)value).ToString(format, cultureInfo);
                else
                    result = ((ulong)value).ToString(format);
            }
            else if (type == typeof(ushort))
            {
                if (cultureInfo != null)
                    result = ((ushort)value).ToString(format, cultureInfo);
                else
                    result = ((ushort)value).ToString(format);
            }
            else
                result = value.ToString();

            if (truncate > 0)
            {
                if (result.Length > truncate)
                    result = result.Substring(truncate);
            }
            else if (truncate < 0)
            {
                truncate = Math.Abs(truncate);
                if (result.Length > truncate)
                    result = result.Substring(0, result.Length - truncate);
            }

            if (maxLength > 0)
            {
                if (result.Length > maxLength)
                    return result.Substring(0, maxLength);
            }
            else if (maxLength < 0)
            {
                maxLength = Math.Abs(maxLength);
                if (result.Length > maxLength)
                    return result.Substring(result.Length - maxLength, maxLength);
            }
            return result;
        }
    }
}
