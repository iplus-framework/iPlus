using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using Avalonia.Threading;
using AvaloniaEdit;
using Avalonia.Labs.Input;
using Avalonia.Input;
using Avalonia.Data;
using Avalonia;
using Avalonia.Interactivity;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.CodeCompletion;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Steuerelement zur bearbeitung einfacher Texte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextEditor'}de{'VBTextEditor'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextEditor : TextEditor, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
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
        protected override void OnInitialized()
        {
            ShowLineNumbers = true;
            TextChanged += ucAvalonTextEditor_ContentChanged;
            TextArea.TextEntering += ucAvalonTextEditor_TextArea_TextEntering;
            TextArea.TextEntered += ucAvalonTextEditor_TextArea_TextEntered;

            _FoldingUpdateTimer = new DispatcherTimer();
            _FoldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _FoldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
            _FoldingUpdateTimer.Start();

            base.OnInitialized();
        }


        /// <summary>
        /// Overrides the OnApplyTemplate method and runs the VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }


        protected DispatcherTimer _FoldingUpdateTimer;
        protected CommandBinding _CmdBindingFind;
        protected KeyBinding _ibFind;
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

            if (IsVisible)
            {
                if (RightControlMode < Global.ControlModes.Disabled)
                {
                    IsVisible = false;
                }
                else
                {
                    var binding = new Binding
                    {
                        Source = dcSource,
                        Path = dcPath,
                        Mode = VBContentPropertyInfo != null && VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
                    };

                    if (binding.Mode == BindingMode.OneWay)
                        _isOneWayBindingVBTextDP = true;

                    this.Bind(VBTextEditor.VBTextProperty, binding);

                    ChangeSyntaxHighlighting();

                    _ibFind = new KeyBinding { Command = ApplicationCommands.Find, Gesture = new KeyGesture(Key.F, KeyModifiers.Control) };
                    this.KeyBindings.Add(_ibFind);
                    _CmdBindingFind = new CommandBinding() { Command = ApplicationCommands.Find };
                    _CmdBindingFind.Executed += OpenFindAndReplace;
                    _CmdBindingFind.CanExecute += CanOpenFindAndReplace;
                    CommandManager.SetCommandBindings(this, new List<CommandBinding> { _CmdBindingFind });

                    if (BSOACComponent != null)
                    {
                        var initStateBinding = new Binding
                        {
                            Source = BSOACComponent,
                            Path = Const.InitState,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBTextEditor.ACCompInitStateProperty, initStateBinding);
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
            }
            if (_ibFind != null)
                this.KeyBindings.Remove(_ibFind);

            this.ClearAllBindings();
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

        protected virtual void ucAvalonTextEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
        {
        }

        protected virtual void ucAvalonTextEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
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


        public static readonly StyledProperty<string> VBTextProperty =
            AvaloniaProperty.Register<VBTextEditor, string>(nameof(VBText));

        public string VBText
        {
            get
            {
                return GetValue(VBTextProperty);
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

        private void CanOpenFindAndReplace(object sender, Avalonia.Labs.Input.CanExecuteRoutedEventArgs e)
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

        private void OpenFindAndReplace(object sender, Avalonia.Labs.Input.ExecutedRoutedEventArgs e)
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
                return IsVisible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        IsVisible = true;
                    }
                }
                else
                {
                    IsVisible = false;
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

        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBTextEditor, Global.ControlModes>(nameof(ControlMode));

        public Global.ControlModes ControlMode
        {
            get
            {
                return GetValue(ControlModeProperty);
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

        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTextEditor, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
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


        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTextEditor>();
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBTextEditor, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBTextEditor, ACInitState>(nameof(ACCompInitState));

        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            VBTextEditor thisControl = this;
            if (change.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
            else if (change.Property == VBTextProperty)
            {
                // Update Text when VBText changes
                if (!_OnTargetUpdated)
                {
                    _OnTargetUpdated = true;
                    Text = this.VBText;
                }
            }
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

        public static readonly AttachedProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBTextEditor>();
        public string VBValidation
        {
            get { return GetValue(VBValidationProperty); }
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
