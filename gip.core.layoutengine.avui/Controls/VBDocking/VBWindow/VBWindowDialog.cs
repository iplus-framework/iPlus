using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a iplus window dialog.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt einen iplus-Fensterdialog dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBWindowDialog'}de{'VBWindowDialog'}", Global.ACKinds.TACVBControl)]
    public class VBWindowDialog : VBWindow
    {
        public VBWindowDialog() : this(null)
        {
        }

        public VBWindowDialog(AvaloniaObject caller)
        {
            Window owner = null;
            if (caller != null)
            {
                owner = caller as Window;
                if (owner == null)
                {
                    owner = caller.TryFindParent<Window>();
                }
            }
            if (   (owner == null || !owner.IsVisible)
                && Database.Root != null 
                && Database.Root.RootPageWPF != null 
                && Database.Root.RootPageWPF.WPFApplication != null)
                owner = ((Database.Root.RootPageWPF.WPFApplication as Application).ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (owner != null)
            {
                try
                {
                    if (owner.Owner == null && Database.Root.RootPageWPF != null && owner != Database.Root.RootPageWPF)
                    {
                        var mainWindow = ((Database.Root.RootPageWPF.WPFApplication as Application).ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                        // TODO:
                        //owner.Show(mainWindow);
                        //(owner as Window).Owner = mainWindow;
                    }
                    this.Owner = owner;
                }
                catch (Exception)
                {
                }
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }
    }
}
