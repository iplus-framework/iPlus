// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;   // NotifyCollectionChangedEventHandler
using System.Diagnostics;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    ///     An GridViewRowPresenter marks the site (in a style) of the panel that controls
    ///     layout of groups or items.
    /// </summary>
    public class GridViewRowPresenter : GridViewRowPresenterBase
    {
        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        ///     Returns a string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("GridViewRowPresenter: {0}, Columns: {1}",
                this.GetType(),
                (Content != null) ? Content.ToString() : String.Empty,
                (Columns != null) ? Columns.Count : 0);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        ///     The StyledProperty for the Content property.
        ///     Default Value:      null
        /// </summary>
        // Any change in Content properties affects layout measurement since
        // a new template may be used. On measurement,
        // ApplyTemplate will be invoked leading to possible application
        // of a new template.
        public static readonly StyledProperty<object> ContentProperty =
                ContentControl.ContentProperty.AddOwner<GridViewRowPresenter>();

        /// <summary>
        ///     Content is the data used to generate the child elements of this control.
        /// </summary>
        public object Content
        {
            get { return GetValue(GridViewRowPresenter.ContentProperty); }
            set { SetValue(GridViewRowPresenter.ContentProperty, value); }
        }

        /// <summary>
        ///     Called when ContentProperty is invalidated on "d."
        /// </summary>
        private static void OnContentChanged(GridViewRowPresenter sender, AvaloniaPropertyChangedEventArgs e)
        {
            //
            // If the old and new value have the same type then we can save a lot of perf by
            // keeping the existing ContentPresenters
            //

            Type oldType = e.OldValue?.GetType();
            Type newType = e.NewValue?.GetType();

            // DisconnectedItem doesn't count as a real type change
            if (e.NewValue == AvaloniaProperty.UnsetValue)
            {
                sender._oldContentType = oldType;
                newType = oldType;
            }
            else if (e.OldValue == AvaloniaProperty.UnsetValue)
            {
                oldType = sender._oldContentType;
            }

            if (oldType != newType)
            {
                sender.NeedUpdateVisualTree = true;
            }
            else
            {
                sender.UpdateCells();
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        // Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            GridViewColumnCollection columns = Columns;
            if (columns == null) { return new Size(); }

            AvaloniaList<Control> children = InternalChildren;
            double maxHeight = 0.0;           // Max height of children.
            double accumulatedWidth = 0.0;    // Total width consumed by children.
            double constraintHeight = availableSize.Height;
            bool desiredWidthListEnsured = false;

            foreach (GridViewColumn column in columns)
            {
                Control child = children[column.ActualIndex];
                if (child == null) { continue; }

                double childConstraintWidth = Math.Max(0.0, availableSize.Width - accumulatedWidth);

                if (column.State == ColumnMeasureState.Init
                    || column.State == ColumnMeasureState.Headered)
                {
                    if (!desiredWidthListEnsured)
                    {
                        EnsureDesiredWidthList();
                        LayoutUpdated += OnLayoutUpdated;
                        desiredWidthListEnsured = true;
                    }

                    // Measure child.
                    child.Measure(new Size(childConstraintWidth, constraintHeight));

                    // As long as this is the first round of measure that has data participate
                    // the width should be ensured
                    // only element on current page participates in calculating the shared width
                    if (IsOnCurrentPage)
                    {
                        column.EnsureWidth(child.DesiredSize.Width);
                    }

                    DesiredWidthList[column.ActualIndex] = column.DesiredWidth;

                    accumulatedWidth += column.DesiredWidth;
                }
                else if (column.State == ColumnMeasureState.Data)
                {
                    childConstraintWidth = Math.Min(childConstraintWidth, column.DesiredWidth);

                    child.Measure(new Size(childConstraintWidth, constraintHeight));

                    accumulatedWidth += column.DesiredWidth;
                }
                else // ColumnMeasureState.SpecificWidth
                {
                    childConstraintWidth = Math.Min(childConstraintWidth, column.Width);

                    child.Measure(new Size(childConstraintWidth, constraintHeight));

                    accumulatedWidth += column.Width;
                }

                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }

            // Reset this flag so that we will re-calculate it on every measure.
            _isOnCurrentPageValid = false;

            // reserve space for dummy header next to the last column
            accumulatedWidth += c_PaddingHeaderMinWidth;

            return new Size(accumulatedWidth, maxHeight);
        }

        /// <summary>
        /// GridViewRowPresenter computes the position of its children inside each child's Margin and calls Arrange
        /// on each child.
        /// </summary>
        /// <param name="finalSize">Size the GridViewRowPresenter will assume.</param>
        protected override Size ArrangeOverride(Size finalSize)
        {
            GridViewColumnCollection columns = Columns;
            if (columns == null) { return finalSize; }

            AvaloniaList<Control> children = InternalChildren;

            double accumulatedWidth = 0.0;
            double remainingWidth = finalSize.Width;

            foreach (GridViewColumn column in columns)
            {
                Control child = children[column.ActualIndex];
                if (child == null) { continue; }

                // has a given value or 'auto'
                double childArrangeWidth = Math.Min(remainingWidth, ((column.State == ColumnMeasureState.SpecificWidth) ? column.Width : column.DesiredWidth));

                child.Arrange(new Rect(accumulatedWidth, 0, childArrangeWidth, finalSize.Height));

                remainingWidth -= childArrangeWidth;
                accumulatedWidth += childArrangeWidth;
            }

            return finalSize;
        }

        #endregion Protected Methods

        //-------------------------------------------------------------------
        //
        // Internal Methods / Properties
        //
        //-------------------------------------------------------------------

        #region Internal Methods / Properties

        /// <summary>
        /// Called when the Template's tree has been generated
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            // +-- GridViewRowPresenter ------------------------------------+
            // |                                                            |
            // |  +- CtPstr1 ---+   +- CtPstr2 ---+   +- CtPstr3 ---+       |
            // |  |             |   |             |   |             |  ...  |
            // |  +-------------+   +-------------+   +-------------+       |
            // +------------------------------------------------------------+

            base.OnApplyTemplate(e);

            if (NeedUpdateVisualTree)
            {
                InternalChildren.Clear();

                // build the whole collection from draft.
                GridViewColumnCollection columns = Columns;
                if (columns != null)
                {
                    foreach (GridViewColumn column in columns)
                    {
                        InternalChildren.Add(CreateCell(column));
                    }
                }

                NeedUpdateVisualTree = false;
            }

            // invalidate viewPort cache
            _viewPortValid = false;
        }

        /// <summary>
        /// Handler of column's PropertyChanged event. Update correspondent property
        /// if change is of Width / CellTemplate / CellTemplateSelector.
        /// </summary>
        internal override void OnColumnPropertyChanged(GridViewColumn column, string propertyName)
        {
            Debug.Assert(column != null);
            int index;

            // ActualWidth change is a noise to RowPresenter, so filter it out.
            // Note-on-perf: ActualWidth property change of will fire N x M times
            // on every start up. (N: number of column with Width set to 'auto',
            // M: number of visible items)
            if (GridViewColumn.c_ActualWidthName.Equals(propertyName))
            {
                return;
            }

            // Width is the #1 property that will be changed frequently. The others
            // (DisplayMemberBinding/CellTemplate/Selector) are not.

            if (((index = column.ActualIndex) >= 0) && (index < InternalChildren.Count))
            {
                if (GridViewColumn.WidthProperty.Name.Equals(propertyName))
                {
                    InvalidateMeasure();
                }

                // Priority: DisplayMemberBinding > CellTemplate > CellTemplateSelector
                else if (GridViewColumn.c_DisplayMemberBindingName.Equals(propertyName))
                {
                    Control cell = InternalChildren[index];
                    if (cell != null)
                    {
                        IBinding binding = column.DisplayMemberBinding;
                        if (binding != null && cell is TextBlock)
                        {
                            cell.Bind(TextBlock.TextProperty, binding);
                        }
                        else
                        {
                            RenewCell(index, column);
                        }
                    }
                }
                else
                {
                    ContentPresenter cp = InternalChildren[index] as ContentPresenter;
                    if (cp != null)
                    {
                        if (GridViewColumn.CellTemplateProperty.Name.Equals(propertyName))
                        {
                            IDataTemplate dt;
                            if ((dt = column.CellTemplate) == null)
                            {
                                cp.ClearValue(ContentControl.ContentTemplateProperty);
                            }
                            else
                            {
                                cp.ContentTemplate = dt;
                            }
                        }

                        else if (GridViewColumn.CellTemplateSelectorProperty.Name.Equals(propertyName))
                        {
                            IDataTemplate dts;
                            if ((dts = column.CellTemplateSelector) == null)
                            {
                                cp.ClearValue(ContentPresenter.ContentTemplateProperty);
                            }
                            else
                            {
                                cp.ContentTemplate = dts;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// process GridViewColumnCollection.CollectionChanged event.
        /// </summary>
        internal override void OnColumnCollectionChanged(GridViewColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);

            if ( e.Action == NotifyCollectionChangedAction.Move)
            {
                InvalidateArrange();
            }
            else
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // New child will always be appended to the very last, no matter it
                        // is actually add via 'Insert' or just 'Add'.
                        InternalChildren.Add(CreateCell((GridViewColumn)(e.NewItems[0])));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        InternalChildren.RemoveAt(e.ActualIndex);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        InternalChildren.RemoveAt(e.ActualIndex);
                        InternalChildren.Add(CreateCell((GridViewColumn)(e.NewItems[0])));
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        InternalChildren.Clear();
                        break;

                    default:
                        break;
                }

                InvalidateMeasure();
            }
        }

        // Used in UIAutomation
        // Return the actual cells array (If user reorder column, the cell in InternalChildren isn't in the correct order)
        internal List<Control> ActualCells
        {
            get
            {
                List<Control> list = new List<Control>();
                GridViewColumnCollection columns = Columns;
                if (columns != null)
                {
                    AvaloniaList<Control> children = InternalChildren;
                    List<int> indexList = columns.IndexList;

                    if (children.Count == columns.Count)
                    {
                        for (int i = 0, count = columns.Count; i < count; ++i)
                        {
                            Control cell = children[indexList[i]];
                            if (cell != null)
                            {
                                list.Add(cell);
                            }
                        }
                    }
                }

                return list;
            }
        }

        #endregion Internal Methods / Properties

        //-------------------------------------------------------------------
        //
        // Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        private void FindViewPort()
        {
            // assume GridViewRowPresenter is in Item's template
            _viewItem = this.TemplatedParent as Control;

            if (_viewItem != null)
            {
                ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(_viewItem) as ItemsControl;

                if (itemsControl != null)
                {
                    ScrollViewer scrollViewer = itemsControl.FindDescendantOfType<ScrollViewer>();
                    if (scrollViewer != null)
                    {
                        // check if Virtualizing Panel do works
                        if (itemsControl.ItemsPanel is VirtualizingStackPanel &&
                            scrollViewer.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled ||
                            scrollViewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                        {
                            // find the 'PART_ScrollContentPresenter' in GridViewScrollViewer
                            _viewPort = scrollViewer.FindNameScope()?.Find("PART_ScrollContentPresenter") as Control;

                            // in case GridViewScrollViewer is re-styled, say, cannot find PART_ScrollContentPresenter
                            if (_viewPort == null)
                            {
                                _viewPort = scrollViewer;
                            }
                        }
                    }
                }
            }
        }

        private bool CheckVisibleOnCurrentPage()
        {
            if (!_viewPortValid)
            {
                FindViewPort();
            }

            bool result = true;

            if (_viewItem != null && _viewPort != null)
            {
                Rect viewPortBounds = new Rect(new Point(), _viewPort.Bounds.Size);
                Rect itemBounds = new Rect(new Point(), _viewItem.Bounds.Size);
                
                // In Avalonia, we use TranslatePoint instead of TransformToAncestor
                var transformedPoint = _viewItem.TranslatePoint(new Point(0, 0), _viewPort);
                if (transformedPoint.HasValue)
                {
                    itemBounds = itemBounds.WithX(transformedPoint.Value.X).WithY(transformedPoint.Value.Y);
                }

                // check if item bounds falls in view port bounds (in height)
                result = CheckContains(viewPortBounds, itemBounds);
            }

            return result;
        }

        private bool CheckContains(Rect container, Rect element)
        {
            // Check if ANY part of the element reside in container
            // return true if and only if (either case)
            //
            // +-------------------------------------------+
            // +  #================================#       +
            // +--#--------------------------------#-------+
            //    #                                #
            //    #                                #
            // +--#--------------------------------#-------+
            // +  #                                #       +
            // +--#--------------------------------#-------+
            //    #                                #
            //    #                                #
            // +--#--------------------------------#-------+
            // +  #================================#       +
            // +-------------------------------------------+

            // The tolerance here is to make sure at least 2 pixels are inside container
            const double tolerance = 2.0;

            return ((CheckIsPointBetween(container, element.Top) && CheckIsPointBetween(container, element.Bottom)) ||
                    CheckIsPointBetween(element, container.Top + tolerance) ||
                    CheckIsPointBetween(element, container.Bottom - tolerance));
        }

        private bool CheckIsPointBetween(Rect rect, double pointY)
        {
            // return rect.Top <= pointY <= rect.Bottom
            return (DoubleUtil.LessThanOrClose(rect.Top, pointY) &&
                    DoubleUtil.LessThanOrClose(pointY, rect.Bottom));
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            bool desiredWidthChanged = false; // whether the shared minimum width has been changed since last layout

            GridViewColumnCollection columns = Columns;
            if(columns != null)
            {
                foreach (GridViewColumn column in columns)
                {
                    if ((column.State != ColumnMeasureState.SpecificWidth))
                    {
                        column.State = ColumnMeasureState.Data;

                        if (DesiredWidthList == null || column.ActualIndex >= DesiredWidthList.Count)
                        {
                            // How can this happen?
                            // Between the last measure was called and this update is called, there can be a
                            // change done to the ColumnCollection and result in DesiredWidthList out of sync
                            // with the column collection. What can we do is end this call asap and the next
                            // measure will fix it.
                            desiredWidthChanged = true;
                            break;
                        }

                        if (!DoubleUtil.AreClose(column.DesiredWidth, DesiredWidthList[column.ActualIndex]))
                        {
                            // Update the record because collection operation later on might
                            // need to verified this list again, e.g. insert an 'auto'
                            // column, so that we won't trigger unnecessary update due to
                            // inconsistency of this column.
                            DesiredWidthList[column.ActualIndex] = column.DesiredWidth;

                            desiredWidthChanged = true;
                        }
                    }
                }
            }
            
            if (desiredWidthChanged)
            {
                InvalidateMeasure();
            }

            LayoutUpdated -= OnLayoutUpdated;
        }

        private Control CreateCell(GridViewColumn column)
        {
            Debug.Assert(column != null, "column shouldn't be null");

            Control cell;
            IBinding binding;

            // Priority: DisplayMemberBinding > CellTemplate > CellTemplateSelector

            if ((binding = column.DisplayMemberBinding) != null)
            {
                cell = new TextBlock
                {
                    // Needed this. Otherwise can't size to content at startup time.
                    // The reason is cell.Text is empty after the first round of measure.
                    DataContext = Content
                };

                cell.Bind(TextBlock.TextProperty, binding);
            }
            else
            {
                ContentPresenter cp = new ContentPresenter
                {
                    Content = Content
                };

                IDataTemplate dt;
                IDataTemplate dts;
                if ((dt = column.CellTemplate) != null)
                {
                    cp.ContentTemplate = dt;
                }
                if ((dts = column.CellTemplateSelector) != null)
                {
                    cp.ContentTemplate = dts;
                }

                cell = cp;
            }

            // copy alignment properties from ListViewItem
            // for perf reason, not use binding here
            ContentControl parent;
            if ((parent = TemplatedParent as ContentControl) != null)
            {
                cell.VerticalAlignment = parent.VerticalContentAlignment;
                cell.HorizontalAlignment = parent.HorizontalContentAlignment;
            }

            cell.Margin = _defaultCellMargin;

            return cell;
        }

        private void RenewCell(int index, GridViewColumn column)
        {
            InternalChildren.RemoveAt(index);
            InternalChildren.Insert(index, CreateCell(column));
        }


        /// <summary>
        /// Updates all cells to the latest Content.
        /// </summary>
        private void UpdateCells()
        {
            ContentPresenter cellAsCP;
            Control cell;
            AvaloniaList<Control> children = InternalChildren;
            ContentControl parent = TemplatedParent as ContentControl;

            for (int i = 0; i < children.Count; i++)
            {
                cell = children[i];

                if ((cellAsCP = cell as ContentPresenter) != null)
                {
                    cellAsCP.Content = Content;
                }
                else
                {
                    Debug.Assert(cell is TextBlock, "cells are either TextBlocks or ContentPresenters");
                    cell.DataContext = Content;
                }

                if (parent != null)
                {
                    cell.VerticalAlignment = parent.VerticalContentAlignment;
                    cell.HorizontalAlignment = parent.HorizontalContentAlignment;
                }
            }
        }


        #endregion

        //-------------------------------------------------------------------
        //
        // Private Properties / Fields
        //
        //-------------------------------------------------------------------

        #region Private Properties / Fields

        // if RowPresenter is not 'real' visible, it should not participating in measuring column width
        // NOTE: IsVisible is force-inheriting parent's value, that's why we pick IsVisible instead of Visibility
        //       e.g. if RowPresenter's parent is hidden/collapsed (e.g. in ListTreeView),
        //            then RowPresenter.Visibility = Visible, but RowPresenter.IsVisible = false
        private bool IsOnCurrentPage
        {
            get
            {
                if (!_isOnCurrentPageValid)
                {
                    _isOnCurrentPage = IsVisible && CheckVisibleOnCurrentPage();
                    _isOnCurrentPageValid = true;
                }

                return _isOnCurrentPage;
            }
        }

        private Control _viewPort;
        private Control _viewItem;
        private Type _oldContentType;
        private bool _viewPortValid = false;
        private bool _isOnCurrentPage = false;
        private bool _isOnCurrentPageValid = false;

        private static readonly Thickness _defaultCellMargin = new Thickness(6, 0, 6, 0);

        #endregion Private Properties / Fields

        static GridViewRowPresenter()
        {
            // Register property change handlers
            ContentProperty.Changed.AddClassHandler<GridViewRowPresenter>(OnContentChanged);
        }
    }
}
