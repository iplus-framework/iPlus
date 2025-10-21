// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    public interface IViewAutomationPeer
    {
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //AutomationControlType GetAutomationControlType();

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="patternInterface"></param>
        ///// <returns></returns>
        //object GetPattern(PatternInterface patternInterface);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<AutomationPeer> GetChildren(IReadOnlyList<AutomationPeer> children);

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="item"></param>
        ///// <returns></returns>
        //ItemAutomationPeer CreateItemAutomationPeer(object item);

        /// <summary>
        /// ListView will call this method when items is changed
        /// </summary>
        /// <param name="e"></param>
        //Note: The following two reasons explain why we need the ItemsChanged method
        //      1 View must know when Items has been changed in order to fire event when IGridProvider.RowCount is changed
        //      2 ItemsControl doesn't fire a ItemsChanged event, the only way to do this is to override the OnItemsChanged event
        void ItemsChanged(NotifyCollectionChangedEventArgs e);

        /// <summary>
        /// ListView will call this method when the view is detached from it
        /// </summary>
        void ViewDetached();
    }

    /// <summary>
    /// GridView automation peer
    /// </summary>
    /// <remarks>
    /// Basically, the idea is to add a virtual method called CreateAutomationPeer on ViewBase
    /// Any view can override this method to create its own automation peer.
    /// ListView will use this method to get an automation peer for a given view and default to
    /// the properties/methods/patterns implemented by the view before going to default fall-backs on it
    /// These view automation peer must implement IViewAutomationPeer interface
    /// </remarks>
    public class GridViewAutomationPeer : ControlAutomationPeer, IViewAutomationPeer
    {
        public GridViewAutomationPeer(GridView owner, ListView listview)
            : base(owner)
        {

            _owner = owner;
            _listview = listview;

            //Remember the items/columns count when GVAP is created, this is used for firing RowCount/ColumnCount changed event
            _oldItemsCount = _listview.Items.Count;
            _oldColumnsCount = _owner.Columns.Count;

            ((INotifyCollectionChanged)_owner.Columns).CollectionChanged += new NotifyCollectionChangedEventHandler(OnColumnCollectionChanged);
        }


        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataGrid;
        }

        private ListView _listview;
        private GridView _owner;
        private int _oldItemsCount = 0;
        private int _oldColumnsCount = 0;


        List<AutomationPeer> IViewAutomationPeer.GetChildren(IReadOnlyList<AutomationPeer> children)
        {
            List<AutomationPeer> childrenCopy = children.ToList();
            //Add GridViewHeaderRowPresenter as the first child of ListView
            if (_owner.HeaderRowPresenter != null)
            {
                AutomationPeer peer = ControlAutomationPeer.CreatePeerForElement(_owner.HeaderRowPresenter);
                if (peer != null)
                {
                    //If children is null, we still need to create an empty list to insert HeaderRowPresenter
                    if (childrenCopy == null)
                    {
                        childrenCopy = new List<AutomationPeer>();
                    }

                    childrenCopy.Insert(0, peer);
                }
            }
            return childrenCopy;
        }

        void IViewAutomationPeer.ItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ListViewAutomationPeer peer = ControlAutomationPeer.FromElement(_listview) as ListViewAutomationPeer;
            if (peer != null)
            {
                if (_oldItemsCount != _listview.Items.Count)
                {
                    //peer.RaisePropertyChangedEvent(GridPatternIdentifiers.RowCountProperty, _oldItemsCount, _listview.Items.Count);
                }
                _oldItemsCount = _listview.Items.Count;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        void IViewAutomationPeer.ViewDetached()
        {
            ((INotifyCollectionChanged)_owner.Columns).CollectionChanged -= new NotifyCollectionChangedEventHandler(OnColumnCollectionChanged);
        }

        private void OnColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_oldColumnsCount != _owner.Columns.Count)
            {
                ListViewAutomationPeer peer = ControlAutomationPeer.FromElement(_listview) as ListViewAutomationPeer;
                //peer?.RaisePropertyChangedEvent(GridPatternIdentifiers.ColumnCountProperty, _oldColumnsCount, _owner.Columns.Count);
            }

            _oldColumnsCount = _owner.Columns.Count;

            ListViewAutomationPeer lvPeer = ControlAutomationPeer.FromElement(_listview) as ListViewAutomationPeer;
            if (lvPeer != null)
            {
                IReadOnlyList<AutomationPeer> list = lvPeer.GetChildrenCorePublic();
                if (list != null)
                {
                    foreach (AutomationPeer peer in list)
                    {
                        peer.BringIntoView();
                    }
                }
            }
        }

    }
}


