using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;

namespace gip.core.layoutengine
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
        static VBGraphSurface()
        {

        }

        public VBGraphSurface()
        {
            InitializeComponent();
            InitVBControl();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

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
                    binding.Path = new PropertyPath(dcPath);
                    SetBinding(AvailablePathsProperty, binding);
                }

                InitGraphSurface();
                if (ContextACObject != null)
                {
                    if (!string.IsNullOrEmpty(SelectedItems) && ContextACObject.ACUrlBinding(SelectedItems, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = new PropertyPath(dcPath);
                        SetBinding(ActiveObjectsProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(SelectedEdges) && ContextACObject.ACUrlBinding(SelectedEdges, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = new PropertyPath(dcPath);
                        SetBinding(ActiveEdgesProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(SelectedGraphAction) && ContextACObject.ACUrlBinding(SelectedGraphAction, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = new PropertyPath(dcPath);
                        SetBinding(GraphActionProperty, binding);
                    }
                    if (!string.IsNullOrEmpty(EdgeRouting) && ContextACObject.ACUrlBinding(EdgeRouting, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcCtrlModes))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = new PropertyPath(dcPath);
                        SetBinding(UseEdgeRoutingProperty, binding);
                    }
                }
            }

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                binding.Mode = BindingMode.OneWay;
                SetBinding(ACUrlCmdMessageProperty, binding);
            }
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            GraphVisible(false);
            InitVBControl();
            GraphVisible(true);
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

        [Category("VBControl")]
        public string SelectedItems
        {
            get;
            set;
        }

        [Category("VBControl")]
        public string SelectedEdges
        {
            get;
            set;
        }

        [Category("VBControl")]
        public string RelayoutMethod
        {
            get; set;
        }

        [Category("VBControl")]
        public bool UseACCaption
        {
            get; set;
        }

        [Category("VBControl")]
        public double StrokeThickness
        {
            get; set;
        }

        [Category("VBControl")]
        public string SelectedGraphAction
        {
            get; set;
        }

        [Category("VBControl")]
        public string EdgeRouting
        {
            get;set;
        }

        [DefaultValue(60)]
        public double GraphItemHeight { get; set; }

        [DefaultValue(200)]
        public double GraphItemWidth { get; set; }

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBGraphSurface), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBGraphSurface));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public IEnumerable<IEnumerable<IEnumerable<IACEdge>>> AvailablePaths
        {
            get { return (IEnumerable<IEnumerable<IEnumerable<IACEdge>>>)GetValue(AvailablePathsProperty); }
            set { SetValue(AvailablePathsProperty, value); }
        }

        public static readonly DependencyProperty AvailablePathsProperty =
            DependencyProperty.Register("AvailablePaths", typeof(IEnumerable<IEnumerable<IEnumerable<IACEdge>>>), typeof(VBGraphSurface), new PropertyMetadata());

        [Category("VBControl")]
        [Bindable(true)]
        public List<IACObject> ActiveObjects
        {
            get { return (List<IACObject>)GetValue(ActiveObjectsProperty); }
            set { SetValue(ActiveObjectsProperty, value); }
        }

        public static readonly DependencyProperty ActiveObjectsProperty =
            DependencyProperty.Register("ActiveObjects", typeof(List<IACObject>), typeof(VBGraphSurface), new PropertyMetadata(OnDepPropChanged));

        [Category("VBControl")]
        [Bindable(true)]
        public List<IACObject> ActiveEdges
        {
            get { return (List<IACObject>)GetValue(ActiveEdgesProperty); }
            set { SetValue(ActiveEdgesProperty, value); }
        }

        public static readonly DependencyProperty ActiveEdgesProperty =
            DependencyProperty.Register("ActiveEdges", typeof(List<IACObject>), typeof(VBGraphSurface), new PropertyMetadata(OnDepPropChanged));

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage", typeof(ACUrlCmdMessage), typeof(VBGraphSurface), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly  DependencyProperty GraphActionProperty
            = DependencyProperty.Register("GraphAction", typeof(Global.GraphAction), typeof(VBGraphSurface), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.GraphAction GraphAction
        {
            get
            {
                return (Global.GraphAction)GetValue(GraphActionProperty);
            }
            set { SetValue(GraphActionProperty, value); }
        }

        public bool UseEdgeRouting
        {
            get { return (bool)GetValue(UseEdgeRoutingProperty); }
            set { SetValue(UseEdgeRoutingProperty, value); }
        }

        public static readonly DependencyProperty UseEdgeRoutingProperty =
            DependencyProperty.Register("UseEdgeRouting", typeof(bool), typeof(VBGraphSurface), new PropertyMetadata(true));

        #region Dependency properties => DataTemplate, Styles

        public static readonly DependencyProperty GraphItemDataTemplateProperty =
            DependencyProperty.Register("GraphItemDataTemplate", typeof(DataTemplate), typeof(VBGraphSurface));
        public DataTemplate GraphItemDataTemplate
        {
            get { return (DataTemplate)GetValue(GraphItemDataTemplateProperty); }
            set { SetValue(GraphItemDataTemplateProperty, value); }
        }

        public static readonly DependencyProperty GraphItemDataTemplateSelectorProperty =
            DependencyProperty.Register("GraphItemDataTemplateSelector", typeof(DataTemplateSelector), typeof(VBGraphSurface));
        public DataTemplateSelector GraphItemDataTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(GraphItemDataTemplateSelectorProperty); }
            set { SetValue(GraphItemDataTemplateSelectorProperty, value); }
        }

        public static readonly DependencyProperty GraphEdgeStyleProperty =
            DependencyProperty.Register("GraphEdgeStyle", typeof(Style), typeof(VBGraphSurface));
        public Style GraphEdgeStyle
        {
            get { return (Style)GetValue(GraphEdgeStyleProperty); }
            set { SetValue(GraphEdgeStyleProperty, value); }
        }

        #endregion

        #endregion

        #region Cleaning methods

        private void ClearBindings()
        {

            BindingOperations.ClearBinding(this, ActiveEdgesProperty);
            BindingOperations.ClearBinding(this, ActiveObjectsProperty);
            BindingOperations.ClearBinding(this, AvailablePathsProperty);
            BindingOperations.ClearBinding(this, ACUrlCmdMessageProperty);
            BindingOperations.ClearAllBindings(this);
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

        private Dictionary<IACObject, FrameworkElement> _GraphItemsMap;

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

        #endregion

        #region Methods

        public static void OnDepPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (dependencyObject is VBGraphSurface)
            {
                VBGraphSurface surface = dependencyObject as VBGraphSurface;
                if (surface == null)
                    return;
                if (args.Property == ActiveEdgesProperty && surface.ActiveEdges != null)
                    surface.UpdateVBGraphEdgesState();
                else if (args.Property == ActiveObjectsProperty && surface.ActiveObjects != null)
                    surface.UpdateVBGraphItemsState();
                else if (args.Property == ACUrlCmdMessageProperty)
                    surface.OnACUrlMessageReceived();
                if (args.Property == GraphActionProperty)
                {
                    Global.GraphAction graphAction = Global.GraphAction.None;
                    if (args.NewValue != null)
                    {
                        Enum.TryParse(args.NewValue.ToString(), out graphAction);
                        surface.SetGraphAction(graphAction);
                    }
                }
            }
        }

        bool isGraphVisible;
        private void GraphVisible(bool isVisible)
        {
            if (isVisible == isGraphVisible)
                return;
            isGraphVisible = isVisible;
            if (isVisible)
            {
                progressBar.Visibility = Visibility.Collapsed;
                graph.Visibility = Visibility.Visible;
            }
            else
            {
                progressBar.Visibility = Visibility.Visible;
                graph.Visibility = Visibility.Collapsed;
            }
        }

        private void InitGraphSurface()
        {
            if (AvailablePaths == null || !AvailablePaths.Any())
                return;

            

            _GraphItemsMap = new Dictionary<IACObject, FrameworkElement>();
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

            _RoutingLogic = new VBRoutingLogic();
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
