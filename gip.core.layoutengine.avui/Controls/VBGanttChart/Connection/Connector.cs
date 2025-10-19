using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia.Controls.Primitives;
using Avalonia;
using gip.core.layoutengine.avui.Helperclasses;
using Avalonia.VisualTree;
using gip.ext.design.avui;
using Avalonia.Media;

namespace gip.core.layoutengine.avui.ganttchart
{
    /// <summary>
    /// Represent a connector of TimelineItem.
    /// Connector connect TimelineItems with Connection.
    /// </summary>
    public class Connector : TemplatedControl
    {
        #region c'tors
        public Connector()
        {
            // fired when layout changes
            base.LayoutUpdated += Connector_LayoutUpdated;
        }
        #endregion

        // keep track of connections that link to this connector
        private List<Connection> _Connections;
        public List<Connection> Connections
        {
            get
            {
                if (_Connections == null)
                    _Connections = new List<Connection>();
                return _Connections;
            }
        }

        // when the layout changes we update the position property
        void Connector_LayoutUpdated(object sender, EventArgs e)
        {
            Visual surface = GetMeasureParent();
            if (surface != null)
            {
                //get centre position of this Connector relative to the DesignerCanvas
                Transform targetTransform = this.TransformToAncestor(surface);
                this.Position = new Point(this.Width / 2, this.Height / 2).Transform(targetTransform.Value);
            }
        }

        private Visual GetMeasureParent()
        {
            Visual element = this;
            while (element != null && !(element is TimelineGanttItemsPresenter))
            {
                element = element.GetVisualParent();
            }

            if (element == null) 
                return null;
            return element.GetVisualParent() as Visual;
        }

        #region Styled Properties

        /// <summary>
        /// Defines the Orientation styled property.
        /// </summary>
        public static readonly StyledProperty<ConnectorOrientation> OrientationProperty =
            AvaloniaProperty.Register<Connector, ConnectorOrientation>(nameof(Orientation), ConnectorOrientation.None);

        /// <summary>
        /// Gets or sets the orientation of the connector.
        /// </summary>
        public ConnectorOrientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Defines the Position styled property.
        /// </summary>
        public static readonly StyledProperty<Point> PositionProperty =
            AvaloniaProperty.Register<Connector, Point>(nameof(Position), new Point());

        /// <summary>
        /// Gets or sets the center position of this Connector relative to the DesignerCanvas.
        /// </summary>
        public Point Position
        {
            get => GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        #endregion
    }

    public enum ConnectorOrientation
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }
}
