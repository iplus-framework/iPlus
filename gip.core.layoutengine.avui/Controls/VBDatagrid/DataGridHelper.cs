// Copyright (c) Trevor Webster
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace gip.core.layoutengine.avui
{
    public static class DataGridHelper
    {

		//#region SelectedCells
		//public static IList<DataGridCellInfo> GetSelectedCells(AvaloniaObject obj)
		//{
		//	return (IList<DataGridCellInfo>)obj.GetValue(SelectedCellsProperty);
		//}
		//public static void SetSelectedCells(AvaloniaObject obj, IList<DataGridCellInfo> value)
		//{
		//	obj.SetValue(SelectedCellsProperty, value);
		//}
		//public static readonly DependencyProperty SelectedCellsProperty =
		//	DependencyProperty.RegisterAttached("SelectedCells", typeof(IList<DataGridCellInfo>), typeof(DataGridHelper), new UIPropertyMetadata(null, OnSelectedCellsChanged));
		//static SelectedCellsChangedEventHandler GetSelectionChangedHandler(AvaloniaObject obj)
		//{
		//	return (SelectedCellsChangedEventHandler)obj.GetValue(SelectionChangedHandlerProperty);
		//}
		//static void SetSelectionChangedHandler(AvaloniaObject obj, SelectedCellsChangedEventHandler value)
		//{
		//	obj.SetValue(SelectionChangedHandlerProperty, value);
		//}
		//static readonly DependencyProperty SelectionChangedHandlerProperty =
		//	DependencyProperty.RegisterAttached("SelectedCellsChangedEventHandler", typeof(SelectedCellsChangedEventHandler), typeof(DataGridHelper), new UIPropertyMetadata(null));

		////d is MultiSelector (d as ListBox not supported)
		//static void OnSelectedCellsChanged(AvaloniaObject d, DependencyPropertyChangedEventArgs args)
		//{
		//	if (GetSelectionChangedHandler(d) != null)
		//		return;

		//	if (d is DataGrid)//DataGrid
		//	{
		//		DataGrid datagrid = d as DataGrid;
		//		SelectedCellsChangedEventHandler selectionchanged = null;
		//		foreach (var selected in GetSelectedCells(d) as IList<DataGridCellInfo>)
		//			datagrid.SelectedCells.Add(selected);

		//		selectionchanged = (sender, e) =>
		//		{
		//			SetSelectedCells(d, datagrid.SelectedCells);
		//		};
		//		SetSelectionChangedHandler(d, selectionchanged);
		//		datagrid.SelectedCellsChanged += GetSelectionChangedHandler(d);
		//	}
		//	//else if (d is ListBox)
		//	//{
		//	//    ListBox listbox = d as ListBox;
		//	//    SelectionChangedEventHandler selectionchanged = null;

		//	//    selectionchanged = (sender, e) =>
		//	//    {
		//	//        SetSelectedCells(d, listbox.SelectedCells);
		//	//    };
		//	//    SetSelectionChangedHandler(d, selectionchanged);
		//	//    listbox.SelectionChanged += GetSelectionChangedHandler(d);
		//	//}
		//}

		//#region HorizontalMouseWheelScrollingEnabled
		//public static bool GetHorizontalMouseWheelScrollingEnabled(AvaloniaObject obj)
		//{
		//	return (bool)obj.GetValue(HorizontalMouseWheelScrollingEnabledProperty);
		//}
		//public static void SetHorizontalMouseWheelScrollingEnabled(AvaloniaObject obj, bool value)
		//{
		//	obj.SetValue(HorizontalMouseWheelScrollingEnabledProperty, value);
		//}
		//public static readonly DependencyProperty HorizontalMouseWheelScrollingEnabledProperty =
		//	DependencyProperty.RegisterAttached("HorizontalMouseWheelScrollingEnabled", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false, OnHorizontalMouseWheelScrollingEnabledChanged));
		//static void OnHorizontalMouseWheelScrollingEnabledChanged(AvaloniaObject d, DependencyPropertyChangedEventArgs args)
		//{
		//	if (d is DataGrid)
		//	{
		//		DataGrid datagrid = d as DataGrid;
		//		datagrid.Loaded += (sender, e) =>
		//		{
		//			var obj = datagrid.Template.FindName("DG_ScrollViewer", datagrid);
		//			if (obj is ScrollViewer)
		//			{
		//				ScrollViewer scrollviewer = obj as ScrollViewer;
		//				var mhelper = new HorizontalMouseScrollHelper(scrollviewer, datagrid);
		//			}

		//		};
		//	}


		//}
		//#endregion

		public static T FindParent<T>(Control element) where T : Control
		{
			Control parent = LogicalTreeHelper.GetParent(element) as Control;
			//parent.FindName
			while (parent != null)
			{
				T correctlyTyped = parent as T;
				if (correctlyTyped != null)
					return correctlyTyped;
				else
					return FindParent<T>(parent);
			}

			return null;
		}

        public static bool IsImeProcessed(KeyEventArgs keyArgs)
        {
            if (keyArgs != null)
            {
                return keyArgs.Key == Key.ImeProcessed;
            }

            return false;
        }

        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        public static T FindTemplatedParent<T>(Control element) where T : Control
        {
            Control parent = element.TemplatedParent as Control;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as Control;
            }

            return null;
        }

        public static T FindVisualParent<T>(Control element) where T : Control
        {
            Control parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as Control;
            }

            return null;
        }

        internal static void SyncColumnProperty<T>(AvaloniaObject column, AvaloniaObject content, AvaloniaProperty<T> property)
        {
            SyncColumnProperty(column, content, property, property);
        }

        internal static void SyncColumnProperty<T>(AvaloniaObject column, AvaloniaObject content, AvaloniaProperty<T> contentProperty, AvaloniaProperty<T> columnProperty)
        {
            if (!column.IsSet(columnProperty))
            {
                content.ClearValue(contentProperty);
            }
            else
            {
                content.SetValue(contentProperty, column.GetValue(columnProperty));
            }
        }

        public static bool IsDefaultValue(AvaloniaObject d, AvaloniaProperty dp)
        {
            return d.IsSet(dp);
        }

        internal static void RefreshReadOnlyProperty(IGriColumn col, short newReadOnlyState)
        {
            if (col.VBDataGrid == null)
                return;
            if (col.VBIsReadOnly)
            {
                if (!col.IsReadOnly)
                    col.IsReadOnly = true;
                return;
            }
            if (col.ACColumnItem == null)
                return;
            if (col.VBDataGrid.DisabledModes == "Disabled" || col.VBDataGrid.IsSetInVBDisabledColumns(col.ACColumnItem.PropertyName))
            {
                if (!col.IsReadOnly)
                    col.IsReadOnly = true;
                return;
            }
            if (newReadOnlyState >= 0)
            {
                if ((newReadOnlyState == 1) && !col.IsReadOnly)
                    col.IsReadOnly = true;
                else if ((newReadOnlyState == 0) && col.IsReadOnly)
                    col.IsReadOnly = false;
            }
        }

        #region set cell value

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            return parent.TryFindChild<T>();

            //T child = default(T);
            //int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            //for (int i = 0; i < numVisuals; i++)
            //{
            //    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            //    child = v as T;
            //    if (child == null)
            //    {
            //        child = GetVisualChild<T>(v);
            //    }
            //    if (child != null)
            //    {
            //        break;
            //    }
            //}
            //return child;
        }

        //public static DataGridRow GetSelectedRow(this DataGrid grid)
        //{
        //    return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        //}

        //public static DataGridRow GetRow(this DataGrid grid, int index)
        //{
        //    DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
        //    if (row == null)
        //    {
        //        // May be virtualized, bring into view and try again.
        //        grid.UpdateLayout();
        //        grid.ScrollIntoView(grid.Items[index]);
        //        row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
        //    }
        //    return row;
        //}

        //public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        //{
        //    if (row != null)
        //    {
        //        DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

        //        if (presenter == null)
        //        {
        //            grid.ScrollIntoView(row, grid.Columns[column]);
        //            presenter = GetVisualChild<DataGridCellsPresenter>(row);
        //        }

        //        DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
        //        return cell;
        //    }
        //    return null;
        //}

        //public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        //{
        //    DataGridRow rowContainer = grid.GetRow(row);
        //    return grid.GetCell(rowContainer, column);
        //}
        #endregion
    }  
}
