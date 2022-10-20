using System;
using System.Collections.ObjectModel;
using combit.ListLabel17.DataProviders;

namespace gip.core.reporthandler
{
	internal abstract class DataProviderWrapper : IDataProvider
	{
		protected IDataProvider Provider
		{
			get;
			set;
		}
		public bool SupportsAnyBaseTable
		{
			get
			{
				return this.Provider.SupportsAnyBaseTable;
			}
		}
		public ReadOnlyCollection<ITable> Tables
		{
			get
			{
				return this.Provider.Tables;
			}
		}
		public ReadOnlyCollection<ITableRelation> Relations
		{
			get
			{
				return this.Provider.Relations;
			}
		}
		public ITable GetTable(string tableName)
		{
			return this.Provider.GetTable(tableName);
		}
		public ITableRelation GetRelation(string relationName)
		{
			return this.Provider.GetRelation(relationName);
		}
	}
}
