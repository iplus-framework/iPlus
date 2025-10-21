// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using System;
using System.Collections.Specialized;
using System.Linq;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// A general purpose control for data presentation as part of the set of common controls provided with Avalon.
    ///    Drop a control into a layout;
    ///    Enable application developers to display data efficiently;
    ///    Allow the presentation of data to be styled, including the layout and the item visuals;
    ///    No type-specific functionality.
    ///
    /// ListView is a control which has
    ///    A data collection;
    ///    A set of predefined operations to manipulate the data/view.
    /// Also, ListView is a control for the most convenient browsing of data.
    /// </summary>


    public class ListView : ListBox
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        public ListView()
            : base()
        {
            if (Items != null)
                Items.CollectionChanged += Items_CollectionChanged; 
        }

        #endregion Constructors

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        /// View DependencyProperty
        /// </summary>
        public static readonly StyledProperty<ViewBase> ViewProperty = AvaloniaProperty.Register<VBTreeView, ViewBase>(nameof(View));

        /// <summary>
        /// descriptor of the whole view. Include chrome/layout/item/...
        /// </summary>
        public ViewBase View
        {
            get { return (ViewBase)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        private void OnViewChanged(AvaloniaPropertyChangedEventArgs change)
        {
            ViewBase oldView = (ViewBase)change.OldValue;
            ViewBase newView = (ViewBase)change.NewValue;
            if (newView != null)
            {
                if (newView.IsUsed)
                {
                    throw new InvalidOperationException("ListView_ViewCannotBeShared");
                }
                newView.IsUsed = true;
            }

            // In ApplyNewView ListView.ClearContainerForItemOverride will be called for each item.
            // Should use old view to do clear item.
            this._previousView = oldView;
            this.ApplyNewView();
            // After ApplyNewView, if item is removed, ListView.ClearContainerForItemOverride will be called.
            // Then should use new view to do clear item.
            this._previousView = newView;

            //Switch ViewAutomationPeer in ListViewAutomationPeer
            ListViewAutomationPeer lvPeer = ControlAutomationPeer.FromElement(this) as ListViewAutomationPeer;
            if (lvPeer != null)
            {
                lvPeer.ViewAutomationPeer?.ViewDetached();

                if (newView != null)
                {
                    lvPeer.ViewAutomationPeer = newView.GetAutomationPeer(this) as IViewAutomationPeer;
                }
                else
                {
                    lvPeer.ViewAutomationPeer = null;
                }
                //Invalidate the ListView automation tree because the view has been changed
                lvPeer.BringIntoView();
            }

            oldView.IsUsed = false;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Prepare the element to display the item. Override the default style
        /// if new view is a GridView and no ItemContainerStyle provided.
        /// Will call View.PrepareItem() to allow view do preparison for item.
        /// </summary>
        protected override void PrepareContainerForItemOverride(Control container, object item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);

            ListViewItem lvi = container as ListViewItem;
            if (lvi != null)
            {
                ViewBase view = View;
                if (view != null)
                {
                    // update default style key
                    lvi.SetDefaultStyleKey(view.ItemContainerDefaultStyleKey as Type);
                    view.PrepareItem(lvi);
                }
                else
                {
                    lvi.ClearDefaultStyleKey();
                }
            }
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return new ListViewItem();
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<ListViewItem>(item, out recycleKey);
        }



        protected virtual void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListViewAutomationPeer lvPeer = ControlAutomationPeer.FromElement(this) as ListViewAutomationPeer;
            if (lvPeer != null && lvPeer.ViewAutomationPeer != null)
            {
                lvPeer.ViewAutomationPeer.ItemsChanged(e);
            }
        }

        #endregion // Protected Methods

        //-------------------------------------------------------------------
        //
        //  Accessibility
        //
        //-------------------------------------------------------------------

        #region Accessibility

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            ListViewAutomationPeer lvPeer = new ListViewAutomationPeer(this);
            if (lvPeer != null && View != null)
            {
                lvPeer.ViewAutomationPeer = View.GetAutomationPeer(this) as IViewAutomationPeer;
            }

            return lvPeer;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        ViewBase _newView;
        // apply styles described in View.
        private void ApplyNewView()
        {
            _newView = View;

            //if (_newView != null)
            //{
            //    // update default style key of ListView
            //    StyleKeyOverride = newView.DefaultStyleKey;
            //}
            //else
            //{
            //    ClearValue(DefaultStyleKeyProperty);
            //}

            // Encounter a new view after loaded means user is switching view.
            // Force to regenerate all containers.
            if (IsLoaded)
            {
                // Doesn't exist in Avalonia
                // ItemContainerGenerator.Refresh();
            }
        }

        protected override Type StyleKeyOverride
        {
            get
            {
                if (_newView != null)
                {
                    return _newView.DefaultStyleKey as Type;
                }
                else
                {
                    return base.StyleKeyOverride;
                }
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ThemeProperty)
            {
                // If the ListView does not have a template generated tree then its
                // View.Header is not reachable via a tree walk.
                if (VisualChildren != null && VisualChildren.Any() && View != null)
                {
                    View.OnThemeChanged();
                }
            }
            else if (change.Property == ViewProperty)
            {
                OnViewChanged(change);
            }
            base.OnPropertyChanged(change);
        }

        #endregion Private Methods

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        private ViewBase _previousView;
    }
}
