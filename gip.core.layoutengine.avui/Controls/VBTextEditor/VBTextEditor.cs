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
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Steuerelement zur bearbeitung einfacher Texte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextEditor'}de{'VBTextEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextEditor : TextEditor, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TextEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextEditor/Themes/TextEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TextEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTextEditor/Themes/TextEditorStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of the custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBTextEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTextEditor), new FrameworkPropertyMetadata(typeof(VBTextEditor)));
        }

        bool _themeApplied = false;

        /// <summary>
        /// Creates a new instance of the VBTextEditor.
        /// </summary>
        public VBTextEditor()
            : base(new VBTextArea())
        {
        }

        /// <summary>
        /// Handles the OnInitialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            ShowLineNumbers = true;
            TextChanged += ucAvalonTextEditor_ContentChanged;
            TextArea.TextEntering += ucAvalonTextEditor_TextArea_TextEntering;
            TextArea.TextEntered += ucAvalonTextEditor_TextArea_TextEntered;

            _FoldingUpdateTimer = new DispatcherTimer();
            _FoldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _FoldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            _FoldingUpdateTimer.Start();

            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overrides the OnApplyTemplate method and runs the VBControl initialization.
        /// </summary>
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

        /// <summary>
        /// Actualizes the theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        protected DispatcherTimer _FoldingUpdateTimer;
        protected CommandBinding _CmdBindingFind;
        protected InputBinding _ibFind;
        protected bool _Loaded = false;
        protected short _BSOACComponentSubscr = 0;
        private bool _isOneWayBindingVBTextDP = false;
        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
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

                    if (binding.Mode == BindingMode.OneWay)
                        _isOneWayBindingVBTextDP = true;

                    this.SetBinding(VBTextEditor.VBTextProperty, binding);

                    this.TargetUpdated += OnTargetUpdatedOfBinding;
                    this.SourceUpdated += OnSourceUpdatedOfBinding;

                    ChangeSyntaxHighlighting();

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
                        SetBinding(VBTextEditor.ACCompInitStateProperty, binding);
                    }
                }
            }

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                {
                    IsEnabled = false;
                }
                else
                {
                    UpdateControlMode();
                }
            }
            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
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
                _FoldingUpdateTimer = null;
            }
            TextChanged -= ucAvalonTextEditor_ContentChanged;
            TextArea.TextEntering -= ucAvalonTextEditor_TextArea_TextEntering;
            TextArea.TextEntered -= ucAvalonTextEditor_TextArea_TextEntered;
            if (_CmdBindingFind != null)
            {
                _CmdBindingFind.Executed -= OpenFindAndReplace;
                _CmdBindingFind.CanExecute -= CanOpenFindAndReplace;
                this.CommandBindings.Remove(_CmdBindingFind); //handle paste
            }
            if (_ibFind != null)
                this.InputBindings.Remove(_ibFind);

            BindingOperations.ClearBinding(this, VBTextEditor.VBTextProperty);
            //BindingOperations.ClearBinding(this, VBTextEditor.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBTextEditor.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
            _VBContentPropertyInfo = null;

            if (_VBFindAndReplace != null && _VBFindAndReplace.ReferencePoint != null)
            {
                _VBFindAndReplace.ReferencePoint.Remove(this);
                FindAndReplaceHandler handler = _VBFindAndReplace.ACUrlCommand("FindAndReplaceHandler") as FindAndReplaceHandler;
                if (handler != null)
                {
                    handler.TextArea = null;
                }
            }
            _VBFindAndReplace = null;
            completionWindow = null;
            foldingManager = null;
            foldingStrategy = null;

        }


        protected CompletionWindow completionWindow;

        protected virtual void ucAvalonTextEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
        }

        protected virtual void ucAvalonTextEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
        }

        protected FoldingManager foldingManager;
        protected XmlFoldingStrategy foldingStrategy;
        protected virtual void ChangeSyntaxHighlighting()
        {
        }

        protected virtual void foldingUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (foldingStrategy != null)
            {
                foldingStrategy.UpdateFoldings(foldingManager, Document);
            }
        }


        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        void ElementACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith(Const.ACState))
            {
                if (e.PropertyName.Length < 8)
                {
                    UpdateControlMode();
                }
                else
                {
                    if (VBContent == e.PropertyName.Substring(8))
                    {
                        UpdateControlMode();
                    }
                }
            }
        }

        public bool AutoFocus { get; set; }

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
            = DependencyProperty.Register("VBText", typeof(string), typeof(VBTextEditor));

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

        private bool _OnTargetUpdated = false;

        // Wird aufgerufen, wenn Text im Texteditor verändert wurde
        protected void ucAvalonTextEditor_ContentChanged(object sender, EventArgs e)
        {
            // Falls Inhalt geändert durch Zuweisung aus BSO
            if (_OnTargetUpdated == true)
            {
                _OnTargetUpdated = false;
                return;
            }

            if (_isOneWayBindingVBTextDP)
                return;

            this.VBText = Text;
            //ACObject.ValueChanged(VBContent);
        }


        public void OnTargetUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            // Falls BSO-Content geändert durch Navigation
            if (args.Property == VBTextEditor.VBTextProperty)
            {
                // Aktualisiere AvalonText-Editor
                _OnTargetUpdated = true;
                Text = this.VBText;
            }
        }

        public void OnSourceUpdatedOfBinding(Object sender, DataTransferEventArgs args)
        {
            if (args.Property == VBTextEditor.VBTextProperty)
            {
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
            if (_VBFindAndReplace != null &&_VBFindAndReplace.InitState == ACInitState.Destructed)
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

        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBTextEditor));

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

        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        public string DisabledModes
        {
            get;
            set;
        }

        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBTextEditor));

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


        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTextEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBTextEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBTextEditor),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBTextEditor thisControl = dependencyObject as VBTextEditor;
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBTextEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

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
