// Copyright (c) Trevor Webster
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using MS.Internal;
using System.Windows.Shapes;
using System.Windows.Interop;

namespace gip.core.layoutengine.avui
{
    public static class DataGridHelper
    {

		#region SelectedCells
		public static IList<DataGridCellInfo> GetSelectedCells(DependencyObject obj)
		{
			return (IList<DataGridCellInfo>)obj.GetValue(SelectedCellsProperty);
		}
		public static void SetSelectedCells(DependencyObject obj, IList<DataGridCellInfo> value)
		{
			obj.SetValue(SelectedCellsProperty, value);
		}
		public static readonly DependencyProperty SelectedCellsProperty =
			DependencyProperty.RegisterAttached("SelectedCells", typeof(IList<DataGridCellInfo>), typeof(DataGridHelper), new UIPropertyMetadata(null, OnSelectedCellsChanged));
		static SelectedCellsChangedEventHandler GetSelectionChangedHandler(DependencyObject obj)
		{
			return (SelectedCellsChangedEventHandler)obj.GetValue(SelectionChangedHandlerProperty);
		}
		static void SetSelectionChangedHandler(DependencyObject obj, SelectedCellsChangedEventHandler value)
		{
			obj.SetValue(SelectionChangedHandlerProperty, value);
		}
		static readonly DependencyProperty SelectionChangedHandlerProperty =
			DependencyProperty.RegisterAttached("SelectedCellsChangedEventHandler", typeof(SelectedCellsChangedEventHandler), typeof(DataGridHelper), new UIPropertyMetadata(null));

		//d is MultiSelector (d as ListBox not supported)
		static void OnSelectedCellsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (GetSelectionChangedHandler(d) != null)
				return;

			if (d is DataGrid)//DataGrid
			{
				DataGrid datagrid = d as DataGrid;
				SelectedCellsChangedEventHandler selectionchanged = null;
				foreach (var selected in GetSelectedCells(d) as IList<DataGridCellInfo>)
					datagrid.SelectedCells.Add(selected);

				selectionchanged = (sender, e) =>
				{
					SetSelectedCells(d, datagrid.SelectedCells);
				};
				SetSelectionChangedHandler(d, selectionchanged);
				datagrid.SelectedCellsChanged += GetSelectionChangedHandler(d);
			}
			//else if (d is ListBox)
			//{
			//    ListBox listbox = d as ListBox;
			//    SelectionChangedEventHandler selectionchanged = null;

			//    selectionchanged = (sender, e) =>
			//    {
			//        SetSelectedCells(d, listbox.SelectedCells);
			//    };
			//    SetSelectionChangedHandler(d, selectionchanged);
			//    listbox.SelectionChanged += GetSelectionChangedHandler(d);
			//}
		}

		#region HorizontalMouseWheelScrollingEnabled
		public static bool GetHorizontalMouseWheelScrollingEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(HorizontalMouseWheelScrollingEnabledProperty);
		}
		public static void SetHorizontalMouseWheelScrollingEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(HorizontalMouseWheelScrollingEnabledProperty, value);
		}
		public static readonly DependencyProperty HorizontalMouseWheelScrollingEnabledProperty =
			DependencyProperty.RegisterAttached("HorizontalMouseWheelScrollingEnabled", typeof(bool), typeof(DataGridHelper), new UIPropertyMetadata(false, OnHorizontalMouseWheelScrollingEnabledChanged));
		static void OnHorizontalMouseWheelScrollingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			if (d is DataGrid)
			{
				DataGrid datagrid = d as DataGrid;
				datagrid.Loaded += (sender, e) =>
				{
					var obj = datagrid.Template.FindName("DG_ScrollViewer", datagrid);
					if (obj is ScrollViewer)
					{
						ScrollViewer scrollviewer = obj as ScrollViewer;
						var mhelper = new HorizontalMouseScrollHelper(scrollviewer, datagrid);
					}

				};
			}


		}
		#endregion

		public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
		{
			FrameworkElement parent = LogicalTreeHelper.GetParent(element) as FrameworkElement;
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

        private const char _escapeChar = '\u001b';
        public static bool HasNonEscapeCharacters(TextCompositionEventArgs textArgs)
        {
            if (textArgs != null)
            {
                string text = textArgs.Text;
                for (int i = 0, count = text.Length; i < count; i++)
                {
                    if (text[i] != _escapeChar)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsImeProcessed(KeyEventArgs keyArgs)
        {
            if (keyArgs != null)
            {
                return keyArgs.Key == Key.ImeProcessed;
            }

            return false;
        }

		#endregion

        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        public static T FindTemplatedParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as FrameworkElement;
            }

            return null;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        internal static void SyncColumnProperty(DependencyObject column, DependencyObject content, DependencyProperty contentProperty, DependencyProperty columnProperty)
        {
            if (IsDefaultValue(column, columnProperty))
            {
                content.ClearValue(contentProperty);

                // Workaround TODO: Bei Aero wird die Foreground-Property von TextBlock in der Datagrid-Cell nicht richtig gesetzt sonderb beliebt weiß warum??
                //if (ControlManager.WpfTheme == eWpfTheme.Aero)
                    //content.SetValue(contentProperty, column.GetValue(columnProperty));
            }
            else
            {
                content.SetValue(contentProperty, column.GetValue(columnProperty));
            }
        }

        public static bool IsDefaultValue(DependencyObject d, DependencyProperty dp)
        {
            return DependencyPropertyHelper.GetValueSource(d, dp).BaseValueSource == BaseValueSource.Default;
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
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridRow GetSelectedRow(this DataGrid grid)
        {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }

        public static DataGridRow GetRow(this DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = grid.GetRow(row);
            return grid.GetCell(rowContainer, column);
        }
        #endregion
    }  
}
