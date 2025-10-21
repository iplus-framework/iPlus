// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    public class GridViewItemAutomationPeer : ListItemAutomationPeer
    {
        public GridViewItemAutomationPeer(ContentControl owner) : base(owner)
        {
        }

        //public GridViewItemAutomationPeer(object owner, ListViewAutomationPeer listviewAP)
        //    : base(owner, listviewAP)
        //{
        //    Invariant.Assert(listviewAP != null);

        //    _listviewAP = listviewAP;
        //}

        protected override string GetClassNameCore()
        {
            return "ListViewItem";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataItem;
        }

        //protected override List<AutomationPeer> GetChildrenCore()
        //{
        //    ListView listview = _listviewAP.Owner as ListView;
        //    Invariant.Assert(listview != null);
        //    object item = Item;

        //    ListViewItem lvi = listview.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
        //    if (lvi != null)
        //    {
        //        GridViewRowPresenter rowPresenter = GridViewAutomationPeer.FindVisualByType(lvi, typeof(GridViewRowPresenter)) as GridViewRowPresenter;
        //        if (rowPresenter != null)
        //        {
        //            Hashtable oldChildren = _dataChildren; //cache the old ones for possible reuse
        //            _dataChildren = new Hashtable(rowPresenter.ActualCells.Count);

        //            List<AutomationPeer> list = new List<AutomationPeer>();
        //            int row = listview.ItemContainerGenerator.IndexFromContainer(lvi);
        //            int column = 0;

        //            foreach (UIElement ele in rowPresenter.ActualCells)
        //            {
        //                GridViewCellAutomationPeer peer = (oldChildren == null ? null : (GridViewCellAutomationPeer)oldChildren[ele]);
        //                if (peer == null)
        //                {
        //                    if (ele is ContentPresenter)
        //                    {
        //                        peer = new GridViewCellAutomationPeer((ContentPresenter)ele, _listviewAP);
        //                    }
        //                    else if (ele is TextBlock)
        //                    {
        //                        peer = new GridViewCellAutomationPeer((TextBlock)ele, _listviewAP);
        //                    }
        //                    else
        //                    {
        //                        Invariant.Assert(false, "Children of GridViewRowPresenter should be ContentPresenter or TextBlock");
        //                    }
        //                }

        //                //protection from indistinguishable UIElement - for example, 2 UIElement wiht same value
        //                if (_dataChildren[ele] == null)
        //                {
        //                    //Set Cell's row and column
        //                    peer.Column = column;
        //                    peer.Row = row;
        //                    list.Add(peer);
        //                    _dataChildren.Add(ele, peer);
        //                    column++;
        //                }
        //            }
        //            return list;
        //        }
        //    }

        //    return null;
        //}

        #region Private Fields

        //private ListViewAutomationPeer _listviewAP;
        //private Hashtable _dataChildren = null;

        #endregion
    }
}
