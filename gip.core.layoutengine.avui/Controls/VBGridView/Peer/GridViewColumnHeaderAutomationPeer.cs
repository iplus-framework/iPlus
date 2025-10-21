// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Media;

namespace gip.core.layoutengine.avui
{
    /// 
    public class GridViewColumnHeaderAutomationPeer : ContentControlAutomationPeer, IInvokeProvider
    {
        ///
        public GridViewColumnHeaderAutomationPeer(GridViewColumnHeader owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.HeaderItem;
        }


        protected override bool IsContentElementCore() => false;

        protected override bool IsControlElementCore() => true;

        protected override string GetClassNameCore()
        {
            return "GridViewColumnHeader";
        }

        void IInvokeProvider.Invoke()
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            GridViewColumnHeader owner = (GridViewColumnHeader)Owner;
            owner.AutomationClick();
        }

    }
}
