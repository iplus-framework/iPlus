// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Labs.Input;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class DefaultCommandsContextMenu : ContextMenu
	{
		private DesignItem designItem;

		public DefaultCommandsContextMenu()
		{
            this.InitializeComponent();
        }

		public DefaultCommandsContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;

            this.InitializeComponent();

            // RoutedCommand.ICommand.Execute routes through CommandManager.FocusedElement.
            // When the user clicks a menu item the popup already holds focus, so FocusedElement
            // points into the PopupRoot – a completely different visual tree from DesignSurface.
            // Replace every RoutedCommand with a wrapper that routes explicitly through the
            // DesignPanel, which is a child of DesignSurface where the CommandBindings live.
            WrapCommandsForExplicitRouting();
        }

        private void WrapCommandsForExplicitRouting()
        {
            foreach (var item in Items.OfType<MenuItem>())
            {
                if (item.Command is RoutedCommand routedCmd)
                {
                    item.Command = new ExplicitTargetCommand(routedCmd,
                        () => designItem?.Context?.Services?.DesignPanel as IInputElement);
                }
            }
        }

        /// <summary>
        /// Wraps a <see cref="RoutedCommand"/> so that both CanExecute and Execute are routed
        /// to a caller-supplied target element instead of <see cref="CommandManager.FocusedElement"/>.
        /// This is needed for context-menu items whose popup steals keyboard focus before Execute
        /// is invoked, which would otherwise cause the routed event to miss the intended handler.
        /// </summary>
        private sealed class ExplicitTargetCommand : System.Windows.Input.ICommand
        {
            private readonly RoutedCommand _inner;
            private readonly Func<IInputElement> _getTarget;

            public ExplicitTargetCommand(RoutedCommand inner, Func<IInputElement> getTarget)
            {
                _inner = inner;
                _getTarget = getTarget;
                // Forward CanExecuteChanged so MenuItem refreshes its enabled state.
                ((System.Windows.Input.ICommand)_inner).CanExecuteChanged += OnInnerCanExecuteChanged;
            }

            private void OnInnerCanExecuteChanged(object sender, EventArgs e)
                => CanExecuteChanged?.Invoke(sender, e);

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                var target = _getTarget();
                return target != null && _inner.CanExecute(parameter, target);
            }

            public void Execute(object parameter)
            {
                var target = _getTarget();
                if (target != null)
                    _inner.Execute(parameter, target);
            }
        }
    }
}

