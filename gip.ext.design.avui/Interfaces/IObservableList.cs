// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Collections.Generic;
using System.Collections.Specialized;


namespace gip.ext.design.avui
{
	/// <summary>
	/// A IList wich implements INotifyCollectionChanged
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IObservableList<T> : IList<T>, INotifyCollectionChanged
    {
    }
}