// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.PropertyGrid;
using System.Reflection;
using gip.ext.design.avui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Reactive.Linq;

namespace gip.ext.designer.avui.PropertyGrid.Editors
{
    [TypeEditor(typeof(byte))]
    [TypeEditor(typeof(sbyte))]
    [TypeEditor(typeof(decimal))]
    [TypeEditor(typeof(double))]
    [TypeEditor(typeof(float))]
    [TypeEditor(typeof(int))]
    [TypeEditor(typeof(uint))]
    [TypeEditor(typeof(long))]
    [TypeEditor(typeof(ulong))]
    [TypeEditor(typeof(short))]
    [TypeEditor(typeof(ushort))]
    public partial class NumberEditor : Controls.NumericUpDown
    {
        static NumberEditor()
        {
            minimums[typeof(byte)] = byte.MinValue;
            minimums[typeof(sbyte)] = sbyte.MinValue;
            minimums[typeof(decimal)] = (double)decimal.MinValue;
            minimums[typeof(double)] = double.MinValue;
            minimums[typeof(float)] = float.MinValue;
            minimums[typeof(int)] = int.MinValue;
            minimums[typeof(uint)] = uint.MinValue;
            minimums[typeof(long)] = long.MinValue;
            minimums[typeof(ulong)] = ulong.MinValue;
            minimums[typeof(short)] = short.MinValue;
            minimums[typeof(ushort)] = ushort.MinValue;

            maximums[typeof(byte)] = byte.MaxValue;
            maximums[typeof(sbyte)] = sbyte.MaxValue;
            maximums[typeof(decimal)] = (double)decimal.MaxValue;
            maximums[typeof(double)] = double.MaxValue;
            maximums[typeof(float)] = float.MaxValue;
            maximums[typeof(int)] = int.MaxValue;
            maximums[typeof(uint)] = uint.MaxValue;
            maximums[typeof(long)] = long.MaxValue;
            maximums[typeof(ulong)] = ulong.MaxValue;
            maximums[typeof(short)] = short.MaxValue;
            maximums[typeof(ushort)] = ushort.MaxValue;
        }

        public NumberEditor()
        {
            InitializeComponent();
            this.GetObservable(StyledElement.DataContextProperty).Subscribe(_ => NumberEditor_DataContextChanged());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        static Dictionary<Type, double> minimums = new Dictionary<Type, double>();
        static Dictionary<Type, double> maximums = new Dictionary<Type, double>();

        public IPropertyNode PropertyNode
        {
            get { return DataContext as IPropertyNode; }
        }

        void NumberEditor_DataContextChanged()
        {
            if (PropertyNode == null) return;
            var type = PropertyNode.FirstProperty.ReturnType;

            var range = Metadata.GetValueRange(PropertyNode.FirstProperty);
            if (range == null)
            {
                range = new NumberRange() { Min = 0, Max = double.MaxValue };
            }

            double test = 0;
            if (!maximums.TryGetValue(type, out test))
                return;
            if (!minimums.TryGetValue(type, out test))
                return;

            if (range.Min == double.MinValue)
            {
                Minimum = minimums[type];
            }
            else
            {
                Minimum = range.Min;
            }

            if (range.Max == double.MaxValue)
            {
                Maximum = maximums[type];
            }
            else
            {
                Maximum = range.Max;
            }

            if (Minimum == 0 && Maximum == 1)
            {
                DecimalPlaces = 2;
                SmallChange = 0.01;
                LargeChange = 0.1;
            }
            else
            {
                this.ClearValue(DecimalPlacesProperty);
                this.ClearValue(SmallChangeProperty);
                this.ClearValue(LargeChangeProperty);
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            TextBox textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            if (textBox != null)
                textBox.TextChanged += TextValueChanged;
        }

        private void TextValueChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (PropertyNode == null)
                return;
            if (textBox == null)
                return;
            double val;
            if (double.TryParse(textBox.Text, out val))
            {
                if (PropertyNode.FirstProperty.TypeConverter.IsValid(textBox.Text))
                {
                    if (val >= Minimum && val <= Maximum || double.IsNaN(val))
                    {
                        textBox.Foreground = Brushes.Black;
                        ToolTip.SetTip(textBox, textBox.Text);
                    }
                    else
                    {
                        textBox.Foreground = Brushes.DarkBlue;
                        ToolTip.SetTip(textBox, "Value should be in between " + Minimum + " and " + Maximum);
                    }
                }
                else
                {
                    textBox.Foreground = Brushes.DarkRed;
                    ToolTip.SetTip(textBox, "Cannot convert to Type : " + PropertyNode.FirstProperty.ReturnType.Name);
                }
            }
            else
            {
                textBox.Foreground = Brushes.DarkRed;
                ToolTip.SetTip(textBox, string.IsNullOrWhiteSpace(textBox.Text) ? null : "Value does not belong to any numeric type");
            }

        }

        ChangeGroup group;

        protected override void OnDragStarted()
        {
            group = PropertyNode.Context.OpenGroup("drag number",
                PropertyNode.Properties.Select(p => p.DesignItem).ToArray());
        }

        protected override void OnDragCompleted()
        {
            group.Commit();
        }
    }
}
