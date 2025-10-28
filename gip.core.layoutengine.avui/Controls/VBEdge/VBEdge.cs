//#define POLY 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using gip.ext.designer.avui.Services;
using System.Transactions;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using gip.ext.graphics.avui.shapes;
using Avalonia;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Input;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents VBEdge resize extension.
    /// </summary>
    [ExtensionFor(typeof(VBEdge), OverrideExtension = typeof(ResizeThumbExtension))]
    public class VBEdgeResizeExtension : SelectionAdornerProvider
    {
        /// <summary>
        /// Creates a new instance of VBEdgeResizeExtension.
        /// </summary>
        public VBEdgeResizeExtension()
        {
        }
    }



    /// <summary>
    /// Event-Args contains info about previous and current Draw-Info over VBEdge
    /// </summary>
    public class VBEdgeRedrawEventArgs : EventArgs
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBEdgeRedrawEventArgs.
        /// </summary>
        /// <param name="lastRendererSourcePosition">The last renderer source position.</param>
        /// <param name="lastRendererTargetPosition">The last renderer target position</param>
        /// <param name="lastGeometry">The last geometry parameter.</param>
        /// <param name="newSourcePosition">The new source position.</param>
        /// <param name="newTargetPosition">The new target position.</param>
        /// <param name="newGeometry">The new geometry parameter.</param>
        public VBEdgeRedrawEventArgs(Point lastRendererSourcePosition, Point lastRendererTargetPosition, Geometry lastGeometry,
                                     Point newSourcePosition, Point newTargetPosition, Geometry newGeometry)
            : base()
        {
            _LastRendererSourcePosition = lastRendererSourcePosition;
            _LastRendererTargetPosition = lastRendererTargetPosition;
            _LastGeometry = lastGeometry;
            _NewSourcePosition = newSourcePosition;
            _NewTargetPosition = newTargetPosition;
            _NewGeometry = newGeometry;
        }
        #endregion

        #region Member
        private Point _LastRendererSourcePosition;
        /// <summary>
        /// Gets the last renderer source position.
        /// </summary>
        public Point LastRendererSourcePosition
        {
            get
            {
                return _LastRendererSourcePosition;
            }
        }
        
        private Point _LastRendererTargetPosition;
        /// <summary>
        /// Gets the last renderer target position.
        /// </summary>
        public Point LastRendererTargetPosition
        {
            get
            {
                return _LastRendererTargetPosition;
            }
        }

        private Geometry _LastGeometry;
        /// <summary>
        /// Gets the last geometry.
        /// </summary>
        public Geometry LastGeometry
        {
            get
            {
                return _LastGeometry;
            }
        }


        private Point _NewSourcePosition;
        /// <summary>
        /// Gets the new source position.
        /// </summary>
        public Point NewSourcePosition
        {
            get
            {
                return _NewSourcePosition;
            }
        }
        
        private Point _NewTargetPosition;
        /// <summary>
        /// Gets the new target position.
        /// </summary>
        public Point NewTargetPosition
        {
            get
            {
                return _NewTargetPosition;
            }
        }

        private Geometry _NewGeometry;
        /// <summary>
        /// Gets the new geometry.
        /// </summary>
        public Geometry NewGeometry
        {
            get
            {
                return _NewGeometry;
            }
        }
        #endregion
    }

    /// <summary>
    /// The delegate for VBEdgeRedrawEvent.
    /// </summary>
    /// <param name="sender">The sender parameter.</param>
    /// <param name="e">The VBEdgeRedraw event arguments.</param>
    public delegate void VBEdgeRedrawEvent(object sender, VBEdgeRedrawEventArgs e);

    /// <summary>
    /// Control element for connecting lines between <see cref="VBVisual"/>/<see cref="VBVisualGroup"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement für Verbindungslinien zwischen <see cref="VBVisual"/>/<see cref="VBVisualGroup"/>.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBEdge'}de{'VBEdge'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBEdge : ArrowPolyline, IVBRelation, IACMenuBuilder, IACObject, IEdge
    {
        #region c'tors and Initialization

        /// <summary>
        /// Creates a new instance of VBEdge.
        /// </summary>
        public VBEdge()
        {
            VBDesignBase.IsSelectableEnum isSelectable = VBDesignBase.GetIsSelectable(this);
            if (isSelectable == VBDesignBase.IsSelectableEnum.Unset)
                VBDesignBase.SetIsSelectable(this, VBDesignBase.IsSelectableEnum.True);
        }


        static VBEdge()
        {
            ArrowEndsProperty.OverrideMetadata(typeof(VBEdge), new StyledPropertyMetadata<ArrowEnds>(ArrowEnds.End));//, FrameworkPropertyMetadataOptions.AffectsMeasure));
            IsArrowClosedProperty.OverrideMetadata(typeof(VBEdge), new StyledPropertyMetadata<bool>(true)); //, FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        private static bool _NewEdgeRouting = false;

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Unloaded += VBConnectPath_Unloaded;
            this.Loaded += VBConnectPath_Loaded;
        }

        protected virtual void VBConnectPath_Loaded(object sender, RoutedEventArgs e)
        {
            InitConnectionPoints();
        }

        void VBConnectPath_Unloaded(object sender, RoutedEventArgs e)
        {
            // Remove List in VBConnector
            this.Source = null;
            this.Target = null;
        }


        private void InitConnectionPoints()
        {
            if ((this.Source != null) || (this.Target != null))
                return;
            this.Source = GetConnector(VBConnectorSource);
            this.Target = GetConnector(VBConnectorTarget);
            if ((this.Source != null) || (this.Target != null))
            {
                CalculateNewConnectorPos();
                if (_NewEdgeRouting)
                    TransformConnectorPosToPolylinePoints();
                Geometry newGeo = GetConnectionPathGeometry();
                if (!_NewEdgeRouting)
                    newGeo = TransformGeometryToZeroCoord(newGeo);

                this.Data = newGeo;
                UpdatePlacementOfParentContainer(_NewSourcePosition, _NewTargetPosition);
            }
        }
        #endregion

        #region Dependency-Properties

        #region Geometry-Data
        //public static readonly DependencyProperty DataProperty
        //    = DependencyProperty.Register("Data", typeof(Geometry), typeof(VBEdge), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        //public Geometry Data
        //{
        //    get { return (Geometry)GetValue(DataProperty); }
        //    set { SetValue(DataProperty, value); }
        //}


        /// <summary>
        /// Gets or sets the geometry data.
        /// </summary>
        public Geometry Data { get; set; }

        /// <summary>
        /// Gets the defining geometry.
        /// </summary>
        protected override Geometry CreateDefiningGeometry()
        {
            if (this.Data == null)
            {
                if (_NewEdgeRouting)
                    InitConnectionPoints();
                return base.DefiningGeometry;
            }
            if (_NewEdgeRouting)
                return this.Data != null ? this.Data : base.DefiningGeometry;
            else
                return this.Data;
        }
        #endregion

        #region Selected
        /// <summary>
        /// Determines is selected or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsSelected.
        /// </summary>
        public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<VBEdge, bool>(nameof(IsSelected));

        #endregion

        #region IVBRelation Member
        /// <summary>
        /// Represents the dependency property for VBConnectorSource.
        /// </summary>

        public static readonly StyledProperty<string> VBConnectorSourceProperty = AvaloniaProperty.Register<VBEdge, string>(nameof(VBConnectorSource));
        /// <summary>VBConnectorSource is the VBContent to address a SOURCE-IACObject
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBConnectorSource
        {
            get { return (string)GetValue(VBConnectorSourceProperty); }
            set { SetValue(VBConnectorSourceProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBConnectorTarget.
        /// </summary>


        public static readonly StyledProperty<string> VBConnectorTargetProperty = AvaloniaProperty.Register<VBEdge, string>(nameof(VBConnectorTarget));

        /// <summary>VBConnectorSource is the VBContent to address a TARGET-IACObject
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBConnectorTarget
        {
            get { return (string)GetValue(VBConnectorTargetProperty); }
            set { SetValue(VBConnectorTargetProperty, value); }
        }
        #endregion

        #endregion

        #region public Properties

        // From/Source Connector
        private VBConnector _Source;
        /// <summary>
        /// Gets or sets the source VBConnector.
        /// </summary>
        public VBConnector Source
        {
            get
            {
                return _Source;
            }
            set
            {
                if (_Source != value)
                {
                    if (_Source != null)
                    {
                        _Source.LayoutUpdated -= SourceConnector_LayoutUpdated;
                        _Source.ConnectedEdges.Remove(this);
                    }

                    _Source = value;

                    if (_Source != null)
                    {
                        _Source.ConnectedEdges.Add(this);
                        _Source.LayoutUpdated += SourceConnector_LayoutUpdated;
                    }
                }
            }
        }

        // To/Target Connector
        private VBConnector _Target;
        /// <summary>
        /// Gets or sets the target VBConnector
        /// </summary>
        public VBConnector Target
        {
            get { return _Target; }
            set
            {
                if (_Target != value)
                {
                    if (_Target != null)
                    {
                        _Target.LayoutUpdated -= TargetConnector_LayoutUpdated;
                        _Target.ConnectedEdges.Remove(this);
                    }

                    _Target = value;

                    if (_Target != null)
                    {
                        _Target.ConnectedEdges.Add(this);
                        _Target.LayoutUpdated += TargetConnector_LayoutUpdated;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the parent container of edge.
        /// </summary>
        public Control ParentContainerOfEdge
        {
            get
            {
                if (this.Parent == null)
                    return null;
                if (this.Parent is VBCanvas)
                    return this.Parent as VBCanvas;
                if (this.Parent is Control)
                    return this.Parent as Control;
                AvaloniaObject parent = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBCanvas));
                if (parent == null)
                    return null;
                return parent as VBCanvas;
            }
        }
        #endregion

        #region private/protected Methods
        /// <summary>
        /// Gets the VBConnector according acName parameter.
        /// </summary>
        /// <param name="acName">The acName parameter.</param>
        /// <returns>The VBConnector if is found, otherwise null.</returns>
        protected VBConnector GetConnector(string acName)
        {
            if (!(this.Parent is Control))
                return null;
            Control x = VBVisualTreeHelper.FindChildVBContentObjectInVisualTree(this.Parent as Control, acName);
            if (x != null)
            {
                if (x is VBConnector)
                    return x as VBConnector;
            }
            return null;
        }
        #endregion

        #region public static Geometry-Generation-Methods
        /// <summary>
        /// Gets the line geometry.
        /// </summary>
        /// <param name="sourcePointRelativeToContainer">The source point relative to container.</param>
        /// <param name="targetPointRelativeToContainer">The target point relative to container.</param>
        /// <returns>The line geometry.</returns>
        public static Geometry GetLineGeometry(Point sourcePointRelativeToContainer, Point targetPointRelativeToContainer)
        {
            LineGeometry lineGeometry = new LineGeometry(sourcePointRelativeToContainer, targetPointRelativeToContainer);
            return lineGeometry;
        }

        /// <summary>
        /// Gets the line path geometry.
        /// </summary>
        /// <returns>The line path geometry.</returns>
        public Geometry GetLinePathGeo()
        {
            Points.Clear();
            Points.Add(_NewSourcePosition);
            Points.Add(_NewTargetPosition);
            return GetPathGeometry(Points, this.ArrowEnds, 6, 50, true);
        }

        /// <summary>
        /// Gets the connection path geometry.
        /// </summary>
        /// <returns>The connection path geometry.</returns>
        public Geometry GetConnectionPathGeometry()
        {
            if (Points == null)
                return null;

            if (Points.Count <= 2)
            {
                Points.Clear();
                Points.Add(_NewSourcePosition);
                Points.Add(_NewTargetPosition);
            }
            Point start, end;
            Points = gip.ext.designer.avui.Controls.DrawShapesAdornerBase.TranslatePointsToBounds(Points, out start, out end);
            return GetPathGeometry(Points, this.ArrowEnds, 6, 50, true);
        }

        /// <summary>
        /// Transforms the geometry to zero coordinates.
        /// </summary>
        /// <param name="geometry">The geometry for transform.</param>
        /// <returns>The transformed geometry.</returns>
        public static Geometry TransformGeometryToZeroCoord(Geometry geometry)
        {
            if (geometry is PathGeometry)
            {
                PathGeometry pathGeometry = geometry as PathGeometry;
                var l = pathGeometry.Bounds.Left;
                var t = pathGeometry.Bounds.Top;
                foreach (var xxx in pathGeometry.Figures)
                {
                    xxx.StartPoint = new Point(xxx.StartPoint.X - l, xxx.StartPoint.Y - t);
                    foreach (var x2 in xxx.Segments)
                    {
                        for (int i = 0; i < ((PolyLineSegment)(x2)).Points.Count(); i++)
                        {
                            var p = ((PolyLineSegment)(x2)).Points[i];
                            p = new Point(p.X - l, p.Y - t);
                            ((PolyLineSegment)(x2)).Points[i] = p;
                        }
                    }
                }
                return pathGeometry;
            }

            else if (geometry is LineGeometry)
            {
                double width = 0;
                double height = 0;

                width = (geometry as LineGeometry).EndPoint.X - (geometry as LineGeometry).StartPoint.X;
                height = (geometry as LineGeometry).EndPoint.Y - (geometry as LineGeometry).StartPoint.Y;
                if ((width == 0) && (height == 0))
                    return new LineGeometry();

                // Transformation auf 0-Koordiaatensystem linke ober Ecke
                Point startPoint;
                Point endPoint;
                // Linie nach rechts unten
                if ((width >= 0) && (height >= 0))
                {
                    startPoint = new Point(0, 0);
                    endPoint = new Point(width, height);
                }
                // Linie nach rechts oben
                else if ((width >= 0) && (height < 0))
                {
                    height *= -1;
                    startPoint = new Point(0, height);
                    endPoint = new Point(width, 0);
                }
                // Linie nach links unten
                else if ((width < 0) && (height >= 0))
                {
                    width *= -1;
                    startPoint = new Point(width, 0);
                    endPoint = new Point(0, height);
                }
                // Linie nach links oben
                else //if ((width < 0) && (height < 0))
                {
                    height *= -1;
                    width *= -1;
                    startPoint = new Point(0, 0);
                    endPoint = new Point(width, height);
                }
                return new LineGeometry(startPoint, endPoint);
            }
            return geometry;
        }

        /// <summary>
        /// Gets the geometry as a string.
        /// </summary>
        /// <param name="geometry">The geometry parameter.</param>
        /// <returns>The geometry in string format.</returns>
        public static string GetGeometryAsString(Geometry geometry)
        {
            if ((geometry == null) || !(geometry is LineGeometry))
                return "";
            LineGeometry transformedLG = geometry as LineGeometry;
            return string.Format("M {0},{1} L {2},{3}",
                    transformedLG.StartPoint.X,
                    transformedLG.StartPoint.Y,
                    transformedLG.EndPoint.X,
                    transformedLG.EndPoint.Y);
        }

        #endregion

        #region Rendering and Drawing and Event-Handling

        Point _LastRendererSourcePosition = new Point();
        Point _LastRendererTargetPosition = new Point();
        Point _NewSourcePosition = new Point();
        Point _NewTargetPosition = new Point();

        protected override void OnMeasureInvalidated()
        {
            // AvaloniaWorkaraound because Render is sealed
            OnRender();
            base.OnMeasureInvalidated();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // AvaloniaWorkaraound because Render is sealed
            OnRender();
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // AvaloniaWorkaraound because Render is sealed
            OnRender();
            return base.ArrangeOverride(finalSize);
        }

        //protected override void Render(DrawingContext drawingContext)
        private void OnRender()
        {
            //base.Render(drawingContext);
            InitConnectionPoints();
            if (Source != null && Target != null && ParentContainerOfEdge != null)
            {
                _LastRendererSourcePosition = Source.GetPointRelativeToContainer(ParentContainerOfEdge);
                _LastRendererTargetPosition = Target.GetPointRelativeToContainer(ParentContainerOfEdge);
            }
        }

        void SourceConnector_LayoutUpdated(object sender, EventArgs e)
        {
            CalculateNewConnectorPos();
            if (_NewSourcePosition != _LastRendererSourcePosition)
            {
                if (_NewEdgeRouting)
                    TransformConnectorPosToPolylinePoints();
                Geometry newGeo = GetLineGeometry(_NewSourcePosition, _NewTargetPosition);
                if (!_NewEdgeRouting)
                    newGeo = TransformGeometryToZeroCoord(newGeo);
                this.Data = newGeo;
                UpdatePlacementOfParentContainer(_NewSourcePosition, _NewTargetPosition);
                OnVBEdgeRedraw(newGeo);
            }
        }

        void TargetConnector_LayoutUpdated(object sender, EventArgs e)
        {
            CalculateNewConnectorPos();
            if (_NewTargetPosition != _LastRendererTargetPosition)
            {
                if (_NewEdgeRouting)
                    TransformConnectorPosToPolylinePoints();
                Geometry newGeo = GetLineGeometry(_NewSourcePosition, _NewTargetPosition);
                if (!_NewEdgeRouting)
                    newGeo = TransformGeometryToZeroCoord(newGeo);
                this.Data = newGeo;
                UpdatePlacementOfParentContainer(_NewSourcePosition, _NewTargetPosition);
                OnVBEdgeRedraw(newGeo);
            }
        }

        /// <summary>
        /// Redraws the VBEdge.
        /// </summary>
        /// <param name="isFromDrag">Determines is redraw is from drag or not.</param>
        /// <param name="withCalcConnPos">Determines is calculation of connector position performed or not.</param>
        public void RedrawVBEdge(bool isFromDrag, bool withCalcConnPos = false)
        {
            if (Points == null)
                return;

            if (withCalcConnPos)
                CalculateNewConnectorPos();

            if (isFromDrag && Points != null)
                Points.Clear();
            if (_NewEdgeRouting)
                TransformConnectorPosToPolylinePoints();
            Geometry newGeo = GetConnectionPathGeometry();
            if (!_NewEdgeRouting)
                newGeo = TransformGeometryToZeroCoord(newGeo);
            this.Data = newGeo;
            UpdatePlacementOfParentContainer(_NewSourcePosition, _NewTargetPosition);
            InvalidateVisual();
        }

        void CalculateNewConnectorPos()
        {
            if ((Source != null) && (Target != null) && (ParentContainerOfEdge != null))
            {
                _NewSourcePosition = Source.GetPointRelativeToContainer(ParentContainerOfEdge);
                _NewTargetPosition = Target.GetPointRelativeToContainer(ParentContainerOfEdge);
            }
            else
            {
                _NewSourcePosition = new Point();
                _NewTargetPosition = new Point();
            }

            if (_NewEdgeRouting)
            {
                bool rebuildPoints = true;
                if (Points.Any() && Points.Count >= 2)
                {
                    if (Points[0] == _NewSourcePosition && Points.Last() == _NewTargetPosition)
                        rebuildPoints = false;
                }

                if (rebuildPoints)
                {
                    if (Points == null || !Points.Any())
                        Points = new List<Point>();
                    Points.Insert(0, _NewSourcePosition);
                    Points.Insert(Points.Count, _NewTargetPosition);
                }
            }
        }

        void TransformConnectorPosToPolylinePoints()
        {
            List<Point> points = new List<Point>();
            points.Add(_NewSourcePosition);
            points.Add(_NewTargetPosition);
            Point startPoint;
            Point endPoint;
            this.Points = DrawShapesAdornerBase.TranslatePointsToBounds(points, out startPoint, out endPoint);
        }

        public event VBEdgeRedrawEvent VBEdgeRedraw;
        protected void OnVBEdgeRedraw(Geometry newGeo)
        {
            if (VBEdgeRedraw != null)
            {
                VBEdgeRedraw(this, new VBEdgeRedrawEventArgs(_LastRendererSourcePosition, _LastRendererTargetPosition,this.Data,
                                                             _NewSourcePosition, _NewTargetPosition, newGeo));
            }
        }

        private void UpdatePlacementOfParentContainer(Point sourceConnector, Point targetConnector)
        {
            if (ParentContainerOfEdge == null)
                return;
            Rect position;
            // TODO: DrawLineAdorner ist falsche Instanz bei Routed Edges
            if (DrawLineAdorner.GetRectForPlacementOperation(sourceConnector, targetConnector, out position))
            {
                if (ParentContainerOfEdge is Canvas)
                {
                    var x = DefiningGeometry;
                    if (x == null)
                        return;
                    this.Width = DefiningGeometry.Bounds.Width+2;
                    this.Height = DefiningGeometry.Bounds.Height+2;
                    //this.Width = position.Width;
                    //this.Height = position.Height;
                    if (DefiningGeometry is PathGeometry)
                    {
                        PathGeometry pathGeometry = DefiningGeometry as PathGeometry;
                        PathFigure pathFigure = pathGeometry.Figures.FirstOrDefault();
                        Canvas.SetLeft(this, sourceConnector.X - pathFigure.StartPoint.X);
                        Canvas.SetTop(this, sourceConnector.Y - pathFigure.StartPoint.Y);
                    }
                    else
                    {
                        Canvas.SetLeft(this, position.Left);
                        Canvas.SetTop(this, position.Top);
                    }
                }
            }
            if (ParentContainerOfEdge == null)
                return;

            // TODO: DrawLineAdorner ist falsche Instanz bei Routed Edges
            //if (DrawLineAdorner.GetRectForPlacementOperation(sourceConnector, targetConnector, out position))
            //{
            //    if (ParentContainerOfEdge is Canvas)
            //    {
            //        this.Width = position.Width;
            //        this.Height = position.Height;
            //        Canvas.SetLeft(this, position.Left);
            //        Canvas.SetTop(this, position.Top);
            //    }
            //}
        }
        #endregion

        #region Arrow's
        #endregion

        #region IACMenuBuilder Member

        /// <summary>
        /// Gets the context menu content.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        /// <returns>The list of ACMenu items.</returns>
        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            //try
            //{
            //}
            //catch (Exception /*e*/)
            //{
            //}
            //this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
            return acMenuItemList;
        }

        #endregion

        #region IVBContent Member
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBEdge, string>(nameof(VBContent));
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (BSOACComponent != null && BSOACComponent.ACIdentifier.StartsWith("VBPresenter"))
                {
                    return BSOACComponent.ACUrlCommand(VBContent) as IACObject;
                }
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBEdge>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }


        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        
        IACObject _ACObject;
        /// <summary>
        /// Gets the ContentACObject.
        /// </summary>
        public IACObject ContentACObject
        {
            get
            {
                if (_ACObject != null)
                    return _ACObject;
                if (ContextACObject == null)
                    return null;
                if (string.IsNullOrEmpty(VBContent))
                    return null;
                _ACObject = ContextACObject.ACUrlCommand(VBContent) as IACObject;
                return _ACObject;
            }
        }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public virtual IEnumerable<IACObject> ACContentList
        {
            get
            {
                if (ContentACObject != null)
                {
                    List<IACObject> acContentList = new List<IACObject>();
                    acContentList.Add(ContentACObject);
                    if (ContentACObject is IACComponent)
                    {
                        IACComponent acComponent = ContentACObject as IACComponent;
                        if (acComponent.Content != null)
                            acContentList.Add(acComponent.Content);
                    }
                    return acContentList;
                }
                return null;
            }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }
        #endregion


        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!e.Properties.IsRightButtonPressed)
            {
                base.OnPointerPressed(e);
                return;
            }
            VBDesign vbDesign = this.GetVBDesign();

            if (vbDesign != null && vbDesign.IsDesignerActive && (vbDesign.GetDesignManager() == null || !vbDesign.GetDesignManager().ACIdentifier.StartsWith("VBDesignerWorkflowMethod") 
                && !vbDesign.GetDesignManager().ACIdentifier.StartsWith("VBDesignerMaterialWF")))
            {
                return;
            }
            Point point = e.GetPosition(this);
            ACActionMenuArgs actionArgs = new ACActionMenuArgs(this as IACInteractiveObject, point.X, point.Y, Global.ElementActionType.ContextMenu);
            //BSOACComponent.ParentACComponent.ACAction(actionArgs);
            BSOACComponent.ACAction(actionArgs);
            if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
            {
                VBContextMenu vbContextMenu = new VBContextMenu(this as IACInteractiveObject, actionArgs.ACMenuItemList);
                this.ContextMenu = vbContextMenu;
                //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                if (vbContextMenu.PlacementTarget == null)
                    vbContextMenu.PlacementTarget = this;
                ContextMenu.Open();
                e.Handled = true;
            }
            base.OnPointerPressed(e);
        }

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
           return  this.ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion

        /// <summary>
        /// Overrides the ToString method.
        /// </summary>
        /// <returns>The string with information about this VBEdge.(Name, VBContent, VBConnectorSource, VBConnectorTarget)</returns>
        public override string ToString()
        {
            return string.Format("VBEdge: {0} {1} {2} <-> {3}", this.Name, this.VBContent, this.VBConnectorSource, this.VBConnectorTarget);
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
        }

        /// <summary>
        /// Gets the point of source connector relative to given container.
        /// </summary>
        /// <param name="container">The container parameter.</param>
        /// <returns>The point relvative to container.</returns>
        public Point GetSourceConnectorPointToContainer(Visual container)
        {
            return Source.GetPointRelativeToContainer(container);
        }

        /// <summary>
        /// Gets the point of target connector relative to give container.
        /// </summary>
        /// <param name="container">The container parameter.</param>
        /// <returns>The point relative to container.</returns>
        public Point GetTargetConnectorPointToContainer(Visual container)
        {
            return Target.GetPointRelativeToContainer(container);
        }

        /// <summary>
        /// Gets the parent element of source VBConnector.
        /// </summary>
        public virtual Control SourceElement
        {
            get
            {
                if(Source != null)
                    return Source.ParentVBControl as Control;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the parent element of target VBConnector.
        /// </summary>
        public virtual Control TargetElement
        {
            get
            {
                if (Target != null)
                    return Target.ParentVBControl as Control;
                else
                    return null;
            }
        }
    }

    #region Arrows
    /// <summary>
    /// Represents the enumeration for Arrow symbols.
    /// </summary>
    public enum ArrowSymbol : short
    {
        None,
        Arrow,
        Diamond
    }

    /// <summary>
    /// Represents the converter for arrows.
    /// </summary>
    static public class ArrowConverter
    {
        /// <summary>
        /// Converts symbol to string.
        /// </summary>
        /// <param name="symbol">The symbol parameter.</param>
        /// <returns>The name of given symbol.</returns>
        public static string ConvertToString(ArrowSymbol symbol)
        {
            return symbol.ToString();
        }

        /// <summary>
        /// Converts string to ArrowSymbol. 
        /// </summary>
        /// <param name="symbol">The name of symbol.</param>
        /// <returns>The ArrowSymbol.</returns>
        public static ArrowSymbol ConvertToSymbol(string symbol)
        {
            switch (symbol)
            {
                case "Arrow":
                    return ArrowSymbol.Arrow;
                case "Diamond":
                    return ArrowSymbol.Diamond;
                case "None":
                default:
                    return ArrowSymbol.None;
            }
        }
    }
    #endregion
}
