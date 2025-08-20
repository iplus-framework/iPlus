using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public static class RoutedEventHelper
    {
        public static ACCommand GetACCommand(RoutedEventArgs e)
        {
            RoutedUICommandEx n;
            if (e is CanExecuteRoutedEventArgs)
            {
                CanExecuteRoutedEventArgs m = (CanExecuteRoutedEventArgs)e;
                n = (RoutedUICommandEx)m.Command;
            }
            else
            {
                ExecutedRoutedEventArgs m = (ExecutedRoutedEventArgs)e;
                n = (RoutedUICommandEx)m.Command;
            }
            return n.ACCommand;
        }

        public static CommandBinding FindCommandBinding(this CommandBindingCollection cbColl, RoutedUICommandEx appCommand)
        {
            if (appCommand == null)
                return null;
            foreach (CommandBinding vb in cbColl)
            {
                if (vb.Command == appCommand)
                {
                    return vb;
                }
            }
            return null;
        }

        public static bool RemoveCommandBinding(this CommandBindingCollection cbColl, RoutedUICommandEx appCommand)
        {
            CommandBinding cb = RoutedEventHelper.FindCommandBinding(cbColl, appCommand);
            if (cb == null)
                return false;
            cbColl.Remove(cb);
            return true;
        }
    }
}
