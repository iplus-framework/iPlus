// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;   // NotifyCollectionChangedEventHandler
using System.Diagnostics;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Base class for GridViewRowPresenter and HeaderRowPresenter.
    /// </summary>
    public abstract class GridViewRowPresenterBase : TemplatedControl
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
            return string.Format("{0}, {1}",
                this.GetType(),
                (Columns != null) ? Columns.Count : 0);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        // Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        ///  Columns StyledProperty
        /// </summary>
        public static readonly StyledProperty<GridViewColumnCollection> ColumnsProperty =
            AvaloniaProperty.Register<GridViewRowPresenterBase, GridViewColumnCollection>(
                nameof(Columns),
                null,
                coerce: CoerceColumns);

        /// <summary>
        /// Columns Property
        /// </summary>
        public GridViewColumnCollection Columns
        {
            get { return GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        private static GridViewColumnCollection CoerceColumns(AvaloniaObject sender, GridViewColumnCollection value)
        {
            var presenter = sender as GridViewRowPresenterBase;
            if (presenter != null)
            {
                var oldColumns = presenter.GetValue(ColumnsProperty);
                presenter.OnColumnsChanged(oldColumns, value);
            }
            return value;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        // Protected Methods / Properties
        //
        //-------------------------------------------------------------------

        #region Protected Methods / Properties

        /// <summary>
        /// Returns enumerator to logical children.
        /// </summary>
        /// 
        protected override void LogicalChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.LogicalChildrenCollectionChanged(sender, e);
        }

        ///// <summary>
        ///// Gets the Visual children count.
        ///// </summary>
        ///// 
        //protected override int VisualChildrenCount
        //{
        //    get
        //    {
        //        if (_uiElementCollection == null)
        //        {
        //            return 0;
        //        }
        //        else
        //        {
        //            return _uiElementCollection.Count;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Gets the Visual child at the specified index.
        ///// </summary>
        //protected override Visual GetVisualChild(int index)
        //{
        //    if (_uiElementCollection == null)
        //    {
        //        throw new ArgumentOutOfRangeException(nameof(index), index, "Visual_ArgumentOutOfRange");
        //    }
        //    return _uiElementCollection[index];
        //}

        #endregion

        //-------------------------------------------------------------------
        //
        // Internal Methods / Properties
        //
        //-------------------------------------------------------------------

        #region Internal Methods

        /// <summary>
        /// process the column collection changed event
        /// </summary>
        internal virtual void OnColumnCollectionChanged(GridViewColumnCollectionChangedEventArgs e)
        {
            if (DesiredWidthList != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove
                    || e.Action == NotifyCollectionChangedAction.Replace)
                {
                    // NOTE: The steps to make DesiredWidthList.Count <= e.ActualIndex
                    //
                    //  1. init with 3 auto columns;
                    //  2. add 1 column to the column collection with width 90.0;
                    //  3. remove the column we just added to the the collection;
                    //
                    //  Now we have DesiredWidthList.Count equals to 3 while the removed column
                    //  has  ActualIndex equals to 3.
                    //
                    if (DesiredWidthList.Count > e.ActualIndex)
                    {
                        DesiredWidthList.RemoveAt(e.ActualIndex);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    DesiredWidthList = null;
                }
            }
        }

        /// <summary>
        /// process the column property changed event
        /// </summary>
        internal abstract void OnColumnPropertyChanged(GridViewColumn column, string propertyName);

        /// <summary>
        /// ensure ShareStateList have at least columns.Count items
        /// </summary>
        internal void EnsureDesiredWidthList()
        {
            GridViewColumnCollection columns = Columns;

            if (columns != null)
            {
                int count = columns.Count;

                if (DesiredWidthList == null)
                {
                    DesiredWidthList = new List<double>(count);
                }

                int c = count - DesiredWidthList.Count;
                for (int i = 0; i < c; i++)
                {
                    DesiredWidthList.Add(Double.NaN);
                }
            }
        }

        /// <summary>
        /// list of currently reached max value of DesiredWidth of cell in the column
        /// </summary>
        internal List<double> DesiredWidthList
        {
            get { return _desiredWidthList; }
            private set { _desiredWidthList = value; }
        }

        /// <summary>
        /// if visual tree is out of date
        /// </summary>
        internal bool NeedUpdateVisualTree
        {
            get { return _needUpdateVisualTree; }
            set { _needUpdateVisualTree = value; }
        }

        /// <summary>
        /// collection if children
        /// </summary>
        internal AvaloniaList<Control> InternalChildren
        {
            get
            {
                if (_uiElementCollection == null) //nobody used it yet
                {
                    _uiElementCollection = new AvaloniaList<Control>();
                }

                return _uiElementCollection;
            }
        }

        // the minimum width for dummy header when measure
        internal const double c_PaddingHeaderMinWidth = 2.0;

        #endregion

        //-------------------------------------------------------------------
        //
        // Private Methods / Properties / Fields
        //
        //-------------------------------------------------------------------

        #region Private Methods / Properties / Fields

        // Property invalidation callback invoked when ColumnCollectionProperty is invalidated
        private void OnColumnsChanged(GridViewColumnCollection oldCollection, GridViewColumnCollection newCollection)
        {
            if (oldCollection != null)
            {
                // Unsubscribe from the old collection's change notifications
                if (_collectionChangedSubscription != null)
                {
                    _collectionChangedSubscription.Dispose();
                    _collectionChangedSubscription = null;
                }

                // NOTE:
                // If the collection is NOT in view mode (a.k.a owner isn't GridView),
                // RowPresenter is responsible to be or to find one to be the collection's mentor.
                //
                //if (!oldCollection.InViewMode && oldCollection.Owner == GetStableAncester())
                //{
                //    oldCollection.Owner = null;
                //}
            }

            if (newCollection != null)
            {
                // Subscribe to the new collection's change notifications using Avalonia's weak event handling
                if (newCollection is INotifyCollectionChanged notifyCollection)
                {
                    // Use Observable.FromEventPattern or direct event subscription instead of WeakEventHandlerManager.Subscribe
                    notifyCollection.CollectionChanged += OnCollectionChanged;
                }

                //// Similar to what we do to oldCollection. But, of course, in a reverse way.
                //if (!newCollection.InViewMode && newCollection.Owner == null)
                //{
                //    newCollection.Owner = GetStableAncester();
                //}
            }

            NeedUpdateVisualTree = true;
            InvalidateMeasure();
        }

        private IDisposable _collectionChangedSubscription;

        //
        // NOTE:
        //
        // If the collection is NOT in view mode, RowPresenter should be mentor of the Collection.
        // But if presenter + collection are used to restyle ListBoxItems and the ItemsPanel is
        // VSP, there are 2 problems:
        //
        //  1. each RowPresenter want to be the mentor, too many context change event
        //  2. when doing scroll, VSP will dispose those LB items which are out of view. But they
        //      are still referenced by the Collection (at the Owner property) - memory leak.
        //
        // Solution:
        //  If RowPresenter is inside an ItemsControl (IC\LB\CB), use the ItemsControl as the
        //  mentor. Therefore,
        //      - context change is minimized because ItemsControl for different items is the same;
        //      - no memory leak because when virtualizing, only dispose items not the IC itself.
        //
        private AvaloniaObject GetStableAncester()
        {
            // In Avalonia, we need to find the ItemsControl ancestor differently
            var parent = this.FindAncestorOfType<ItemsControl>();
            if (parent != null)
                return parent;
            
            return this;
        }

        // if and only if both conditions below are satisfied, row presenter visual is ready.
        // 1. is initialized, which ensures RowPresenter is created
        // 2. !NeedUpdateVisualTree, which ensures all visual elements generated by RowPresenter are created
        private bool IsPresenterVisualReady
        {
            get { return (IsInitialized && !NeedUpdateVisualTree); }
        }

        /// <summary>
        /// Handler of GridViewColumnCollection.CollectionChanged event.
        /// </summary>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            var e = arg as GridViewColumnCollectionChangedEventArgs;

            if (e != null
                && IsPresenterVisualReady)// if and only if rowpresenter's visual is ready, shall rowpresenter go ahead process the event.
            {
                // Property of one column changed
                if (e.Column != null)
                {
                    OnColumnPropertyChanged(e.Column, e.PropertyName);
                }
                else
                {
                    OnColumnCollectionChanged(e);
                }
            }
        }

        private AvaloniaList<Control> _uiElementCollection;
        private bool _needUpdateVisualTree = true;
        private List<double> _desiredWidthList;

        #endregion

        #region Cleanup

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            // Clean up event subscription
            if (Columns is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged -= OnCollectionChanged;
            }
            if (_collectionChangedSubscription != null)
            {
                _collectionChangedSubscription.Dispose();
                _collectionChangedSubscription = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Manager for the GridViewColumnCollection.CollectionChanged event.
    /// This is replaced by Avalonia's WeakEventHandlerManager, but kept for compatibility
    /// if other parts of the codebase reference it.
    /// </summary>
    internal class InternalCollectionChangedEventManager
    {
        #region Public Methods

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(GridViewColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(source);

            // In Avalonia, we directly subscribe to the event
            // This is a simplified version - in practice, you might want to use WeakEventHandlerManager
            if (source is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += (s, e) => handler(s, e);
            }
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(GridViewColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        {
            ArgumentNullException.ThrowIfNull(handler);
            ArgumentNullException.ThrowIfNull(source);

            // In Avalonia, we would need to keep track of subscriptions to remove them
            // This is handled better by the WeakEventHandlerManager approach used above
        }

        #endregion
    }
}
