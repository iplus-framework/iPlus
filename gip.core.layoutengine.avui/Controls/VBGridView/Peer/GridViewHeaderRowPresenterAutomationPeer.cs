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
    /// 
    public class GridViewHeaderRowPresenterAutomationPeer : ControlAutomationPeer
    {
        ///
        public GridViewHeaderRowPresenterAutomationPeer(GridViewHeaderRowPresenter owner)
            : base(owner)
        {
        }

        ///
        protected override string GetClassNameCore()
        {
            return "GridViewHeaderRowPresenter";
        }

        ///
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Header;
        }

        protected override bool IsContentElementCore() => false;

        protected override bool IsControlElementCore() => true;

        ///
        //protected override List<AutomationPeer> GetChildrenCore()
        //{
        //    List<AutomationPeer> list = base.GetChildrenCore();
        //    List<AutomationPeer> newList = null;
        //    if (list != null) 
        //    {
        //        newList = new List<AutomationPeer>(list.Count);
        //        //GVHRP contains 2 extra column headers, one is dummy header, the other is floating header
        //        //We need to remove them from the tree
        //        foreach (AutomationPeer peer in list)
        //        {
        //            if (peer is UIElementAutomationPeer)
        //            {
        //                GridViewColumnHeader header = ((UIElementAutomationPeer)peer).Owner as GridViewColumnHeader;
        //                if (header != null && header.Role == GridViewColumnHeaderRole.Normal)
        //                {
        //                    //Because GVHRP uses inverse sequence to store column headers, we need to use insert here
        //                    newList.Insert(0, peer);
        //                }
        //            }
        //        }
        //    }
        //    return newList;
        //}
    }
}
