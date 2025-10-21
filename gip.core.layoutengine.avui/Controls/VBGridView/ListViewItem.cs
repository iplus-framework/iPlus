// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia.Controls;
using System;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    ///     Control that implements a selectable item inside a ListView.
    /// </summary>

    public class ListViewItem : ListBoxItem
    {
        // NOTE: ListViewItem has no default theme style. It uses ThemeStyleKey 
        // to find default style for different view.

        protected override Type StyleKeyOverride
        {
            get
            {
                if (_DefaultStyleKey != null)
                {
                    return _DefaultStyleKey;
                }
                else
                {
                    return base.StyleKeyOverride;
                }
            }
        }
        // helper to set DefaultStyleKey of ListViewItem

        private Type _DefaultStyleKey;
        internal void SetDefaultStyleKey(Type key)
        {
            _DefaultStyleKey = key;
        }

        ////  helper to clear DefaultStyleKey of ListViewItem
        internal void ClearDefaultStyleKey()
        {
            _DefaultStyleKey = null;
        }
    }
}
