using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.chart.Charts;

namespace gip.ext.chart.Common.Auxiliary
{
	internal static class ArrayExtensions
	{
		internal static T Last<T>(this T[] array) {
			return array[array.Length - 1];
		}

		internal static T[] CreateArray<T>(int length, T defaultValue)
		{
			T[] res = new T[length];
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = defaultValue;
			}
			return res;
		}

		internal static IEnumerable<Range<T>> GetPairs<T>(this T[] array)
		{
			for (int i = 0; i < array.Length - 1; i++)
			{
				yield return new Range<T>(array[i], array[i + 1]);
			}
		}
	}
}
