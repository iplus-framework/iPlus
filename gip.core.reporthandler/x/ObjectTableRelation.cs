using System;
using combit.ListLabel17.DataProviders;

namespace gip.core.reporthandler
{
	internal class ObjectTableRelation : ITableRelation
	{
		private Type _genericEnumerableTableType;
		public Type GenericEnumerableTableType
		{
			get
			{
				return this._genericEnumerableTableType;
			}
		}
		public string ChildPropertyName
		{
			get;
			set;
		}
		public string RelationName
		{
			get;
			set;
		}
		public string ParentColumnName
		{
			get;
			set;
		}
		public string ChildColumnName
		{
			get;
			set;
		}
		public string ParentTableName
		{
			get;
			set;
		}
		public string ChildTableName
		{
			get;
			set;
		}
		public ObjectTableRelation(string parentTableName, Type genericEnumerableTableType, string childTableName, string childPropertyName, string relationName)
		{
			this.ParentTableName = parentTableName;
			this.ChildTableName = childTableName;
			this.RelationName = relationName;
			this.ParentColumnName = "__LL__ID";
			this.ChildColumnName = "__LL__ID";
			this.ChildPropertyName = childPropertyName;
			this._genericEnumerableTableType = genericEnumerableTableType;
		}
	}
}
