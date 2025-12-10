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
            RoutedUICommandEx n;
            if (e is CanExecuteRoutedEventArgs)
            {
                CanExecuteRoutedEventArgs m = (CanExecuteRoutedEventArgs)e;
                n = m.Command as RoutedUICommandEx;
            }
            else
            {
                ExecutedRoutedEventArgs m = (ExecutedRoutedEventArgs)e;
                n = m.Command as RoutedUICommandEx;
            }
            return n?.ACCommand;
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
