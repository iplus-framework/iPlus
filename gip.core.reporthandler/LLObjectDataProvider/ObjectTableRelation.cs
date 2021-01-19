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

        private string _ChildPropertyName;
		public string ChildPropertyName
		{
            get
            {
                return _ChildPropertyName;
            }
            set
            {
                _ChildPropertyName = value;
            }
		}

        private string _RelationName;
		public string RelationName
		{
            get
            {
                return _RelationName;
            }
            set
            {
                _RelationName = value;
            }
		}

        private string _ParentColumnName;
		public string ParentColumnName
		{
            get
            {
                return _ParentColumnName;
            }
			set
            {
                _ParentColumnName = value;
            }
		}

        private string _ChildColumnName;
		public string ChildColumnName
		{
            get
            {
                return _ChildColumnName;
            }
            set
            {
                _ChildColumnName = value;
            }
		}

        private string _ParentTableName;
		public string ParentTableName
		{
            get
            {
                return _ParentTableName;
            }
            set
            {
                _ParentTableName = value;
            }
		}

        private string _ChildTableName;
		public string ChildTableName
		{
            get
            {
                return _ChildTableName;
            }
            set
            {
                _ChildTableName = value;
            }
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
