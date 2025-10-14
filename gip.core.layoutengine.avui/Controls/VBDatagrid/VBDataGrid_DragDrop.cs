using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    public partial class VBDataGrid
    {
        #region DragAndDrop

        protected void OnLeftButtonDown(PointerPressedEventArgs e)
        {
            if (DragEnabled == DragMode.Enabled)
            {
                object dragItem = SelectedItem;
                if (dragItem is IACObject)
                {
                    string vbContent = null;
                    UpdateACContentList(dragItem as IACObject, vbContent);
                    VBDragDrop.VBDoDragDrop(e, this);
                }
            }
            else if (IsEnabledMoveRows)
            {
                var row = this.InputHitTest(e.GetPosition(this)) as DataGridRow;
                _DraggedItem = row?.DataContext;
            }
        }

        /// <summary>
        /// Handles the OnDragEnter event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected void OnDragEnter(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                //base.OnDragEnter(e);
                return;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.Source.ToString()); // as Control
#endif
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
        }

        /// <summary>
        /// Handles the OnDragLeave event.
        /// </summary>
        /// <param name="e">The DragEvents arguments.</param>
        protected void OnDragLeave(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
        }

        /// <summary>
        /// Handles the OnDragOver event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected void OnDragOver(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
        }

        /// <summary>
        /// Handles the OnDrop event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected void OnDrop(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                return;
            }
            //HandleDrop(e.Source, 0, 0, e);
            HandleDrop(this, 0, 0, e);
        }

        /// <summary>
        /// Handles the DragOver event.
        /// </summary>
        /// <param name="sender">The sender paramter.</param>
        /// <param name="x">The x-cordinate paramter.</param>
        /// <param name="y">The y-cordinate paramter.</param>
        /// <param name="e">The DragEvents arguments.</param>
        public void HandleDragOver(object sender, double x, double y, DragEventArgs e)
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
                    HandleDragOver_Move(sender, x, y, e);
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, x, y, e);
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Move(object sender, double x, double y, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das verschieben erlaubt ist
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
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
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="x">The x-cordinate paramter.</param>
        /// <param name="y">The y-cordinate paramter.</param>
        /// <param name="e">The DragEvents arguments.</param>
        public void HandleDrop(object sender, double x, double y, DragEventArgs e)
        {
            Control uiElement = e.Source as Control;

            if (uiElement == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.DragEffects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            switch (e.DragEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    {
                        if (e.KeyModifiers != KeyModifiers.Control)
                        {
                            e.DragEffects = DragDropEffects.None;
                            return;
                        }
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
                        ACAction(actionArgs);
                        e.Handled = true;
                        return;
                    }
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.DragEffects = DragDropEffects.None;
                    return;
            }
        }

        private Control GetNearestContainer(Control element, ref string vbContent)
        {
            if (element == null)
                return null;

            if (element is DataGridCellsPresenter)
            {
                DataGridCellsPresenter presenter = element as DataGridCellsPresenter;
                return presenter;
            }
            else //if (element is Border)
            {
                int count = 0;
                while (element != null && count < 10)
                {
                    if (vbContent == null)
                    {
                        if (element is TextBox)
                        {
                            TextBox textBox = element as TextBox;
                            BindingExpressionBase be = BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty);
                            // TODO: Doesn't work in Avalonia
                            //vbContent = be.ParentBinding.Path.Path;
                        }
                        if (element is ContentPresenter)
                        {
                            ContentPresenter contentPresenter = element as ContentPresenter;
                            if (contentPresenter.Content is VBCheckBox)
                            {
                                VBCheckBox vbCheckbox = contentPresenter.Content as VBCheckBox;
                                DataGridBoundColumn dataGridColumn = ((DataGridCell)(vbCheckbox.Parent)).GetOwningColumnViaReflection() as DataGridBoundColumn;
                                vbContent = ((System.Windows.Data.Binding)(dataGridColumn.Binding)).Path.Path;
                            }
                        }
                        if (element is TextBlock)
                        {
                            TextBlock textBlock = element as TextBlock;
                            BindingExpressionBase be = BindingOperations.GetBindingExpressionBase(textBlock, TextBlock.TextProperty);
                            if (be != null)
                            {
                                // TODO: Doesn't work in Avalonia
                                //vbContent = be.ParentBinding.Path.Path;
                            }
                            else
                            {
                                vbContent = textBlock.Text;
                            }
                        }
                        if (element is ComboBox)
                        {
                            ComboBox comboBox = element as ComboBox;
                            BindingExpressionBase be = BindingOperations.GetBindingExpressionBase(comboBox, ComboBox.SelectedItemProperty);
                            if (be != null)
                            {
                                // TODO: Doesn't work in Avalonia
                                //vbContent = be.ParentBinding.Path.Path;
                            }
                            else
                            {
                                BindingExpressionBase be2 = BindingOperations.GetBindingExpressionBase(comboBox, ComboBox.SelectedValueProperty);
                                // TODO: Doesn't work in Avalonia
                                //vbContent = be2.ParentBinding.Path.Path;
                            }
                        }
                    }
                    element = VisualTreeHelper.GetParent(element) as Control;
                    if ((element != null) && (element is DataGridCellsPresenter || element is DataGridColumnHeader))
                        return element as Control;
                    count++;
                }
            }
            return null;
        }
        #endregion

        #region MovableRows(DragAndDrop)

        protected void OnPreviewMouseLeftButtonDown(PointerPressedEventArgs e)
        {
            if (IsEnabledMoveRows)
            {
                var rowItem = this.InputHitTest(e.GetPosition(this));
                var row = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(rowItem as AvaloniaObject, typeof(DataGridRow)) as DataGridRow;
                _DraggedItem = row?.DataContext;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
             if (_DraggedItem == null) 
                return;
            var rowItem = this.InputHitTest(e.GetPosition(this));
            var row = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(rowItem as AvaloniaObject, typeof(DataGridRow)) as DataGridRow;
            if (row == null || row.GetIsEditingViaReflection()) 
                return;
            ExchangeItems(row.DataContext);

            base.OnPointerMoved(e);
        }

        protected void OnPreviewMouseLeftButtonUp(PointerReleasedEventArgs e)
        {
            // Onnly preview events:
            if (e.Route != RoutingStrategies.Tunnel)
                return;

            if (_DraggedItem == null) 
                return;
            ExchangeItems(SelectedItem);
            //select the dropped item
            SelectedItem = _DraggedItem;
            //reset
            _DraggedItem = null;

            if (!string.IsNullOrEmpty(_VBOnDrag) && (ContextACObject != null))
            {
                string[] vbDragData = _VBOnDrag.Split(',');
                Dictionary<int, string> newOrderInfo = new Dictionary<int, string>();
                int nr = 0;
                foreach (var item in this.GetDataGridCollectionViewViaReflection())
                {
                    nr++;
                    object itemObject = item.GetValue(vbDragData[1]);
                    if (itemObject == null)
                    {
                        itemObject = item.GetType().GetProperty(vbDragData[1]).GetValue(item);
                    }
                    string itemValue = itemObject.ToString();
                    newOrderInfo.Add(nr, itemValue);
                }
                ContextACObject.ACUrlCommand(vbDragData[0], new object[] { newOrderInfo });
            }
        }

        private void ExchangeItems(object targetItem)
        {
            try
            {
                if (_DraggedItem == null)
                    return;

                foreach (var col in Columns)
                {
                    col.ClearSort();
                }

                if (targetItem != null && !ReferenceEquals(_DraggedItem, targetItem))
                {
                    var list = this.ItemsSource as IList;
                    if (list == null)
                        throw new ApplicationException("EnableRowsMoveProperty requires the ItemsSource property of DataGrid to be at least IList inherited collection. Use ObservableCollection to have movements reflected in UI.");
                    //get target index
                    //var targetIndex = list.IndexOf(targetItem);
                    //DataGridRow row = (DataGridRow)ItemContainerGenerator.ContainerFromIndex(targetIndex);

                    DataGridRow row = this.GetRowFromItemViaReflection(targetItem);
                    if (row != null && !row.GetIsEditingViaReflection())
                    {
                        int targetIndex = row.Index;
                        //remove the source from the list
                        if (targetIndex >= 0)
                            list.Remove(_DraggedItem);

                        //move source at the target's location
                        if (targetIndex >= 0)
                            list.Insert(targetIndex, _DraggedItem);
                    }
                }
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException(BSOACComponent != null ? BSOACComponent.GetACUrl() : "VBDataGrid", "VBDataGrid.ExchangeItems(10)", e);
            }
        }

        #endregion
    }
}
