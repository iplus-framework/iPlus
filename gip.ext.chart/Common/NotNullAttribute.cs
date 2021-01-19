using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.chart
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
	internal sealed class NotNullAttribute : Attribute
	{
	}
}
