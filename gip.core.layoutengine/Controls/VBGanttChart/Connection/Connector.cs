using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace gip.core.layoutengine.ganttchart
{
    /// <summary>
    /// Represent a connector of TimelineItem.
    /// Connector connect TimelineItems with Connection.
    /// </summary>
    public class Connector : Control, INotifyPropertyChanged
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ConnectorStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/ConnectorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ConnectorStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/ConnectorStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static Connector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connector),
                new FrameworkPropertyMetadata(typeof(Connector)));
        }

        bool _themeApplied = false;
        public Connector()
        {
            // fired when layout changes
            base.LayoutUpdated += new EventHandler(Connector_LayoutUpdated);
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #endregion

        // keep track of connections that link to this connector
        private List<Connection> connections;
        public List<Connection> Connections
        {
            get
            {
                if (connections == null)
                    connections = new List<Connection>();
                return connections;
            }
        }


        // when the layout changes we update the position property
        void Connector_LayoutUpdated(object sender, EventArgs e)
        {
            Visual surface = GetMeasureParent();
            if (surface != null)
            {
                //get centre position of this Connector relative to the DesignerCanvas
                this.Position = this.TransformToAncestor(surface).Transform(
                    new Point(this.Width / 2, this.Height / 2));
            }
        }

        private Visual GetMeasureParent()
        {
            DependencyObject element = this;
            while (element != null && !(element is TimelineGanttItemsPresenter))
                element = VisualTreeHelper.GetParent(element);

            if (element == null) return null;
            return VisualTreeHelper.GetParent(element) as Visual;
        }


        public ConnectorOrientation Orientation { get; set; }

        // center position of this Connector relative to the DesignerCanvas
        private Point position;
        public Point Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged("Position");
                }
            }
        }

        // keep track of connections that link to this connector
        //private List<Connection> connections;
        //public List<Connection> Connections {
        //  get {
        //    if (connections == null)
        //      connections = new List<Connection>();
        //    return connections;
        //  }
        //}

        // we could use DependencyProperties as well to inform others of property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

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
