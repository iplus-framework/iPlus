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
// <copyright file="PropertyLogRing.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;


namespace gip.core.datamodel
{
    /// <summary>
    /// Class PropertyLogRing
    /// </summary>
    public class PropertyLogRing : PropertyLogRing<PropertyLogItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyLogRing"/> class.
        /// </summary>
        public PropertyLogRing()
            : this(500)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyLogRing"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public PropertyLogRing(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddValue(object value)
        {
            base.Add(new PropertyLogItem() { Time = DateTime.Now, Value = value });
        }
    }

    /// <summary>
    /// Class PropertyLogRing
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class PropertyLogRing<T> : INotifyCollectionChanged, IList<T>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyLogRing{T}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
		public PropertyLogRing(int capacity)
		{
			this._Capacity = capacity;
			_Array = new T[capacity];
		}

        /// <summary>
        /// Fügt der <see cref="T:System.Collections.Generic.ICollection`1" /> ein Element hinzu.
        /// </summary>
        /// <param name="item">Das Objekt, das <see cref="T:System.Collections.Generic.ICollection`1" /> hinzugefügt werden soll.</param>
		public void Add(T item)
		{
			int index = (startIndex + _Count) % _Capacity;
			if (startIndex + _Count >= _Capacity)
			{
				startIndex++;
			}
			else
			{
				_Count++;
			}

			_Array[index] = item;

			CollectionChanged.RaiseAdd(this,item,index);
		}

        /// <summary>
        /// Ruft das Element am angegebenen Index ab oder legt dieses fest.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>`0.</returns>
		public T this[int index]
		{
			get { return _Array[(startIndex + index) % _Capacity]; }
			set
			{
				_Array[(startIndex + index) % _Capacity] = value;
				CollectionChanged.Raise(this);
			}
		}

        /// <summary>
        /// Entfernt alle Elemente aus <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
		public void Clear()
		{
			_Count = 0;
			startIndex = 0;
            var tempArray = _Array;
			_Array = new T[_Capacity];
            CollectionChanged.RaiseRemove(this,tempArray);
        }

        /// <summary>
        /// Gibt einen Enumerator zurück, der die Auflistung durchläuft.
        /// </summary>
        /// <returns>Ein <see cref="T:System.Collections.Generic.IEnumerator`1" />, der zum Durchlaufen der Auflistung verwendet werden kann.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < _Count; i++)
			{
				yield return this[i];
			}
		}

        /// <summary>
        /// The count
        /// </summary>
		private int _Count;
        /// <summary>
        /// Ruft die Anzahl der Elemente ab, die in <see cref="T:System.Collections.Generic.ICollection`1" /> enthalten sind.
        /// </summary>
        /// <value>The count.</value>
        /// <returns>Die Anzahl der Elemente, die in <see cref="T:System.Collections.Generic.ICollection`1" /> enthalten sind.</returns>
		public int Count
		{
			get { return _Count; }
		}

        /// <summary>
        /// The array
        /// </summary>
		private T[] _Array;

        /// <summary>
        /// The capacity
        /// </summary>
		private int _Capacity;
        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>The capacity.</value>
		public int Capacity
		{
			get { return _Capacity; }
		}

        /// <summary>
        /// The start index
        /// </summary>
		private int startIndex = 0;

		#region INotifyCollectionChanged Members

        /// <summary>
        /// Tritt ein, wenn die Auflistung geändert wird.
        /// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region IList<T> Members

        /// <summary>
        /// Bestimmt den Index eines bestimmten Elements in der <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">Das im <see cref="T:System.Collections.Generic.IList`1" /> zu suchende Objekt.</param>
        /// <returns>Der Index von <paramref name="item" />, wenn das Element in der Liste gefunden wird, andernfalls -1.</returns>
		public int IndexOf(T item)
		{
			int index = Array.IndexOf(_Array, item);

			if (index == -1)
				return -1;

			return (index - startIndex + _Count) % _Capacity;
		}

        /// <summary>
        /// Fügt am angegebenen Index ein Element in die <see cref="T:System.Collections.Generic.IList`1" /> ein.
        /// </summary>
        /// <param name="index">Der nullbasierte Index, an dem <paramref name="item" /> eingefügt werden soll.</param>
        /// <param name="item">Das in die <see cref="T:System.Collections.Generic.IList`1" /> einzufügende Objekt.</param>
        /// <exception cref="System.NotImplementedException"></exception>
		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// Entfernt das <see cref="T:System.Collections.Generic.IList`1" />-Element am angegebenen Index.
        /// </summary>
        /// <param name="index">Der nullbasierte Index des zu entfernenden Elements.</param>
        /// <exception cref="System.NotImplementedException"></exception>
		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ICollection<T> Members

        /// <summary>
        /// Bestimmt, ob <see cref="T:System.Collections.Generic.ICollection`1" /> einen bestimmten Wert enthält.
        /// </summary>
        /// <param name="item">Das im <see cref="T:System.Collections.Generic.ICollection`1" /> zu suchende Objekt.</param>
        /// <returns>true, wenn sich <paramref name="item" /> in <see cref="T:System.Collections.Generic.ICollection`1" /> befindet, andernfalls false.</returns>
		public bool Contains(T item)
		{
			return Array.IndexOf(_Array, item) > -1;
		}

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        /// <exception cref="System.NotImplementedException"></exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
            if (array.Length < _Array.Length)
            {
                Array.Copy(_Array, arrayIndex, array, 0, array.Length);
            }
            else if (array.Length > _Array.Length)
            {
                Array.Copy(_Array, arrayIndex, array, 0, _Array.Length);
            }
            else
            {
                this._Array.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob <see cref="T:System.Collections.Generic.ICollection`1" /> schreibgeschützt ist.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <returns>true, wenn <see cref="T:System.Collections.Generic.ICollection`1" /> schreibgeschützt ist, andernfalls false.</returns>
		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

        /// <summary>
        /// Entfernt das erste Vorkommen eines bestimmten Objekts aus <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">Das aus dem <see cref="T:System.Collections.Generic.ICollection`1" /> zu entfernende Objekt.</param>
        /// <returns>true, wenn <paramref name="item" /> erfolgreich aus <see cref="T:System.Collections.Generic.ICollection`1" /> gelöscht wurde, andernfalls false. Diese Methode gibt auch dann false zurück, wenn <paramref name="item" /> nicht in der ursprünglichen <see cref="T:System.Collections.Generic.ICollection`1" /> gefunden wurde.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable Members

        /// <summary>
        /// Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
        /// </summary>
        /// <returns>Ein <see cref="T:System.Collections.IEnumerator" />-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion    
    }

    /// <summary>
    /// Class EventExtensions
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Raises the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sender">The sender.</param>
        public static void Raise(this NotifyCollectionChangedEventHandler @event, object sender)
        {
            if (@event != null)
            {
                @event(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Raises the specified event when item added.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="newItem">The new item.</param>
        /// <param name="index">Index of new item.</param>
        public static void RaiseAdd(this NotifyCollectionChangedEventHandler @event, object sender,object newItem, int index)
        {
            if (@event != null && newItem != null)
            {
                @event(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,newItem,index));
            }
        }

        /// <summary>
        /// Raises the specified event when items removed.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="items">Removed items.</param>
        public static void RaiseRemove(this NotifyCollectionChangedEventHandler @event, object sender, IList items)
        {
            if (@event != null && items != null)
            {
                @event(sender, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,items));
            }
        }
    }
}
