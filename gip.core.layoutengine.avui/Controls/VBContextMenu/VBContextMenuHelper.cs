// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Browser (WASM) support for context menus.
    ///
    /// The classic right-click handlers of the VB*-Controls call
    /// <c>BSOACComponent.ACAction(actionArgs)</c> synchronously. For proxies this performs a
    /// remote invocation that blocks on a wait handle — impossible on the single-threaded
    /// browser UI, where the response can only be delivered while the event loop is running
    /// (a blocking wait deadlocks the whole app).
    ///
    /// This helper installs ONE tunneling handler on the root view. On the browser it
    /// intercepts right-button releases before the controls see them, fetches the menu via
    /// the async path (<see cref="IACInteractiveObjectAsync.ACActionAsync"/>) and opens the
    /// <see cref="VBContextMenu"/> when the server has responded. On other platforms it does
    /// nothing and the controls behave as before.
    /// </summary>
    public static class VBContextMenuHelper
    {
        /// <summary>
        /// Registers the browser right-click interceptor on a root view. No-op on
        /// non-browser platforms.
        /// </summary>
        public static void RegisterBrowserRightClickHandler(Control root)
        {
            if (!OperatingSystem.IsBrowser())
                return;
            root.AddHandler(InputElement.PointerReleasedEvent, OnPreviewPointerReleased, RoutingStrategies.Tunnel);
        }

        private static void OnPreviewPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.Handled || e.InitialPressMouseButton != MouseButton.Right)
                return;

            // Find the deepest visual that is an IACInteractiveObject with a VBContent —
            // this is the element whose content resolves to the target component (like the
            // desktop control handlers). Pure presentation elements (e.g. a VBTextBlock
            // inside a button) have no VBContent and would only yield the generic menu.
            Control owner = null;
            IACBSO ownerBso = null;
            for (Visual v = e.Source as Visual; v != null; v = v.GetVisualParent())
            {
                if (v is Control control && v is IACInteractiveObject)
                {
                    string vbContent = (control as IACInteractiveObject).VBContent;
                    if (String.IsNullOrEmpty(vbContent))
                        continue;
                    IACBSO bso = control.GetValue(ContentPropertyHandler.BSOACComponentProperty);
                    if (bso == null)
                        continue;
                    owner = control;
                    ownerBso = bso;
                    break;
                }
            }
            if (owner == null)
                return;
            e.Handled = true;
            OpenContextMenuAsync(owner, ownerBso, e.GetPosition(owner));
        }

        /// <summary>
        /// Fetches the menu asynchronously (remote invocations are awaited) and opens the
        /// context menu once the server has responded.
        /// </summary>
        public static async void OpenContextMenuAsync(Control control, IACBSO bso, Point point)
        {
            try
            {
                IACInteractiveObject acElement = control as IACInteractiveObject;
                if (acElement == null || bso == null)
                    return;

                ACActionMenuArgs actionArgs = new ACActionMenuArgs(acElement, point.X, point.Y, Global.ElementActionType.ContextMenu);
if (bso is IACInteractiveObjectAsync asyncBso)
                await asyncBso.ACActionAsync(actionArgs);
                else
                    bso.ACAction(actionArgs);

                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(acElement, actionArgs.ACMenuItemList);
                    control.ContextMenu = vbContextMenu;
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = control;
                    vbContextMenu.Closed += (s, args) => { control.ContextMenu = null; };
                    control.ContextMenu.Open();
                }
            }
            catch (Exception)
            {
                // Never crash the UI thread from the fire-and-forget menu opener.
            }
        }
    }
}
