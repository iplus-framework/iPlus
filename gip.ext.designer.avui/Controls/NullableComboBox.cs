// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// A ComboBox which is Nullable
	/// </summary>
	public class NullableComboBox : ComboBox
	{
		static NullableComboBox()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableComboBox), new FrameworkPropertyMetadata(typeof(NullableComboBox)));
		}

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            var btn = e.NameScope?.Find<Button>("PART_ClearButton");
            if (btn != null)
            {
                btn.PointerPressed += Btn_PointerPressed;
            }
        }

        private void Btn_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Button clearButton)
            {
                var parent = clearButton.GetVisualParent();

                while (parent != null && !(parent is ComboBox))
                {
                    parent = parent.GetVisualParent();
                }

                if (parent is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1;
                }
            }
        }

		public bool IsNullable
		{
			get { return GetValue(IsNullableProperty); }
			set { SetValue(IsNullableProperty, value); }
		}

        public static readonly StyledProperty<bool> IsNullableProperty = 
            AvaloniaProperty.Register<NullableComboBox, bool>(nameof(IsNullable), true);
	}
}
