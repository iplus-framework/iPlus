using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Interaction logic for VBGraphSurface.xaml
    /// </summary>
    public partial class VBGraphSurface : UserControl, IACObject
    {
        #region Events

        public event EventHandler OnGraphItemsChanged;
        public event EventHandler OnEdgesChanged;

        #endregion

        #region ctor's
        public VBGraphSurface()
        {
            InitializeComponent();
            InitVBControl();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            GraphVisible(false);
            InitVBControl();
            GraphVisible(true);
        }

        #endregion

        #region Initialization
        protected void InitVBControl()
        {
            if (!_Initialized && DataContext != null && !string.IsNullOrEmpty(VBContent))
            {
                _Initialized = true;
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcCtrlModes = Global.ControlModes.Enabled;

                if (ContextACObject != null && ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                {
                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = dcPath;
                    Bind(AvailablePathsProperty, binding);
                }

                //InitGraphSurface();
                if (ContextACObject != null)
                {
                    if (!string.IsNullOrEmpty(SelectedItems) && ContextACObject.ACUrlBinding(SelectedItems, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        Bind(ActiveObjectsProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(SelectedEdges) && ContextACObject.ACUrlBinding(SelectedEdges, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        Bind(ActiveEdgesProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(SelectedGraphAction) && ContextACObject.ACUrlBinding(SelectedGraphAction, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        Bind(GraphActionProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(EdgeRouting) && ContextACObject.ACUrlBinding(EdgeRouting, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        Bind(UseEdgeRoutingProperty, binding);
                    }
                }
            }

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.ACUrlCmdMessage;
                binding.Mode = BindingMode.OneWay;
                Bind(ACUrlCmdMessageProperty, binding);
            }
        }

        #endregion

        #region IACObject properties

        /// <summary>
        /// Parent BSO object
        /// </summary>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public string SelectedItems
        {
            get { return GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly StyledProperty<string> SelectedItemsProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(SelectedItems));

        public string SelectedEdges
        {
            get { return GetValue(SelectedEdgesProperty); }
            set { SetValue(SelectedEdgesProperty, value); }
        }

        public static readonly StyledProperty<string> SelectedEdgesProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(SelectedEdges));

        public string RelayoutMethod
        {
            get { return GetValue(RelayoutMethodProperty); }
            set { SetValue(RelayoutMethodProperty, value); }
        }

        public static readonly StyledProperty<string> RelayoutMethodProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(RelayoutMethod));

        public bool UseACCaption
        {
            get { return GetValue(UseACCaptionProperty); }
            set { SetValue(UseACCaptionProperty, value); }
        }

        public static readonly StyledProperty<bool> UseACCaptionProperty =
            AvaloniaProperty.Register<VBGraphSurface, bool>(nameof(UseACCaption));

        public double StrokeThickness
        {
            get { return GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<VBGraphSurface, double>(nameof(StrokeThickness));

        public string SelectedGraphAction
        {
            get { return GetValue(SelectedGraphActionProperty); }
            set { SetValue(SelectedGraphActionProperty, value); }
        }

        public static readonly StyledProperty<string> SelectedGraphActionProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(SelectedGraphAction));

        public string EdgeRouting
        {
            get { return GetValue(EdgeRoutingProperty); }
            set { SetValue(EdgeRoutingProperty, value); }
        }

        public static readonly StyledProperty<string> EdgeRoutingProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(EdgeRouting));

        public double GraphItemHeight
        {
            get { return GetValue(GraphItemHeightProperty); }
            set { SetValue(GraphItemHeightProperty, value); }
        }

        public static readonly StyledProperty<double> GraphItemHeightProperty =
            AvaloniaProperty.Register<VBGraphSurface, double>(nameof(GraphItemHeight), defaultValue: 60.0);

        public double GraphItemWidth
        {
            get { return GetValue(GraphItemWidthProperty); }
            set { SetValue(GraphItemWidthProperty, value); }
        }

        public static readonly StyledProperty<double> GraphItemWidthProperty =
            AvaloniaProperty.Register<VBGraphSurface, double>(nameof(GraphItemWidth), defaultValue: 200.0);

        #endregion

        #region Styled properties

        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBGraphSurface>();

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBGraphSurface, string>(nameof(VBContent));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public IEnumerable<IEnumerable<IEnumerable<IACEdge>>> AvailablePaths
        {
            get { return GetValue(AvailablePathsProperty); }
            set { SetValue(AvailablePathsProperty, value); }
        }

        public static readonly StyledProperty<IEnumerable<IEnumerable<IEnumerable<IACEdge>>>> AvailablePathsProperty =
            AvaloniaProperty.Register<VBGraphSurface, IEnumerable<IEnumerable<IEnumerable<IACEdge>>>>(nameof(AvailablePaths));

        [Category("VBControl")]
        [Bindable(true)]
        public List<IACObject> ActiveObjects
        {
            get { return GetValue(ActiveObjectsProperty); }
            set { SetValue(ActiveObjectsProperty, value); }
        }

        public static readonly StyledProperty<List<IACObject>> ActiveObjectsProperty =
            AvaloniaProperty.Register<VBGraphSurface, List<IACObject>>(nameof(ActiveObjects));

        [Category("VBControl")]
        [Bindable(true)]
        public List<IACObject> ActiveEdges
        {
            get { return GetValue(ActiveEdgesProperty); }
            set { SetValue(ActiveEdgesProperty, value); }
        }

        public static readonly StyledProperty<List<IACObject>> ActiveEdgesProperty =
            AvaloniaProperty.Register<VBGraphSurface, List<IACObject>>(nameof(ActiveEdges));

        /// <summary>
        /// Represents the styled property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBGraphSurface, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly StyledProperty<Global.GraphAction> GraphActionProperty =
            AvaloniaProperty.Register<VBGraphSurface, Global.GraphAction>(nameof(GraphAction));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.GraphAction GraphAction
        {
            get
            {
                return GetValue(GraphActionProperty);
            }
            set { SetValue(GraphActionProperty, value); }
        }

        public bool UseEdgeRouting
        {
            get { return GetValue(UseEdgeRoutingProperty); }
            set { SetValue(UseEdgeRoutingProperty, value); }
        }

        public static readonly StyledProperty<bool> UseEdgeRoutingProperty =
            AvaloniaProperty.Register<VBGraphSurface, bool>(nameof(UseEdgeRouting), defaultValue: true);

        #region Styled properties => DataTemplate, Styles

        public static readonly StyledProperty<DataTemplate> GraphItemDataTemplateProperty =
            AvaloniaProperty.Register<VBGraphSurface, DataTemplate>(nameof(GraphItemDataTemplate));

        public DataTemplate GraphItemDataTemplate
        {
            get { return GetValue(GraphItemDataTemplateProperty); }
            set { SetValue(GraphItemDataTemplateProperty, value); }
        }

        public static readonly StyledProperty<VBGraphItemDataTemplateSelector> GraphItemDataTemplateSelectorProperty =
            AvaloniaProperty.Register<VBGraphSurface, VBGraphItemDataTemplateSelector>(nameof(GraphItemDataTemplateSelector));

        public VBGraphItemDataTemplateSelector GraphItemDataTemplateSelector
        {
            get { return GetValue(GraphItemDataTemplateSelectorProperty); }
            set { SetValue(GraphItemDataTemplateSelectorProperty, value); }
        }

        public static readonly StyledProperty<ControlTheme> GraphEdgeStyleProperty =
            AvaloniaProperty.Register<VBGraphSurface, ControlTheme>(nameof(GraphEdgeStyle));

        public ControlTheme GraphEdgeStyle
        {
            get { return GetValue(GraphEdgeStyleProperty); }
            set { SetValue(GraphEdgeStyleProperty, value); }
        }

        #endregion

        #endregion

        #region Cleaning methods

        private void ClearBindings()
        {
            this.ClearAllBindings();
        }

        private void ClearProperties()
        {
            ActiveObjects = null;
            ActiveEdges = null;
            AvailablePaths = null;
            SelectedItems = null;
            SelectedEdges = null;
            RelayoutMethod = null;
        }

        #endregion

        #region Fields

        private Dictionary<IACObject, Control> _GraphItemsMap;

        private Dictionary<IACEdge, VBEdge> _EdgesMap;

        private List<IACEdge> _Edges;

        private EfficientSugiyama.SugiLayoutAlgorithm<IACObject, IACEdge> _sugiLayoutAlgorithm;

        private VBRoutingLogic _RoutingLogic;

        private short _GraphItemArrangeCounter = 0;

        private int _ModeIndex = -1;

        int _EdgesLoadedCount = 0;
        private bool _Initialized;

        #endregion

        #region IACObject

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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier { get { return this.Name; } }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption { get { return this.Name; } }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType { get; }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return Parent as IACObject; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
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

        #region Methods

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ActiveEdgesProperty && ActiveEdges != null)
                UpdateVBGraphEdgesState();
            else if (change.Property == ActiveObjectsProperty && ActiveObjects != null)
                UpdateVBGraphItemsState();
            else if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();
            else if (change.Property == AvailablePathsProperty)
                UpdateVBGraphAvailablePaths();
            if (change.Property == GraphActionProperty)
            {
                Global.GraphAction graphAction = Global.GraphAction.None;
                if (change.NewValue != null)
                {
                    Enum.TryParse(change.NewValue.ToString(), out graphAction);
                    SetGraphAction(graphAction);
                }
            }
            base.OnPropertyChanged(change);
        }

        bool isGraphVisible;
        private void GraphVisible(bool isVisible)
        {
            if (isVisible == isGraphVisible)
                return;
            isGraphVisible = isVisible;
            if (isVisible)
            {
                progressBar.IsVisible = false;
                graph.IsVisible = true;
            }
            else
            {
                progressBar.IsVisible = true;
                graph.IsVisible = false;
            }
        }

        private void InitGraphSurface()
        {
            if (AvailablePaths == null || !AvailablePaths.Any())
                return;

            this.graph.Children.Clear();

            _GraphItemsMap = new Dictionary<IACObject, Control>();
            _EdgesMap = new Dictionary<IACEdge, VBEdge>();

            _Edges = AvailablePaths.SelectMany(x => x, (k, v) => new { v }).Select(c => c.v).SelectMany(t => t, (m, n) => new { n })
                             .Select(p => p.n).GroupBy(g => new { g.SourceParent, g.TargetParent }).Select(gs => gs.First()).ToList();

            List<IACObject> vertices = _Edges.Select(c => c.SourceParent).Distinct().ToList();
            vertices.AddRange(_Edges.Select(c => c.TargetParent).Distinct());
            vertices = vertices.Distinct().OrderBy(c => c.GetACUrl()).ToList();

            VBGraphItem[] visuals = CreateVBGraphItems(vertices);
            VBGraphEdge[] routeEdges = CreateVBGraphEdges(_Edges);

            Dictionary<IACObject, Point> vertexPositions;
            IDictionary<IACEdge, Point[]> edgePositions;

            _EdgesLoadedCount = 0;

            _sugiLayoutAlgorithm = new EfficientSugiyama.SugiLayoutAlgorithm<IACObject, IACEdge>(vertices, _Edges, _GraphItemsMap, routeEdges);
            try
            {
                if (_GraphItemsMap.Count > 1)
                {
                    DateTime start = DateTime.Now;
                    _sugiLayoutAlgorithm.LayoutElements(out vertexPositions, out edgePositions);
                    DateTime end = DateTime.Now;
                    var diff = end - start;
                    SetPositionOnElements(vertexPositions, edgePositions);
                }
            }
            catch (Exception ec)
            {
                Console.WriteLine(ec.Message);
            }

            _RoutingLogic = new VBRoutingLogic(BSOACComponent);
            MarkSourcesAndTargets();
        }

        private VBGraphItem[] CreateVBGraphItems(List<IACObject> objects)
        {
            int cCount = objects.Count();
            VBGraphItem[] result = new VBGraphItem[cCount];

            for (int i = 0; i < cCount; i++)
            {
                var obj = objects[i];
                VBGraphItem graphItem = new VBGraphItem(obj, this) { Height = GraphItemHeight, Width = GraphItemWidth };
                result[i] = graphItem;
                _GraphItemsMap.Add(obj, graphItem);
                this.graph.Children.Add(graphItem);
            }
            return result;
        }

        private VBGraphEdge[] CreateVBGraphEdges(List<IACEdge> edges)
        {
            int eCount = edges.Count();
            VBGraphEdge[] result = new VBGraphEdge[eCount];

            for (int i = 0; i < eCount; i++)
            {
                IACEdge edge = edges[i];
                VBGraphEdge graphEdge = FactoryVBGraphEdge(edge);
                result[i] = graphEdge;
                _EdgesMap.Add(edge, graphEdge);
            }
            return result;
        }

        private VBGraphEdge FactoryVBGraphEdge(IACEdge edge)
        {
            string sourceACUrl = edge.SourceParent.GetACUrl();
            string targetACUrl = edge.TargetParent.GetACUrl();


            string routeIdentifier = edge.SourceParent.GetACUrl() + "--" + edge.TargetParent.GetACUrl();

            VBGraphEdge graphEdge = new VBGraphEdge(edge, this, routeIdentifier);
            graphEdge.SetConnectorNames(ACUrlHelper.GetTrimmedName(sourceACUrl) + "\\OutB", ACUrlHelper.GetTrimmedName(targetACUrl) + "\\InT");
            this.graph.Children.Add(graphEdge);
            return graphEdge;
        }

        private void SetPositionOnElements(Dictionary<IACObject, Point> vertexPositions, IDictionary<IACEdge, Point[]> edgesPositions)
        {
            double width = vertexPositions.Max(c => c.Value.X) + Math.Abs(vertexPositions.Min(c => c.Value.X)) + 400;
            double height = vertexPositions.Max(c => c.Value.Y) + 120;
            double widthCenter = width / 2;

            if (vertexPositions.All(x => x.Value.X >= 0))
                widthCenter = 100;
            else if (vertexPositions.All(c => c.Value.X <= 0))
                widthCenter = width - 300;

            //Sugiyama algorithm problem with element positions - temp fix
            CheckElementsPosition(vertexPositions);

            foreach (var item in vertexPositions)
            {
                double top = item.Value.Y;
                double left = item.Value.X + widthCenter;

                var visual = _GraphItemsMap[item.Key];
                Canvas.SetLeft(visual, left);
                Canvas.SetTop(visual, top);
            }

            this.Height = height;
            this.Width = width + 200;
        }

        private void CheckElementsPosition(Dictionary<IACObject, Point> vertexPositions)
        {
            var elements = vertexPositions.GroupBy(c => c.Value.Y);
            foreach (var group in elements)
            {
                var groupSorted = group.OrderBy(c => c.Value.X).ToArray();
                for (int i = 0; i < groupSorted.Count() - 1; i++)
                {
                    if (groupSorted[i].Value.X + 280 > groupSorted[i + 1].Value.X)
                    {
                        var item = groupSorted[i];
                        vertexPositions[item.Key] = new Point(groupSorted[i + 1].Value.X - 280, item.Value.Y);
                    }
                }
            }
        }

        protected void UpdateVBGraphAvailablePaths()
        {
            InitGraphSurface();
        }

        protected void UpdateVBGraphItemsState()
        {
            if (OnGraphItemsChanged != null)
                OnGraphItemsChanged.Invoke(this, new EventArgs());
        }

        protected void UpdateVBGraphEdgesState()
        {
            if (OnEdgesChanged != null)
                OnEdgesChanged.Invoke(this, new EventArgs());
        }

        private void MarkSourcesAndTargets()
        {
            var sources = _Edges.Where(c => _Edges.All(x => x.TargetParent != c.SourceParent)).Select(k => k.SourceParent).Distinct();
            var targets = _Edges.Where(c => _Edges.All(x => x.SourceParent != c.TargetParent)).Select(k => k.TargetParent).Distinct();

            foreach (var source in sources)
                ((VBGraphItem)_GraphItemsMap[source]).IsStartOrEndItem = true;

            foreach (var target in targets)
                ((VBGraphItem)_GraphItemsMap[target]).IsStartOrEndItem = true;
        }

        internal void OnGraphItemArranged()
        {
            _GraphItemArrangeCounter++;
            if (_GraphItemArrangeCounter >= _GraphItemsMap.Count)
            {
                if (_EdgesMap.Values.All(c => c.IsLoaded))
                {
                    foreach (var item in _EdgesMap.Values)
                        item.RedrawVBEdge(true, true);
                    RecalcEdgesRoute();
                }
                _GraphItemArrangeCounter = 0;
            }
        }

        private void RecalcEdgesRoute(bool resetEdgeRoutes = false)
        {
            if (UseEdgeRouting)
            {
                if (_RoutingLogic != null && _EdgesMap.Count < 500)
                {
                    _RoutingLogic.InitWithDesignItems(null, _GraphItemsMap.Values.ToList(), _EdgesMap.Values);
                    _RoutingLogic.CalculateEdgeRoute(this, 5, false);
                }
            }
            else if(resetEdgeRoutes)
            {
                foreach (VBEdge item in _EdgesMap.Select(c => c.Value))
                {
                    item.Points.Clear();
                    item.RedrawVBEdge(false);
                }
            }
        }

        private void Relayout()
        {
            if (_GraphItemsMap == null || _EdgesMap == null)
                return;

            Dictionary<IACObject, Point> vertexPositions;
            IDictionary<IACEdge, Point[]> edgePositions;

            _ModeIndex++;
            if (_ModeIndex > 3)
                _ModeIndex = -1;

            if (_GraphItemsMap.Count > 1)
            {
                DateTime start = DateTime.Now;
                _sugiLayoutAlgorithm = new EfficientSugiyama.SugiLayoutAlgorithm<IACObject, IACEdge>(_GraphItemsMap.Keys, _EdgesMap.Keys, _GraphItemsMap, _EdgesMap.Values);
                _sugiLayoutAlgorithm.LayoutElements(out vertexPositions, out edgePositions, _ModeIndex);
                DateTime end = DateTime.Now;
                var diff = end - start;
                SetPositionOnElements(vertexPositions, edgePositions);
            }

            foreach (var item in _GraphItemsMap.Values)
                ((VBGraphItem)item).ArrangeConnectors();
        }

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (!this.IsLoaded)
                return;
            var acUrlMessage = ACUrlCmdMessage;
            if (acUrlMessage == null || acUrlMessage.ACUrl == null || acUrlMessage.TargetVBContent == null)
                return;
            Global.GraphAction graphAction = Global.GraphAction.None;
            Enum.TryParse(acUrlMessage.TargetVBContent, out graphAction);
            Global.GraphAction[] hanldedGraphActions = new Global.GraphAction[]
            {
                Global.GraphAction.InitVBControl,
                Global.GraphAction.StartGraphProgress,
                 Global.GraphAction.CleanUpGraph,
                 Global.GraphAction.RecalcEdgesRoute,
                 Global.GraphAction.Relayout
            };

            if (acUrlMessage.ACUrl == RelayoutMethod && hanldedGraphActions.Contains(graphAction))
                SetGraphAction(graphAction);
            if (acUrlMessage.ACUrl == RelayoutMethod && acUrlMessage.TargetVBContent == this.VBContent)
                SetGraphAction(Global.GraphAction.Relayout);
        }

        internal void OnVBGraphEdgeLoaded()
        {
            if(_EdgesLoadedCount != -1)
                _EdgesLoadedCount++;

            if (_EdgesMap != null && _EdgesLoadedCount >= _EdgesMap.Values.Count && _EdgesLoadedCount != -1)
            {
                RecalcEdgesRoute();
                _EdgesLoadedCount = -1;
            }
        }

        private void SetGraphAction(Global.GraphAction graphAction)
        {
            if (graphAction == Global.GraphAction.InitVBControl)
            {
                GraphVisible(false);
                _Initialized = false;
                ClearBindings();
                graph.Children.Clear();
                ClearProperties();
                InitVBControl();
                GraphVisible(true);
            }
            if (graphAction == Global.GraphAction.InitGraphSurface)
            {
                GraphVisible(false);
                graph.Children.Clear();
                InitGraphSurface();
                GraphVisible(true);
            }
            if (graphAction == Global.GraphAction.StartGraphProgress)
            {
                this.Height = 400;
                this.Width = 800;
                GraphVisible(false);
            }
            if (graphAction == Global.GraphAction.CleanUpGraph)
            {
                GraphVisible(false);
                graph.Children.Clear();
                GraphVisible(true);
            }
            if (graphAction == Global.GraphAction.RecalcEdgesRoute)
            {
                RecalcEdgesRoute(true);
            }
            if (graphAction == Global.GraphAction.Relayout)
            {
                Relayout();
            }
        }

        #endregion

    }
}
