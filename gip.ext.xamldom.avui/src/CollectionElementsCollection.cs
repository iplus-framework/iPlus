// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls.Documents;
using System.Linq;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// The collection used by XamlProperty.CollectionElements
    /// </summary>
    sealed class CollectionElementsCollection : Collection<XamlPropertyValue>, INotifyCollectionChanged
    {
        XamlProperty property;
        bool isClearing = false;

        internal CollectionElementsCollection(XamlProperty property)
        {
            this.property = property;
        }

        /// <summary>
        /// Used by parser to construct the collection without changing the XmlDocument.
        /// </summary>
        internal void AddInternal(XamlPropertyValue value)
        {
            base.InsertItem(this.Count, value);
        }

        protected override void ClearItems()
        {
            isClearing = true;
            try
            {
                while (Count > 0)
                {
                    RemoveAt(Count - 1);
                }
            }
            finally
            {
                isClearing = false;
            }

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected override void RemoveItem(int index)
        {
            XamlPropertyInfo info = property.propertyInfo;
            object collection = info.GetValue(property.ParentObject.Instance);
            if (!CollectionSupport.RemoveItemAt(info.ReturnType, collection, index))
            {
                var propertyValue = this[index];
                if (collection is InlineCollection ilc)
                {
                    CollectionSupport.RemoveItem(info.ReturnType, collection, ilc.ElementAt(index), propertyValue);
                }
                else
                    CollectionSupport.RemoveItem(info.ReturnType, collection, propertyValue.GetValueFor(info), propertyValue);
            }

            var item = this[index];
            item.RemoveNodeFromParent();
            item.ParentProperty = null;
            base.RemoveItem(index);

            if (CollectionChanged != null && !isClearing)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        protected override void InsertItem(int index, XamlPropertyValue item)
        {
            XamlPropertyInfo info = property.propertyInfo;
            object collection = info.GetValue(property.ParentObject.Instance);
            if (!CollectionSupport.TryInsert(info.ReturnType, collection, item, index))
            {
                CollectionSupport.AddToCollection(info.ReturnType, collection, item);
            }

            item.ParentProperty = property;
            property.InsertNodeInCollection(item.GetNodeForCollection(), index);

            base.InsertItem(index, item);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        protected override void SetItem(int index, XamlPropertyValue item)
        {
            var oldItem = this[index];
            RemoveItem(index);
            InsertItem(index, item);

            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        #region INotifyCollectionChanged implementation

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}
