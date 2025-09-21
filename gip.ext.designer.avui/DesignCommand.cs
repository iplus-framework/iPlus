// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Input;
using System.Diagnostics;
using Avalonia.Labs.Input;

namespace gip.ext.designer.avui
{
	/// <summary>
	/// Custom implementation of ICommand based on the RelayCommand model by Josh Smith for the designer.
	/// </summary>
	public class DesignCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Func<object, bool> _canExecute;
        
        public DesignCommand(Action<object> action,Func<object,bool> canExecute)
        {
            Debug.Assert(action != null);
            this._action = action;
            this._canExecute = canExecute;
        }
        
        public void Execute(object parameter)
        {
            _action(parameter);
        }
        
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);
            return true;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
