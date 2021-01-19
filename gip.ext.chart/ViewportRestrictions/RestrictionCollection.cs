using System.Linq;
using System.Windows;
using gip.ext.chart;
using gip.ext.chart.Common;
using gip.ext.chart.Common.Auxiliary;
using System;
using System.Collections.Specialized;

namespace gip.ext.chart.ViewportRestrictions
{
	public sealed class RestrictionCollection : D3Collection<IViewportRestriction>
	{
		protected override void OnItemAdding(IViewportRestriction item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
		}

		protected override void OnItemAdded(IViewportRestriction item)
		{
			item.Changed += OnItemChanged;
		}

		private void OnItemChanged(object sender, EventArgs e)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void OnItemRemoving(IViewportRestriction item)
		{
			item.Changed -= OnItemChanged;
		}

		internal Rect ApplyRestrictions(Rect oldVisible, Rect newVisible, Viewport2D viewport)
		{
			Rect res = newVisible;
			foreach (var restriction in this)
			{
				res = restriction.Apply(oldVisible, newVisible, viewport);
			}
			return res;
		}
	}
}
