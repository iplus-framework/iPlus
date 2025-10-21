// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    /// 
    public class GridViewCellAutomationPeer : ControlAutomationPeer
    {
        public GridViewCellAutomationPeer(Control owner) : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return Owner.GetType().Name;
        }

        ///
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            if (Owner is TextBlock)
            {
                return AutomationControlType.Text;
            }
            else
            {
                return AutomationControlType.Custom;
            }
        }


    }
}
