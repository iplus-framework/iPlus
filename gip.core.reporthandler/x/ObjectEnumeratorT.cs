using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using combit.ListLabel17;

namespace gip.core.reporthandler
{
	internal class ObjectEnumerator<T> : IEnumerator<ITableRow>, IDisposable, IEnumerator
	{
		private ObjectTable<T> _table;
		private IEnumerable _source;
		private IEnumerator _sourceEnumerator;
		private int _currentRecord;
		public ITableRow Current
		{
			get
			{
				return new ObjectTableRow<T>(this._sourceEnumerator.Current, this._source as ITypedList, this._table, string.Format("{0}.{1}", this._table.NavigationId, this._currentRecord));
			}
		}
		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}
		public ObjectEnumerator(IEnumerable source, ObjectTable<T> table)
		{
			this._sourceEnumerator = ((source != null) ? source.GetEnumerator() : null);
			this._table = table;
			this._source = source;
		}
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		~ObjectEnumerator()
		{
			this.Dispose(false);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && this._sourceEnumerator is IDisposable)
			{
				(this._sourceEnumerator as IDisposable).Dispose();
			}
		}
		public bool MoveNext()
		{
			if (this._sourceEnumerator != null)
			{
				this._currentRecord++;
				return this._sourceEnumerator.MoveNext();
			}
			return false;
		}
		public void Reset()
		{
			throw new NotImplementedException();
		}
	}
}
