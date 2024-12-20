﻿using System.Windows;
using System.Windows.Media;
using gip.ext.chart.DataSources;
using gip.ext.chart.PointMarkers;

namespace gip.ext.chart
{
	public class MarkerPointsGraph : PointsGraphBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		public MarkerPointsGraph()
		{
			ManualTranslate = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
		/// </summary>
		/// <param name="dataSource">The data source.</param>
		public MarkerPointsGraph(IPointDataSource dataSource)
			: this()
		{
			DataSource = dataSource;
		}

		protected override void OnVisibleChanged(Rect newRect, Rect oldRect)
		{
			base.OnVisibleChanged(newRect, oldRect);
			InvalidateVisual();
		}

		public PointMarker Marker
		{
			get { return (PointMarker)GetValue(MarkerProperty); }
			set { SetValue(MarkerProperty, value); }
		}

		public static readonly DependencyProperty MarkerProperty =
			DependencyProperty.Register(
			  "Marker",
			  typeof(PointMarker),
			  typeof(MarkerPointsGraph),
              new FrameworkPropertyMetadata { DefaultValue = null, AffectsRender = true }
				  );

		protected override void OnRenderCore(DrawingContext dc, RenderState state)
		{
			if (DataSource == null) return;
			if (Marker == null) return;

			var transform = Plotter2D.Viewport.Transform;

			Rect bounds = Rect.Empty;
			using (IPointEnumerator enumerator = DataSource.GetEnumerator(GetContext()))
			{
				Point point = new Point();
				while (enumerator.MoveNext())
				{
					enumerator.GetCurrent(ref point);
					enumerator.ApplyMappings(Marker);

					//Point screenPoint = point.Transform(state.Visible, state.Output);
					Point screenPoint = point.DataToScreen(transform);

					bounds = Rect.Union(bounds, point);
					Marker.Render(dc, screenPoint);
				}
			}

			ContentBounds = bounds;
		}
	}
}
