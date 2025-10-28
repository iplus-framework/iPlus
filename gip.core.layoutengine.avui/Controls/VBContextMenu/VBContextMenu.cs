using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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
        public VBContextMenu()
            : base()
        {
            IRootPageWPF vbWPF = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as IRootPageWPF;
            if (vbWPF != null)
                Zoom = ControlManager.TouchScreenMode ? vbWPF.Zoom + 15 : vbWPF.Zoom;
            else
                Zoom = ControlManager.TouchScreenMode ? 115 : 100;
        }

        public VBContextMenu(IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
            : this()
        {
            IRootPageWPF vbWPF = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as IRootPageWPF;
            if (vbWPF != null)
                Zoom = ControlManager.TouchScreenMode ? vbWPF.Zoom + 15 : vbWPF.Zoom;
            else
                Zoom = ControlManager.TouchScreenMode ? 115 : 100;

            FillContextMenu(this, acElement, acMenuItemList);
        }

        private List<VBMenuItem> _AppCommandsToRemove = new List<VBMenuItem>();

        public static readonly StyledProperty<double> ZoomProperty = AvaloniaProperty.Register<VBContextMenu, double>(nameof(Zoom));
        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsOpenProperty && !(bool)change.NewValue)
            {
                if (_AppCommandsToRemove == null || PlacementTarget == null)
                    return;

                foreach (var acMenuItem in _AppCommandsToRemove)
                {
                    ICommand command = AppCommands.FindVBApplicationCommand(acMenuItem.ACCommand.ACUrl);
                    if (command != null)
                    {
                        AppCommands.RemoveVBApplicationCommand(command);
                        // TODO: Remove in Avalonia
                        //acMenuItem.CommandBindings.RemoveCommandBinding(command as RoutedUICommandEx);
                    }
                }
                PlacementTarget = null;
            }
        }
    }
}
