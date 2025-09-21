// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Data;
using System.Windows.Input;

namespace gip.ext.designer.avui
{
	public class CallExtension : MarkupExtension
	{
		public CallExtension(string methodName)
		{
			this.methodName = methodName;
		}

		string methodName;

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var t = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
			return new CallCommand(t.TargetObject as Control, methodName);
		}
	}

	public class CallCommand : AvaloniaObject, ICommand
	{
		public CallCommand(Control element, string methodName)
		{
			this.element = element;
			this.methodName = methodName;
			element.DataContextChanged += target_DataContextChanged;

			// Use AvaloniaUI binding approach
			var binding = new Binding("DataContext.Can" + methodName) {
				Source = element
			};
			this.Bind(CanCallProperty, binding);

			GetMethod();
		}

        Control element;
		string methodName;
		MethodInfo method;

		public static readonly StyledProperty<bool> CanCallProperty =
			AvaloniaProperty.Register<CallCommand, bool>(nameof(CanCall), true);

		public bool CanCall {
			get { return GetValue(CanCallProperty); }
			set { SetValue(CanCallProperty, value); }
		}

		public object DataContext {
			get { return element.DataContext; }
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == CanCallProperty) {
				RaiseCanExecuteChanged();
			}
		}

		void GetMethod()
		{
			if (DataContext == null) {
				method = null;
			}
			else {
				method = DataContext.GetType().GetMethod(methodName, Type.EmptyTypes);
			}
		}

		void target_DataContextChanged(object sender, EventArgs e)
		{
			GetMethod();
			RaiseCanExecuteChanged();
		}

		void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null) {
				CanExecuteChanged(this, EventArgs.Empty);
			}
		}

		#region ICommand Members

		public event EventHandler CanExecuteChanged;

		public bool CanExecute(object parameter)
		{
			return method != null && CanCall;
		}

		public void Execute(object parameter)
		{
			method.Invoke(DataContext, null);
		}

		#endregion
	}
}
