using System;
using combit.ListLabel17;
using combit.ListLabel17.DataProviders;

namespace gip.core.reporthandler
{
	internal class ObjectTableColumn : ITableColumn
	{
		private string _columnName;
		private Type _dataType;
		private object _content;
		public string ColumnName
		{
			get
			{
				return this._columnName;
			}
		}
		public Type DataType
		{
			get
			{
				return this._dataType;
			}
		}
		public object Content
		{
			get
			{
				return this._content;
			}
		}
		public ObjectTableColumn(string columnName, Type dataType, object content)
		{
			this._columnName = columnName;
			this._dataType = dataType;
			this._content = content;
		}
	}
}
