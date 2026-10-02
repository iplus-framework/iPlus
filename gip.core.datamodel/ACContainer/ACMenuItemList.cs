// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMenuItemList.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    //[DataContract]
    /// <summary>
    /// Class ACMenuItemList
    /// </summary>
    /// <remarks>
    /// Implements INotifyCollectionChanged so that WPF/Avalonia ItemsControls bound to this
    /// list (e.g. VBTreeView via VBChilds="Items") refresh automatically when menu entries
    /// are added/removed at runtime. The base class stays List&lt;ACMenuItem&gt; so the
    /// DataContractSerializer contract (ACClassDesign.MenuEntry) is unchanged and existing
    /// serialized menus keep deserializing. Note: the mutation methods are hidden with "new"
    /// instead of overridden because List&lt;T&gt; declares them non-virtual. Code that calls
    /// them through a List&lt;ACMenuItem&gt; or ICollection&lt;T&gt; reference bypasses the
    /// notifications — always mutate through an ACMenuItemList-typed reference (as
    /// ACMenuItem.Items does).
    /// </remarks>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMenuItemList'}de{'ACMenuItemList'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMenuItemList : List<ACMenuItem>, INotifyCollectionChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACMenuItemList"/> class.
        /// </summary>
        public ACMenuItemList()
        {
        }

        /// <summary>
        /// Occurs when the list changes (add/remove/move/replace/reset).
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Adds an item and raises CollectionChanged.</summary>
        public new void Add(ACMenuItem item)
        {
            int index = Count;
            base.Add(item);
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>Adds a range of items and raises CollectionChanged.</summary>
        public new void AddRange(IEnumerable<ACMenuItem> collection)
        {
            if (collection == null)
                return;
            int index = Count;
            List<ACMenuItem> added = new List<ACMenuItem>();
            foreach (ACMenuItem item in collection)
            {
                base.Add(item);
                added.Add(item);
            }
            if (added.Count > 0)
                Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, index));
        }

        /// <summary>Inserts an item and raises CollectionChanged.</summary>
        public new void Insert(int index, ACMenuItem item)
        {
            base.Insert(index, item);
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>Inserts a range of items and raises CollectionChanged.</summary>
        public new void InsertRange(int index, IEnumerable<ACMenuItem> collection)
        {
            if (collection == null)
                return;
            List<ACMenuItem> added = new List<ACMenuItem>(collection);
            base.InsertRange(index, added);
            if (added.Count > 0)
                Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, index));
        }

        /// <summary>Removes an item and raises CollectionChanged.</summary>
        public new bool Remove(ACMenuItem item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            base.RemoveAt(index);
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return true;
        }

        /// <summary>Removes the item at the given index and raises CollectionChanged.</summary>
        public new void RemoveAt(int index)
        {
            ACMenuItem item = this[index];
            base.RemoveAt(index);
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>Removes all items matching the predicate and raises CollectionChanged.</summary>
        public new int RemoveAll(Predicate<ACMenuItem> match)
        {
            int count = base.RemoveAll(match);
            if (count > 0)
                Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return count;
        }

        /// <summary>Removes a range of items and raises CollectionChanged.</summary>
        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>Clears the list and raises CollectionChanged.</summary>
        public new void Clear()
        {
            base.Clear();
            Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>Gets or sets the item at the given index; raises CollectionChanged on set.</summary>
        public new ACMenuItem this[int index]
        {
            get { return base[index]; }
            set
            {
                ACMenuItem oldItem = base[index];
                base[index] = value;
                Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
            }
        }

        private void Raise(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }
    }
}
