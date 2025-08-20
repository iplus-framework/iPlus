using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;

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
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DialogStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/VBWindow/Themes/DialogStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DialogStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/VBWindow/Themes/DialogStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBWindowDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBWindowDialog), new FrameworkPropertyMetadata(typeof(VBWindowDialog)));
        }

        bool _themeApplied = false;
        public VBWindowDialog() : this(null)
        {
        }

        public VBWindowDialog(DependencyObject caller)
        {
            Window owner = null;
            if (caller != null)
            {
                owner = caller as Window;
                if (owner == null)
                {
                    owner = Window.GetWindow(caller);
                }
            }
            if (   (owner == null || !owner.IsVisible)
                && Database.Root != null 
                && Database.Root.RootPageWPF != null 
                && Database.Root.RootPageWPF.WPFApplication != null)
                owner = (Database.Root.RootPageWPF.WPFApplication as Application).MainWindow;
            if (owner != null)
            {
                try
                {
                    if (owner.Owner == null && Database.Root.RootPageWPF != null && owner != Database.Root.RootPageWPF)
                        owner.Owner = (Database.Root.RootPageWPF.WPFApplication as Application).MainWindow;
                    this.Owner = owner;
                }
                catch (Exception)
                {
                }
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }
    }
}
