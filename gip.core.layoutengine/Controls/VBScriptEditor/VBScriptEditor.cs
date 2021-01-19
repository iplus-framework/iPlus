using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Markup;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Windows.Threading;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.IO;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBScriptEditor'}de{'VBScriptEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBScriptEditor : RoslynCodeEditor, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
        #region c'tors

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "TextEditorStyleGip",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextEditor/Themes/TextEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "TextEditorStyleAero",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextEditor/Themes/TextEditorStyleAero.xaml" },
        };
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBScriptEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBScriptEditor), new FrameworkPropertyMetadata(typeof(VBScriptEditor)));
        }

        public VBScriptEditor()
            : base()
        {
            
        }

        #endregion

        #region fields

        bool _themeApplied = false;
        protected DispatcherTimer _FoldingUpdateTimer;
        protected CommandBinding _CmdBindingFind;
        protected InputBinding _ibFind;
        protected bool _Loaded = false;
        protected short _BSOACComponentSubscr = 0;
        protected FoldingManager foldingManager;
        protected VBScriptEditorBraceFoldingStrategy foldingStrategy;
        private bool _OnTargetUpdated = false;
        private RoslynHost _roslynHost;
        IClassificationHighlightColors _classificationHighlightColors;
        private List<Assembly> _roslynAssembly;
        private string _referencesArea;
        DocumentId _docId;
        string[] _roslynReferences;

        #endregion

        #region Init/Deinit

        protected override void OnInitialized(EventArgs e)
        {
            ShowLineNumbers = true;

            _FoldingUpdateTimer = new DispatcherTimer();
            _FoldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _FoldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            _FoldingUpdateTimer.Start();

            base.OnInitialized(e);
            //ActualizeTheme(true);
        }

        public override void OnApplyTemplate()
        {
            if (this.Style == null)
            {
                if (ControlManager.WpfTheme == eWpfTheme.Aero)
                    Style = ControlManager.GetStyleOfTheme(StyleInfoList);
            }
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            InitVBControl();
        }

        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        protected virtual void InitVBControl()
        {
            if (_Loaded || (ContextACObject == null))
                return;
            _Loaded = true;

            AttachVBFindAndReplace();

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", this.GetType().Name, VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (Visibility == Visibility.Visible)
            {
                if (RightControlMode < Global.ControlModes.Disabled)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Binding binding = new Binding();
                    binding.Source = dcSource;
                    binding.Path = new PropertyPath(dcPath);
                    binding.NotifyOnSourceUpdated = true;
                    binding.NotifyOnTargetUpdated = true;
                    if (VBContentPropertyInfo != null)
                        binding.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;

                    this.SetBinding(VBScriptEditor.VBTextProperty, binding);

                    this.TargetUpdated += OnTargetUpdatedOfBinding;
                    this.SourceUpdated += OnSourceUpdatedOfBinding;

                    _ibFind = new InputBinding(ApplicationCommands.Find, new KeyGesture(Key.F, ModifierKeys.Control));
                    this.InputBindings.Add(_ibFind);
                    _CmdBindingFind = new CommandBinding(ApplicationCommands.Find);
                    _CmdBindingFind.Executed += OpenFindAndReplace;
                    _CmdBindingFind.CanExecute += CanOpenFindAndReplace;
                    this.CommandBindings.Add(_CmdBindingFind);

                    if (BSOACComponent != null)
                    {
                        binding = new Binding();
                        binding.Source = BSOACComponent;
                        binding.Path = new PropertyPath(Const.InitState);
                        binding.Mode = BindingMode.OneWay;
                        SetBinding(VBScriptEditor.ACCompInitStateProperty, binding);
                    }

                    _roslynAssembly = new List<Assembly>();
                    _roslynAssembly.Add(Assembly.Load("RoslynPad.Roslyn.Windows"));
                    _roslynAssembly.Add(Assembly.Load("RoslynPad.Editor.Windows"));

                    if (ControlManager.WpfTheme == eWpfTheme.Gip)
                        _classificationHighlightColors = new ClassificationHighlightColorsGip();
                    else
                        _classificationHighlightColors = new ClassificationHighlightColors();

                    if (this.VBText == null)
                        this.VBText = "";

                    var assemblyPath = Directory.GetFiles(AppContext.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly);
                    try
                    {
                        _roslynHost = new RoslynHost(_roslynAssembly, RoslynHostReferences.DesktopDefault); 
                    }
                    catch (Exception e)
                    {
                        this.Root().Messages.LogException("VBScriptEditor","InitVBControl0010",e);
                        this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0020", e.StackTrace);
                        if (e.InnerException != null)
                        {
                            this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0030", e.InnerException.Message);
                            this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0040", e.InnerException.StackTrace);
                        }
                        try
                        {
                            _roslynHost = new RoslynHost(_roslynAssembly, RoslynHostReferences.DesktopDefault);
                        }
                        catch (Exception ec)
                        {
                            this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0050", ec);
                            this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0060", ec.StackTrace);
                            if (e.InnerException != null)
                            {
                                this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0070", ec.InnerException.Message);
                                this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0080", ec.InnerException.StackTrace);
                            }
                        }
                    }
                    try
                    {
                        if(_roslynHost != null)
                            _docId = this.Initialize(_roslynHost, _classificationHighlightColors, AppContext.BaseDirectory, this.VBText);
                    }
                    catch (Exception e)
                    {
                        this.Root().Messages.LogException("VBScriptEditor", "InitVBControl0020", e);
                    }

                    foldingStrategy = new VBScriptEditorBraceFoldingStrategy();
                    foldingManager = FoldingManager.Install(TextArea);
                }
            }

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                    IsEnabled = false;
                else
                    UpdateControlMode();
            }
            if (AutoFocus)
                Focus();
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
            if (!_Loaded)
                return;

            _Loaded = false;
            if (_FoldingUpdateTimer != null)
            {
                _FoldingUpdateTimer.Stop();
                _FoldingUpdateTimer.Tick -= foldingUpdateTimer_Tick;
                _FoldingUpdateTimer = null;
            }

            if (_CmdBindingFind != null)
            {
                _CmdBindingFind.Executed -= OpenFindAndReplace;
                _CmdBindingFind.CanExecute -= CanOpenFindAndReplace;
                this.CommandBindings.Remove(_CmdBindingFind); //handle paste
            }
            if (_ibFind != null)
                this.InputBindings.Remove(_ibFind);

            BindingOperations.ClearBinding(this, VBScriptEditor.VBTextProperty);
            BindingOperations.ClearBinding(this, VBScriptEditor.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
            _VBContentPropertyInfo = null;

            if (_VBFindAndReplace != null && _VBFindAndReplace.ReferencePoint != null)
            {
                _VBFindAndReplace.ReferencePoint.Remove(this);
                
                FindAndReplaceHandler handler = _VBFindAndReplace.ACUrlCommand("FindAndReplaceHandler") as FindAndReplaceHandler;
                if (handler != null)
                    handler.TextArea = null;
            }
            _VBFindAndReplace = null;
            foldingManager = null;
            foldingStrategy = null;
        }

        #endregion

        #region Properties

        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        [Category("VBControl")]
        public int EncodingCodePage
        {
            get
            {
                return Encoding.CodePage;
            }
            set
            {
                this.Encoding = Encoding.GetEncoding(value);
            }
        }

        public static readonly DependencyProperty VBTextProperty
            = DependencyProperty.Register("VBText", typeof(string), typeof(VBScriptEditor));

        [Category("VBControl")]
        public string VBText
        {
            get
            {
                return (string)GetValue(VBTextProperty);
            }
            set
            {
                SetValue(VBTextProperty, value);
            }
        }

        protected IACComponent _VBFindAndReplace;
        public IACComponent VBBSOFindAndReplace
        {
            get
            {
                if (_VBFindAndReplace != null)
                    return _VBFindAndReplace;
                InstanceVBFindAndReplace();
                return _VBFindAndReplace;
            }
        }

        #endregion

        #region Methods

        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (foldingStrategy != null)
            {
                int firstErrorOffset;
                IEnumerable<NewFolding> foldings = foldingStrategy.CreateNewFoldings(Document, out firstErrorOffset);
                foldingManager.UpdateFoldings(foldings, firstErrorOffset);
            }
            if (IsAllowedCheckReferences())
                CheckReferences();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (_OnTargetUpdated == true)
            {
                _OnTargetUpdated = false;
                return;
            }
            this.VBText = Text;
            base.OnTextChanged(e);
        }

        public void OnTargetUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            // Falls BSO-Content geändert durch Navigation
            if (args.Property == VBScriptEditor.VBTextProperty)
            {
                // Aktualisiere AvalonText-Editor
                _OnTargetUpdated = true;
                Text = this.VBText;
            }
        }

        public void OnSourceUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBScriptEditor.VBTextProperty)
            {
            }
        }

        protected virtual void InstanceVBFindAndReplace()
        {
            if (_VBFindAndReplace != null)
                return;
            VBDesignBase vbDesign = this.GetVBDesignBase();
            // wird benötigt, falls gekapselt im UserControl
            if (vbDesign == null)
                vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesignBase)) as VBDesignBase;
            if (vbDesign != null)
            {
                FindAndReplaceHandler handler = new FindAndReplaceHandler(TextArea);
                _VBFindAndReplace = vbDesign.GetACComponentByKey(BSOACComponent, VBBSOFindAndReplaceID, new object[] { handler });
                // Falls nicht erzeugt, weil DataContext ein Sub-BSO ist, dann gehe zum Haupt-BSO
                if ((_VBFindAndReplace == null) && (BSOACComponent.ParentACComponent != null))
                    _VBFindAndReplace = vbDesign.GetACComponentByKey(BSOACComponent.ParentACComponent, VBBSOFindAndReplaceID, new object[] { handler });
                if (_VBFindAndReplace != null && _VBFindAndReplace.ReferencePoint != null)
                {
                    FindAndReplaceHandler handler2 = _VBFindAndReplace.ACUrlCommand("FindAndReplaceHandler") as FindAndReplaceHandler;
                    if (handler != handler2)
                        handler.UngegisterEvents();
                    _VBFindAndReplace.ReferencePoint.Add(this);
                    _VBFindAndReplace.ACUrlCommand("!ShowFindAndReplaceDialog", this);
                }
                SetSelectedTextToCombo();
            }
        }

        protected virtual void AttachVBFindAndReplace()
        {
            if (_VBFindAndReplace != null)
                return;
            VBDesignBase vbDesign = this.GetVBDesignBase();
            // wird benötigt, falls gekapselt im UserControl
            if (vbDesign == null)
                vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesignBase)) as VBDesignBase;
            if (vbDesign != null)
            {
                _VBFindAndReplace = vbDesign.GetACComponentByKey(BSOACComponent, VBBSOFindAndReplaceID, null, true);
                // Falls nicht erzeugt, weil DataContext ein Sub-BSO ist, dann gehe zum Haupt-BSO
                if ((_VBFindAndReplace == null) && (BSOACComponent.ParentACComponent != null))
                    _VBFindAndReplace = vbDesign.GetACComponentByKey(BSOACComponent.ParentACComponent, VBBSOFindAndReplaceID, null, true);
                if (_VBFindAndReplace != null && _VBFindAndReplace.ReferencePoint != null)
                {
                    _VBFindAndReplace.ReferencePoint.Add(this);
                    FindAndReplaceHandler handler = _VBFindAndReplace.ACUrlCommand("FindAndReplaceHandler") as FindAndReplaceHandler;
                    if (handler != null)
                    {
                        handler.TextArea = TextArea;
                    }
                }
            }
        }

        protected virtual string VBBSOFindAndReplaceID
        {
            get
            {
                return "VBBSOFindAndReplace(" + this.GetType().Name + ")";
            }
        }

        private void CanOpenFindAndReplace(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_VBFindAndReplace != null && _VBFindAndReplace.InitState == ACInitState.Destructed)
                _VBFindAndReplace = null;

            if (_VBFindAndReplace == null)
                e.CanExecute = true;
            else
            {
                e.CanExecute = false;
                SetSelectedTextToCombo();
            }
            e.Handled = true;
        }

        private void OpenFindAndReplace(object sender, ExecutedRoutedEventArgs e)
        {
            InstanceVBFindAndReplace();
        }

        private void SetSelectedTextToCombo()
        {
            if (_VBFindAndReplace != null)
            {
                _VBFindAndReplace.ACUrlCommand("!UpdateFindTextFromSelection");
            }
        }

        protected bool Visible
        {
            get
            {
                return Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Visibility = Visibility.Hidden;
                }
            }
        }

        protected bool Enabled
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject == null)
                    {
                        IsEnabled = true;
                    }
                    else
                    {
                        IsEnabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                }
                else
                {
                    IsEnabled = false;
                }
                ControlModeChanged();
            }
        }

        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            Enabled = controlMode >= Global.ControlModes.Enabled;
            Visible = controlMode >= Global.ControlModes.Disabled;
        }

        public void ControlModeChanged()
        {
            if (Enabled)
            {
                if (VBContentPropertyInfo == null)
                {
                    ControlMode = Global.ControlModes.Enabled;
                }
                else
                {
                    if ((VBContentPropertyInfo as ACClassProperty).IsNullable)
                    {
                        ControlMode = Global.ControlModes.Enabled;
                    }
                    else
                    {
                        ControlMode = Global.ControlModes.EnabledRequired;
                    }
                }
            }
            else
            {
                ControlMode = Global.ControlModes.Disabled;
            }
        }

        private void CheckReferences()
        {
            if (_roslynHost == null || _docId == null)
                return;

            if (!string.IsNullOrEmpty(VBText))
            {
                string preCompStart = "/// <Precompiler>";
                string preCompEnd = "/// </Precompiler>";

                int startPosition = VBText.IndexOf(preCompStart);
                int endPosition = VBText.IndexOf(preCompEnd);

                string precompilerRegion = VBText.Substring(startPosition + preCompStart.Length, endPosition - startPosition - preCompStart.Length).Trim();
                _referencesArea = precompilerRegion;
                IEnumerable<string> precompilierLines = precompilerRegion.Split(new string[] { ";" }, StringSplitOptions.None);
                List<string> assemblyPath = new List<string>();
                string assemblyStringFormat = "{0}{1}.dll";

                foreach (string line in precompilierLines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    string lineTemp = line.Trim();

                    if (lineTemp.StartsWith("using"))
                    {
                        string path = string.Format("{0}{1}.dll", AppContext.BaseDirectory, lineTemp.Replace("using", "").Replace(";", "").Trim());
                        if (File.Exists(path) && !assemblyPath.Contains(path))
                            assemblyPath.Add(path);
                    }
                    else if (lineTemp.StartsWith("/// refassembly"))
                    {
                        if (lineTemp.EndsWith(".dll") || lineTemp.EndsWith(".dll;"))
                            assemblyStringFormat = "{0}{1}"; 

                        string path = string.Format(assemblyStringFormat, AppContext.BaseDirectory, lineTemp.Replace("/// refassembly", "").Replace(";", "").Trim());
                        try
                        {
                            if (path.Contains(System.Environment.NewLine))
                                continue;

                            if (File.Exists(path) && !assemblyPath.Contains(path))
                                assemblyPath.Add(path);
                            else
                            {
                                string dllName = lineTemp.Replace("/// refassembly", "").Replace(";", "").Trim();
                                path = Directory.EnumerateFiles(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), dllName, SearchOption.AllDirectories)
                                                              .FirstOrDefault();

                                if (File.Exists(path) && !assemblyPath.Contains(path))
                                    assemblyPath.Add(path);
                            }
                        }
                        catch (Exception e)
                        {
                            this.Root().Messages.LogException("VBScriptEditor", "CheckReferences0010", e);
                            this.Root().Messages.LogException("VBScriptEditor", "CheckReferences0020", e.StackTrace);
                            if (e.InnerException != null)
                            {
                                this.Root().Messages.LogException("VBScriptEditor", "CheckReferences0030", e.InnerException.Message);
                                this.Root().Messages.LogException("VBScriptEditor", "CheckReferences0040", e.InnerException.StackTrace);
                            }
                        }
                    }
                }

                string[] refsToRemove = Array.Empty<string>();
                string[] refsToAdd = Array.Empty<string>();
                if(_roslynReferences != null)
                {
                    refsToRemove = _roslynReferences.Except(assemblyPath).ToArray();
                    refsToAdd = assemblyPath.Except(_roslynReferences).ToArray();
                }
                else
                {
                    refsToAdd = assemblyPath.ToArray();
                }

                if (_roslynReferences == null || refsToAdd.Any() || refsToRemove.Any())
                {
                    _roslynReferences = assemblyPath.ToArray();
                    var document = _roslynHost.GetDocument(_docId);
                    if(document != null)
                    {
                        Project project = document.Project;
                        var refToRemove = project.MetadataReferences.Where(c => refsToRemove.Any(x => c.Display == x)).ToArray();
                        foreach(var refRem in refToRemove)
                        {
                            project = project.RemoveMetadataReference(refRem);
                        }

                        var refAssemblies = refsToAdd.Select(x => _roslynHost.CreateMetadataReference(x)).ToArray();
                        project = project.AddMetadataReferences(refAssemblies);

                        _roslynHost.UpdateDocument(project.Documents.FirstOrDefault());
                    }
                }
            }
        }

        private bool IsAllowedCheckReferences()
        {
            if (string.IsNullOrEmpty(VBText))
                return false;

            string preCompStart = "/// <Precompiler>";
            string preCompEnd = "/// </Precompiler>";

            int startPosition = VBText.IndexOf(preCompStart);
            int endPosition = VBText.IndexOf(preCompEnd);

            if (startPosition < 0 || endPosition <= 0)
                return false;

            string precompilerRegion = VBText.Substring(startPosition + preCompStart.Length, endPosition - startPosition - preCompStart.Length).Trim();

            if (precompilerRegion.Split(';').Count() - 1 != precompilerRegion.Count(c => c == ';'))
                return false;

            return precompilerRegion != _referencesArea;
        }

        #endregion

        #region IVBContent members

        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBScriptEditor));

        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        protected IACType _VBContentPropertyInfo = null;
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentPropertyInfo as ACClassProperty;
            }
        }

        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }

        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBScriptEditor));

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
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }


        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBScriptEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBScriptEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBScriptEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBScriptEditor thisControl = dependencyObject as VBScriptEditor;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
            //else if (args.Property == ACUrlCmdMessageProperty)
            //    thisControl.OnACUrlMessageReceived();
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                    }
                    break;
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            return this.ReflectIsEnabledACUrlCommand(acCommand.GetACUrl(), null);
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return ACUrlHelper.BuildACNameForGUI(this, Name);
            }

            set
            {
                Name = value;
            }
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
            switch (acUrl)
            {
                // Event
                case Const.EventDeInit:
                    if ((acParameter != null) && (acParameter[0] != null))
                    {
                        if ((_VBFindAndReplace != null) && (acParameter[0] == _VBFindAndReplace) && _VBFindAndReplace.ReferencePoint != null)
                        {
                            _VBFindAndReplace.ReferencePoint.Remove(this);
                            _VBFindAndReplace = null;
                        }
                    }
                    return null;
            }

            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            switch (acUrl)
            {
                case Const.EventDeInit:
                    return true;
            }
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
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

        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBScriptEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        [Category("VBControl")]
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        #endregion

        #region IACMenuBuilder Member
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
        }
        #endregion

    }
}
