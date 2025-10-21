// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia.Automation.Peers;
using System.Collections.Generic;


namespace gip.core.layoutengine.avui
{
    public class ListViewAutomationPeer : SelectingItemsControlAutomationPeer
    {
        public ListViewAutomationPeer(ListView owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return "ListView";
        }

        protected override bool IsContentElementCore() => false;

        protected override bool IsControlElementCore() => true;

        protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore()
        {
            return base.GetOrCreateChildrenCore();
        }

        protected override IReadOnlyList<AutomationPeer> GetChildrenCore()
        {
            if (_refreshItemPeers)
            {
                _refreshItemPeers = false;
                //ItemPeers.Clear();
            }

            IReadOnlyList<AutomationPeer> ret = base.GetChildrenCore();

            if (_viewAutomationPeer != null)
            {
                //If a custom view doesn't want to implement GetChildren details
                //just return null, we'll use the base.GetChildren as the return value
                ret = _viewAutomationPeer.GetChildren(ret);
            }

            return ret;
        }

        public IReadOnlyList<AutomationPeer> GetChildrenCorePublic()
        {
            return GetChildrenCore();
        }

        private bool _refreshItemPeers = false;
        private IViewAutomationPeer _viewAutomationPeer;
        protected internal IViewAutomationPeer ViewAutomationPeer
        {
            // Note: see bug 1555137 for details.
            // Never inline, as we don't want to unnecessarily link the 
            // automation DLL via the ISelectionProvider interface type initialization.
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            get { return _viewAutomationPeer; }
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            set
            {
                if (_viewAutomationPeer != value)
                {
                    _refreshItemPeers = true;
                }
                _viewAutomationPeer = value;
            }
        }
    }
}


