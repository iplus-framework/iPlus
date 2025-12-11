using Avalonia.Interactivity;
using Avalonia.Labs.Input;
using gip.core.datamodel;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public static class RoutedEventHelper
    {
        public static ACCommand GetACCommand(RoutedEventArgs e)
        {
            RoutedCommand n;
            if (e is CanExecuteRoutedEventArgs)
            {
                CanExecuteRoutedEventArgs m = (CanExecuteRoutedEventArgs)e;
                n = m.Command as RoutedCommand;
            }
            else
            {
                ExecutedRoutedEventArgs m = (ExecutedRoutedEventArgs)e;
                n = m.Command as RoutedCommand;
            }
            if (n == null)
                return null;
            else if (n is RoutedUICommandEx ex)
                return ex.ACCommand;
            else
            {
                return AppCommands.GetACCommandIfStandardApplicationCommands(n);
            }
        }

        public static CommandBinding FindCommandBinding(this IList<CommandBinding> cbColl, RoutedUICommandEx appCommand)
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

        public static bool RemoveCommandBinding(this IList<CommandBinding> cbColl, RoutedUICommandEx appCommand)
        {
            CommandBinding cb = RoutedEventHelper.FindCommandBinding(cbColl, appCommand);
            if (cb == null)
                return false;
            cbColl.Remove(cb);
            return true;
        }
    }
}
