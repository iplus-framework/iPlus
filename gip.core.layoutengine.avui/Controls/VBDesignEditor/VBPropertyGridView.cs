using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using gip.ext.design.avui;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a grid view for properties.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Rasteransicht für Eigenschaften dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPropertyGridView'}de{'VBPropertyGridView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBPropertyGridView : PropertyGridView
    {
        #region c'tors

        protected override gip.ext.designer.avui.PropertyGrid.PropertyGrid OnCreatePropertyGrid()
        {
            return new VBPropertyGrid();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            DragDrop.SetAllowDrop(this, true);
            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            AddHandler(DragDrop.DropEvent, OnDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);

        }


        #endregion

        #region DragAndDrop
        void OnDragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            HandleDrop(sender, e);
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
        }

        public void HandleDragLeave(object sender, DragEventArgs e)
        {
        }

        public void HandleDragOver(object sender, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.DragEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    e.Handled = true;
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, 0, 0, e);
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            CurrentPropertyNode = GetPropertyNode(sender, e, false);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null || CurrentPropertyNode == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            // Drag und Drop auf Unterelement nicht erlauben!!
            e.Handled = true;
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            CurrentPropertyNode = GetPropertyNode(sender, e, true);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.DragEffects)
            {
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    return;
            }
        }


        private VBPropertyNode GetPropertyNode(object sender, DragEventArgs e, bool IsDrop)
        {
            Point x = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, x);
            if (result == null)
                return null;
            if ((result.VisualHit == null) || !(result.VisualHit is AvaloniaObject))
                return null;
#if DEBUG
            if (IsDrop)
            {
                System.Diagnostics.Debug.WriteLine("GetPropertyNode() P:" + x.ToString() + " Type:" + result.VisualHit.GetType().FullName);
            }
#endif
            Border row;
            VBPropertyNode node = GetNodeOverVisualPos(e.Source as Visual, out row) as VBPropertyNode;
            return node;
        }


        /// <summary>
        /// The ACAction method.
        /// </summary>
        /// <param name="actionArgs">The ACAction arguments.</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            BSOACComponent.ACActionToTarget(CurrentPropertyNode, actionArgs);
        }

        /// <summary>
        /// Determines is enabled ACAction.
        /// </summary>
        /// <param name="actionArgs">The ACAction arguments.</param>
        ///<returns>Returns true if is ACAction enabled, otherwise false.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return BSOACComponent.IsEnabledACActionToTarget(CurrentPropertyNode, actionArgs);
        }

        /// <summary>
        /// Zielobjekt beim ACDropData
        /// </summary>
        VBPropertyNode CurrentPropertyNode
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        ///
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBPropertyGridView>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }
        #endregion
    }
}
