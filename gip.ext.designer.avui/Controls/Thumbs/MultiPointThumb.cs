// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Description of MultiPointThumb.
	/// </summary>
	internal sealed class MultiPointThumb : DesignerThumb
	{
		private int _index;

		public int Index
		{
			get { return _index; }
			set
			{
				_index = value;
				var p = AdornerPlacement as PointTrackerPlacementSupport;
				if (p != null)
					p.Index = value;
			}
		}

		private AdornerPlacement _adornerPlacement;

		public AdornerPlacement AdornerPlacement
		{
			get { return _adornerPlacement; }
			set { _adornerPlacement = value; }
		}
	}
}
