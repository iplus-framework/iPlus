using System;
using System.Web.UI.WebControls;
using combit.ListLabel17;

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
