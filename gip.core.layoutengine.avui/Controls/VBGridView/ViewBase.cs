// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Styling;
using System.ComponentModel;            // DesignerSerializationVisibility


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// ViewBase is something that tells the ListView the way to present each 
    /// entity in the data collection, i.e. the default style key.
    /// </summary>

    public abstract class ViewBase : Control
    {
        #region Protected Methods

        /// <summary>
        /// called when ListView is prepare container for item
        /// </summary>
        /// <param name="item">the container</param>
        protected internal virtual void PrepareItem(ListViewItem item)
        {
        }

        /// <summary>
        /// called when ListView is clear container for item
        /// </summary>
        /// <param name="item">the container</param>
        protected internal virtual void ClearItem(ListViewItem item)
        {
        }

        /// <summary>
        /// default style key. 
        /// ListView will degrate to ListBox if sub-class doesn't override 
        /// this value.
        /// </summary>
        protected internal virtual object DefaultStyleKey
        {
            get { return typeof(ListBox); }
        }

        /// <summary>
        /// default container style key
        /// The container, ListViewItem, will degrate to ListBoxItem if 
        /// sub-class doesn't override this value.
        /// </summary>
        protected internal virtual object ItemContainerDefaultStyleKey
        {
            get { return typeof(ListBoxItem); }
        }

        // Propagate theme changes to contained headers
        internal virtual void OnThemeChanged()
        {
        }

        #endregion

        /// <summary>
        /// called when ListView creates its Automation peer
        /// </summary>
        /// <param name="parent">listview reference</param>
        /// <returns>IViewAutomationPeer</returns>
        /// <remarks>ListView will use this method to get an automationPeer for a given view 
        /// and default to the properties/patterns implemented by the view before going to 
        /// default fall-backs on ListView.</remarks>
        protected internal virtual AutomationPeer GetAutomationPeer(ListView parent)
        {
            return null;
        }

        // True, when view is assigned to a ListView.
        internal bool IsUsed
        {
            get { return _isUsed; }
            set { _isUsed = value; }
        }

        private bool _isUsed;
    }
}
