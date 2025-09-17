// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace gip.ext.designer.avui.Controls
{
    public partial class EnumBar : UserControl
    {
        public EnumBar()
        {
            InitializeComponent();
        }

        Type currentEnumType;

        public static readonly StyledProperty<object> ValueProperty =
            AvaloniaProperty.Register<EnumBar, object>("Value", defaultBindingMode: BindingMode.TwoWay);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly StyledProperty<Panel> ContainerProperty =
            AvaloniaProperty.Register<EnumBar, Panel>("Container");

        public Panel Container
        {
            get { return GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }

        public static readonly StyledProperty<Style> ButtonStyleProperty =
            AvaloniaProperty.Register<EnumBar, Style>("ButtonStyle");

        public Style ButtonStyle
        {
            get { return GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                var newValue = change.NewValue;
                if (newValue != null)
                {
                    var type = newValue.GetType();

                    if (currentEnumType != type)
                    {
                        currentEnumType = type;
                        uxPanel.Children.Clear();
                        foreach (var v in Enum.GetValues(type))
                        {
                            var b = new EnumButton();
                            b.Value = v;
                            string enumName = Enum.GetName(type, v);
                            b.Content = enumName;
                            if (ButtonStyle != null)
                            {
                                b.Styles.Add(ButtonStyle);
                            }
                            b.PointerPressed += button_PointerPressed;
                            uxPanel.Children.Add(b);
                        }
                    }

                    UpdateButtons();
                    UpdateContainer();
                }
            }
            else if (change.Property == ContainerProperty)
            {
                UpdateContainer();
            }
        }

        void UpdateButtons()
        {
            foreach (EnumButton c in uxPanel.Children)
            {
                if (c.Value != null && c.Value.Equals(Value))
                {
                    c.IsChecked = true;
                }
                else
                {
                    c.IsChecked = false;
                }
            }
        }

        void UpdateContainer()
        {
            if (Container != null)
            {
                for (int i = 0; i < uxPanel.Children.Count && i < Container.Children.Count; i++)
                {
                    var c = uxPanel.Children[i] as EnumButton;
                    if (c?.IsChecked == true)
                        Container.Children[i].IsVisible = true;
                    else
                        Container.Children[i].IsVisible = false;
                }
            }
        }

        void button_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                Value = (sender as EnumButton)?.Value;
                e.Handled = true;
            }
        }
    }
}
