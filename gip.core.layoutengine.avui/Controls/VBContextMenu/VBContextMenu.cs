using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a pop-up menu that enables a control to expose functionality that is specific to the context of the control.
    /// </summary>
    /// <summary>
    /// Stellt ein Popup-Menü dar, das es einem Control ermöglicht, eine für den Kontext des Controls spezifische Funktionalität freizugeben.
    /// </summary>
    public class VBContextMenu : ContextMenu
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ContextMenuStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBContextMenu/Themes/ContextMenuStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ContextMenuStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBContextMenu/Themes/ContextMenuStyleAero.xaml" },
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

        static VBContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBContextMenu), new FrameworkPropertyMetadata(typeof(VBContextMenu)));
        }

        bool _themeApplied = false;
        public VBContextMenu()
            : base()
        {
            IRootPageWPF vbWPF = Application.Current?.MainWindow as IRootPageWPF;
            if (vbWPF != null)
                Zoom = ControlManager.TouchScreenMode ? vbWPF.Zoom + 15 : vbWPF.Zoom;
            else
                Zoom = ControlManager.TouchScreenMode ? 115 : 100;
        }

        public VBContextMenu(IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
            : this()
        {
            IRootPageWPF vbWPF = Application.Current?.MainWindow as IRootPageWPF;
            if (vbWPF != null)
                Zoom = ControlManager.TouchScreenMode ? vbWPF.Zoom + 15 : vbWPF.Zoom;
            else
                Zoom = ControlManager.TouchScreenMode ? 115 : 100;

            FillContextMenu(this, acElement, acMenuItemList);
        }

        private List<VBMenuItem> _AppCommandsToRemove = new List<VBMenuItem>();

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

        //public void ShowContextMenu(IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
        //{
        //    VBContextMenu contextMenu = new VBContextMenu(this);
        //    FillContextMenu(contextMenu, acElement, acMenuItemList);
        //    contextMenu.StaysOpen = true;
        //    contextMenu.IsOpen = true;
        //}

        public double Zoom
        {
            get;
            set;
        }

        private void FillContextMenu(ItemsControl itemsControl, IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
        {
            int countRestElements = acMenuItemList.Count();
            int nLoopCount = 0;
            List<ACMenuItem> copyList = acMenuItemList.ToList();
            bool previousSeparator = false;
            if (countRestElements > 0)
            {
                foreach (var acMenuItem in acMenuItemList)
                {
                    nLoopCount++;
                    countRestElements--;
                    if (acMenuItem.ACCaption == "-")
                    {
                        if (!previousSeparator)
                        {
                            if (countRestElements > 0)
                            {
                                if (copyList[nLoopCount].ACCaption == "-")
                                    continue;
                                else
                                {
                                    itemsControl.Items.Add(new VBMenuSeparator());
                                    previousSeparator = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        previousSeparator = false;
                        ICommand command = AppCommands.FindVBApplicationCommand(acMenuItem.ACUrl);
                        VBMenuItem vbMenuItem = new VBMenuItem(acElement.ContextACObject, acMenuItem);
                        if (command == null)
                            _AppCommandsToRemove.Add(vbMenuItem);
                        itemsControl.Items.Add(vbMenuItem);
                        if (acMenuItem.Items != null && acMenuItem.Items.Count > 0)
                        {
                            FillContextMenu(vbMenuItem, acElement, acMenuItem.Items);
                        }
                    }
                }
            }
        }

        protected override void OnClosed(RoutedEventArgs e)
        {
            //@ihrastinski NOTE: Remote desktop context menu problem - check PlacementTarget is null
            if (_AppCommandsToRemove == null || PlacementTarget == null)
                return;

            foreach (var acMenuItem in _AppCommandsToRemove)
            {
                ICommand command = AppCommands.FindVBApplicationCommand(acMenuItem.ACCommand.ACUrl);
                if (command != null)
                {
                    AppCommands.RemoveVBApplicationCommand(command);
                    acMenuItem.CommandBindings.RemoveCommandBinding(command as RoutedUICommandEx);
                }
            }
            PlacementTarget = null;
            base.OnClosed(e);
            //Console.WriteLine("OK");
        }
    }
}
