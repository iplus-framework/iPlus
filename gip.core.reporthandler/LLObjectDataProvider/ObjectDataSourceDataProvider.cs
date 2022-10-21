using System;
using combit.ListLabel17;
using combit.ListLabel17.DataProviders;

namespace gip.core.reporthandler
{
	internal class ObjectDataSourceDataProvider : DataProviderWrapper
	{
		internal ObjectDataSourceDataProvider(ObjectDataSource dataSource)
		{
			base.Provider = new ObjectDataProvider(dataSource.Select());
		}
	}
}
