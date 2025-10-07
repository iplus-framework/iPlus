using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Labs.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui;
using SkiaSharp;
using System.Collections;


namespace gip.core.layoutengine.avui
{

    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesignBase'}de{'VBDesignBase'}", Global.ACKinds.TACVBControl)]
    public abstract class VBDesignBase : ContentControl, IVBContent, IACObject, IACMenuBuilderWPFTree
    {
        #region c'tors
        static VBDesignBase()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBDesignBase>();
        }

        public VBDesignBase()
            : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Loaded += VBDesignBase_Loaded;
            this.Unloaded += VBDesignBase_Unloaded;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        protected bool _LoadedBase = false;
        internal virtual void InitVBControl()
        {
            if (!_LoadedBase && BSOACComponent != null)
            {
                foreach (VBInstanceInfo instanceInfo in InstanceInfoList)
                {
                    IACComponent subACComponent = null;
                    if (instanceInfo.ACIdentifier == "..")
                    {
                        subACComponent = BSOACComponent != null ? BSOACComponent.ParentACComponent : null;
                    }
                    else
                    {
                        subACComponent = BSOACComponent != null ? BSOACComponent.GetChildComponent(instanceInfo.ACIdentifier, true) : null;
                        if (subACComponent == null && instanceInfo.AutoStart)
                        {
                            subACComponent = BSOACComponent != null ? BSOACComponent.StartComponent(instanceInfo.ACIdentifier, null, instanceInfo.BuildStartParameter()) as IACComponent : null;
                        }
                    }
                    if (subACComponent != null)
                    {
                        if (instanceInfo.SetAsDataContext)
                        {
                            var binding = new Binding
                            {
                                Source = subACComponent
                            };
                            this.Bind(Control.DataContextProperty, binding);
                        }
                        if (instanceInfo.SetAsBSOACComponet)
                        {
                            var binding = new Binding
                            {
                                Source = subACComponent
                            };
                            this.Bind(VBDesignBase.BSOACComponentProperty, binding);
                        }
                    }
                    else
                    {
                        if (instanceInfo.SetAsDataContext)
                            this.DataContext = null;
                        if (instanceInfo.SetAsBSOACComponet)
                            this.BSOACComponent = null;
                    }
                }

                if (!string.IsNullOrEmpty(BSOIsActivated))
                {
                    var binding = new Binding
                    {
                        Path = BSOIsActivated,
                        Converter = new ConverterVisibilityBool()
                    };
                    this.Bind(IsVisibleProperty, binding);
                }

                if (BSOACComponent != null)
                {
                    var binding = new Binding
                    {
                        Source = BSOACComponent,
                        Path = Const.InitState,
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(VBDesignBase.ACCompInitStateProperty, binding);

                    binding = new Binding
                    {
                        Source = BSOACComponent,
                        Path = Const.ACUrlCmdMessage,
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(VBDesignBase.ACUrlCmdMessageProperty, binding);
                }
                _LoadedBase = true;
            }
        }

        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_LoadedBase)
                return;
            _LoadedBase = false;
            this.Loaded -= VBDesignBase_Loaded;
            this.Unloaded -= VBDesignBase_Unloaded;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }

            this.ClearAllBindings();

            _SelectionManager = null;
            _VBContentPropertyInfo = null;
            SelectedVBControl = null;
            adornVBControlManagerList = null;
            this.Content = null;
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }


        protected virtual void VBDesignBase_Unloaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp -= DesignPanel_KeyUp;
            this.KeyDown -= DesignPanel_KeyDown;
            _pressedKeys.Clear();

            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }

            if (!string.IsNullOrEmpty(VBContent) && ContextACObject != null && ContextACObject is IACComponent)
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.VBDesignUnloaded));
        }

        protected virtual void VBDesignBase_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyUp += DesignPanel_KeyUp;
            this.KeyDown += DesignPanel_KeyDown;

            InitVBControl(); // Aufruf leider notwendig, weil im VBDockingPanelTabbedDoc.container_VBDesignLoaded erst bei Loaded-Event der TabItem-Name refreshed wird
            //InstanceSelectionManager();

            if (_dispTimer != null)
            {
                if (!_dispTimer.IsEnabled)
                    _dispTimer.Start();
            }
            else
            {
                if (this.IsSet(CanExecuteCyclicProperty))
                {
                    _dispTimer = new DispatcherTimer();
                    _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                    _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                    _dispTimer.Start();
                }
            }

            if (!string.IsNullOrEmpty(VBContent) && ContextACObject != null && ContextACObject is IACComponent)
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.VBDesignLoaded));
        }

        #endregion

        #region InstanceInfo
        public VBInstanceInfoList _InstanceInfoList = new VBInstanceInfoList();

        public VBInstanceInfoList InstanceInfoList
        {
            get
            {
                return _InstanceInfoList;
            }
            set
            {
                _InstanceInfoList = value;
            }
        }

        public bool ContainsInstanceInfoForKey(string key)
        {
            if (this.InstanceInfoList.Count <= 0)
                return false;
            return (this.InstanceInfoList.Where(c => c.Key == key).Any());
        }

        public bool ContainsInstanceInfoForACIdentifier(string acIdentifier)
        {
            if (this.InstanceInfoList.Count <= 0)
                return false;
            return (this.InstanceInfoList.Where(c => c.ACIdentifier == acIdentifier).Any());
        }

        protected VBDesignBase FindVBDesignWithInstanceInfoByKey(IACComponent bsoACComponent, string key)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = null;
            if (this.ContainsInstanceInfoForKey(key) && this.BSOACComponent == bsoACComponent)
            {
                vbDesignBaseWithInstanceInfo = this;
                return vbDesignBaseWithInstanceInfo;
            }
            var queryParentDesigns = this.GetVisualAncestors().OfType<VBDesignBase>();
            if (queryParentDesigns.Any())
            {
                foreach (VBDesignBase parentVBDesign in queryParentDesigns)
                {
                    if (parentVBDesign.ContainsInstanceInfoForKey(key) && this.BSOACComponent == bsoACComponent)
                    {
                        vbDesignBaseWithInstanceInfo = parentVBDesign;
                        return vbDesignBaseWithInstanceInfo;
                    }
                }
            }
            if (this.BSOACComponent == bsoACComponent)
                return this;
            return null;
        }

        protected VBDesignBase FindVBDesignWithInstanceInfoByACIdentifier(IACComponent bsoACComponent, string acIdentifier)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = null;
            if (this.ContainsInstanceInfoForACIdentifier(acIdentifier) && this.BSOACComponent == bsoACComponent)
            {
                vbDesignBaseWithInstanceInfo = this;
                return vbDesignBaseWithInstanceInfo;
            }
            var queryParentDesigns = this.GetVisualAncestors().OfType<VBDesignBase>();
            if (queryParentDesigns.Any())
            {
                foreach (VBDesignBase parentVBDesign in queryParentDesigns)
                {
                    if (parentVBDesign.ContainsInstanceInfoForACIdentifier(acIdentifier) && this.BSOACComponent == bsoACComponent)
                    {
                        vbDesignBaseWithInstanceInfo = parentVBDesign;
                        return vbDesignBaseWithInstanceInfo;
                    }
                }
            }
            if (this.BSOACComponent == bsoACComponent)
                return this;
            return null;
        }

        public IACComponent GetACComponentByKey(IACComponent bsoACComponent, string key, object[] parameter = null, bool findOnly = false)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = FindVBDesignWithInstanceInfoByKey(bsoACComponent, key);
            if (vbDesignBaseWithInstanceInfo == null)
                return null;

            VBInstanceInfo instanceInfo = vbDesignBaseWithInstanceInfo.InstanceInfoList.GetInstanceInfoByKey(key);
            string instanceACName;
            if (instanceInfo == null)
                instanceACName = key;
            else
                instanceACName = instanceInfo.ACIdentifier;

            IACComponent subACComponent = bsoACComponent.GetChildComponent(instanceACName, true);
            if (subACComponent == null && !findOnly)
            {
                subACComponent = bsoACComponent.StartComponent(instanceACName, null, parameter) as IACComponent;
            }
            return subACComponent;
        }

        public IACComponent GetACComponentByACIdentifier(IACComponent bsoACComponent, string acIdentifier, object[] parameter = null, bool findOnly = false)
        {
            if (bsoACComponent == null)
                return null;
            VBDesignBase vbDesignBaseWithInstanceInfo = FindVBDesignWithInstanceInfoByACIdentifier(bsoACComponent, acIdentifier);
            if (vbDesignBaseWithInstanceInfo == null)
                return null;

            VBInstanceInfo instanceInfo = vbDesignBaseWithInstanceInfo.InstanceInfoList.GetInstanceInfoByACIdentifier(acIdentifier);
            string instanceACName;
            if (instanceInfo == null)
                instanceACName = acIdentifier;
            else
                instanceACName = instanceInfo.ACIdentifier;

            IACComponent subACComponent = bsoACComponent.GetChildComponent(instanceACName, true);
            if (subACComponent == null && !findOnly)
            {
                subACComponent = bsoACComponent.StartComponent(instanceACName, null, parameter) as IACComponent;
            }
            return subACComponent;
        }
        #endregion

        #region IVBContent Member

        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty =
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDesignBase>();
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBDesignBase, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        public static readonly StyledProperty<ACUrlCmdMessage> MsgFromSelMngrProperty =
            AvaloniaProperty.Register<VBDesignBase, ACUrlCmdMessage>(nameof(MsgFromSelMngr));

        public ACUrlCmdMessage MsgFromSelMngr
        {
            get { return GetValue(MsgFromSelMngrProperty); }
            set { SetValue(MsgFromSelMngrProperty, value); }
        }

        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBDesignBase, ACInitState>(nameof(ACCompInitState));

        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            VBDesignBase thisControl = this;
            if (change.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (change.Property == MsgFromSelMngrProperty)
                thisControl.OnMsgFromSelMngrReceived();
            else if (change.Property == ACCompInitStateProperty)
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
        }


        private IACComponent _SelectionManager;
        public IACComponent VBBSOSelectionManager
        {
            get
            {
                if (_SelectionManager == null)
                    InstanceSelectionManager();
                return _SelectionManager;
            }
        }

        private void InstanceSelectionManager()
        {
            if (_SelectionManager == null)
                _SelectionManager = GetACComponentByACIdentifier(this.BSOACComponent, Const.SelectionManagerCDesign_ClassName);

            if (_SelectionManager != null)
            {
                if (BindingOperations.GetBindingExpressionBase(this, VBDesignBase.MsgFromSelMngrProperty) == null)
                {
                    var binding = new Binding
                    {
                        Source = _SelectionManager,
                        Path = Const.ACUrlCmdMessage,
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(VBDesignBase.MsgFromSelMngrProperty, binding);
                }
            }
        }

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

        public static readonly AttachedProperty<int> CanExecuteCyclicProperty =
            ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner<VBDesignBase>();
        public int CanExecuteCyclic
        {
            get { return GetValue(CanExecuteCyclicProperty); }
            set { SetValue(CanExecuteCyclicProperty, value); }
        }

        public static readonly AttachedProperty<bool> DisableContextMenuProperty =
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBDesignBase>();
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }


        public static readonly AttachedProperty<DragMode> DragEnabledProperty =
            ContentPropertyHandler.DragEnabledProperty.AddOwner<VBDesignBase>();
        public DragMode DragEnabled
        {
            get { return GetValue(DragEnabledProperty); }
            set { SetValue(DragEnabledProperty, value); }
        }

        public static readonly AttachedProperty<string> StringFormatProperty;
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string StringFormat
        {
            get { return GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }


        public static readonly AttachedProperty<bool> AnimationOffProperty =
            ContentPropertyHandler.AnimationOffProperty.AddOwner<VBDesignBase>();
        [Category("VBControl")]
        public bool AnimationOff
        {
            get { return GetValue(AnimationOffProperty); }
            set { SetValue(AnimationOffProperty, value); }
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

        public void ControlModeChanged()
        {
        }

        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDesignBase, string>(nameof(VBContent));

        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public virtual void ACAction(ACActionArgs actionArgs)
        {
        }

        public virtual bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        public virtual string ACCaption
        {
            get;
            set;
        }

        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        public virtual IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
        }

        public virtual void OnACUrlMessageReceived()
        {
            if (!this.IsLoaded)
                return;
            var acUrlMessage = ACUrlCmdMessage;
            if (acUrlMessage == null
                || acUrlMessage.ACParameter == null
                || !acUrlMessage.ACParameter.Any()
                || !(acUrlMessage.ACParameter[0] is IACComponent)
                || acUrlMessage.TargetVBContent != this.VBContent)
                return;
            byte[] result = null;
            switch (acUrlMessage.ACUrl)
            {
                case Const.CmdPrintScreenToImage:
                    result = PrintScreenToImage(acUrlMessage.ACParameter);
                    if (result != null)
                    {
                        (acUrlMessage.ACParameter[0] as IACComponent).ACUrlCommand(Const.CmdPrintScreenToImage, result);
                    }
                    break;
                case Const.CmdPrintScreenToIcon:
                    result = PrintScreenToIcon(acUrlMessage.ACParameter);
                    if (result != null)
                    {
                        (acUrlMessage.ACParameter[0] as IACComponent).ACUrlCommand(Const.CmdPrintScreenToIcon, result);
                    }
                    break;
                case Const.CmdPrintScreenToClipboard:
                    PrintScreenToClipboard(CurrentScreenToBitmap());
                    break;
                case Const.CmdExportDesignToFile:
                    Bitmap bmp = CurrentScreenToBitmap();
                    string fileName = acUrlMessage.ACParameter[1].ToString();
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    WriteableBitmap wbmp = WriteableFromBitmap(bmp);
                    byte[] bytes = EncodeToJpeg(wbmp);
                    File.WriteAllBytes(fileName, bytes);

                    break;
                case Const.CmdCopyTextToClipboard:
                    string text = acUrlMessage.ACParameter[1].ToString();
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    clipboard?.SetTextAsync(text);
                    break;
                case Const.CmdInitSelectionManager:
                    {
                        _ = VBBSOSelectionManager;
                    }
                    break;
                case Const.CmdDesignModeOff:
                    if (this is VBDesign)
                    {
                        VBDesign vbDesign = this as VBDesign;
                        vbDesign.DesignModeOff();
                    }
                    break;
                default:
                    break;
            }
        }

        public static WriteableBitmap WriteableFromBitmap(Bitmap bitmap)
        {
            var writeableBitmap = new WriteableBitmap(
                bitmap.PixelSize,
                bitmap.Dpi,
                bitmap.Format
            );
            using (var fb = writeableBitmap.Lock())
            {
                bitmap.CopyPixels(fb, AlphaFormat.Opaque);
            }
            return writeableBitmap;
        }

        public static byte[] EncodeToJpeg(Bitmap bitmap, int quality = 80)
        {
            WriteableBitmap wb = bitmap as WriteableBitmap;
            WriteableBitmap tempWb = (wb == null) ? WriteableFromBitmap(bitmap) : null;
            wb ??= tempWb;
            try
            {
                using var pi = wb!.Lock();
                SKColorType colorType = SKColorType.RgbaF32;
                var skImageInfo = new SKImageInfo(pi.Size.Width, pi.Size.Height, colorType);
                using var skBitmap = new SKBitmap(skImageInfo);
                skBitmap.InstallPixels(skImageInfo, pi.Address, pi.RowBytes);
                using var skImage = SKImage.FromBitmap(skBitmap);
                return skImage.Encode(SKEncodedImageFormat.Jpeg, quality).ToArray();
            }
            finally
            {
                tempWb.Dispose();
            }
        }

        public void OnMsgFromSelMngrReceived()
        {
            if (!this.IsLoaded)
                return;
            var acUrlMessage = MsgFromSelMngr;
            if (acUrlMessage == null
                || acUrlMessage.ACParameter == null
                || !acUrlMessage.ACParameter.Any()
                || acUrlMessage.ACParameter.Count() < 2
                || acUrlMessage.ACParameter[0] != _SelectionManager)
                return;
            switch (acUrlMessage.ACUrl)
            {
                case Const.CmdHighlightVBControl:
                    HighlightVBControl(acUrlMessage.ACParameter[1] as IVBContent, (bool)(acUrlMessage.ACParameter[2]));
                    break;
                case Const.CmdHighlightContentACObject:
                    HighlightContentACObject(acUrlMessage.ACParameter[1] as IACObject, (bool)(acUrlMessage.ACParameter[2]));
                    break;
                default:
                    break;
            }
        }


        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        #endregion

        #region IACObject Member

        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion

        #region Selection

        #region Dependency-Properties

        public static readonly StyledProperty<bool> ShowAdornerLayerProperty =
            AvaloniaProperty.Register<VBDesignBase, bool>(nameof(ShowAdornerLayer), true);

        [Category("VBControl")]
        public bool ShowAdornerLayer
        {
            get { return GetValue(ShowAdornerLayerProperty); }
            set { SetValue(ShowAdornerLayerProperty, value); }
        }

        public static readonly AttachedProperty<bool> IsDesignerActiveProperty =
            AvaloniaProperty.RegisterAttached<VBDesignBase, bool>("IsDesignerActive", typeof(VBDesignBase), false, true);

        public bool IsDesignerActive
        {
            get { return GetValue(IsDesignerActiveProperty); }
            set { SetValue(IsDesignerActiveProperty, value); }
        }

        #endregion

        #region Mouse-Event
        IVBContent _LastClickedControl;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!IsDesignerActive && e.Source is Visual)
            {
                Visual clickedElement = e.Source as Visual;
                if (e.Source is Adorner)
                {
                    if (AdornVBControlManagerList != null)
                    {
                        foreach (AdornerVBControlManager adManager in AdornVBControlManagerList)
                        {
                            AdornerHitTestResult adornerHitResult = adManager.AdornerLayerOfDesign.AdornerHitTest(e.GetPosition(this));
                            if (adornerHitResult != null)
                                clickedElement = adornerHitResult.VisualHit;
                        }
                    }
                    clickedElement = (e.Source as Adorner).AdornedElement;
                    var hitResult = this.InputHitTest(e.GetPosition(clickedElement));
                    if (hitResult != null && hitResult is Visual)
                        clickedElement = hitResult as Visual;
                }
                var ancestors = clickedElement.GetVisualAncestors();
                VBDesignBase vbDesign = ancestors.OfType<VBDesignBase>().FirstOrDefault();

                // Das OnPreviewMouseDown wird getunnelt (oben -> unten am Element-Tree)
                // Falls das durch zuerst gefundene FirstOrDefault() vbDesign dieses Design ist, dann ist er selbst das letzte vbDesign, 
                // das eine Instanz-Definition für den Selection-Manager besitzen kann
                if (vbDesign == this)
                {
                    IVBContent selectableElement = null;
                    var vbContentAncestors = ancestors.OfType<IVBContent>();
                    foreach (IVBContent elementInVisualTree in vbContentAncestors)
                    {
                        if (elementInVisualTree == this)
                            break;
                        if (elementInVisualTree is Visual)
                        {
                            if (GetIsSelectable(elementInVisualTree as Visual) == IsSelectableEnum.True)
                            {
                                selectableElement = elementInVisualTree;
                                break;
                            }

                            if (elementInVisualTree is VBCheckBox)
                            {
                                VBCheckBox checkBox = elementInVisualTree as VBCheckBox;
                                if (checkBox != null && checkBox.PushButtonStyle)
                                {
                                    selectableElement = null;
                                    break;
                                }
                            }
                        }
                    }

                    if (selectableElement != null)
                        SetSelectionAtManager(selectableElement);
                }
                // Sonst sammle weiter Instanz-Informationen
                else
                {
                    //if (this.ContainsSelectionManagerInstanceInfo)
                    //_LastVBDesignBaseWithInfo = this;
                }
            }
            base.OnPointerPressed(e);
        }

        private void SetSelectionAtManager(IVBContent controlToSelect)
        {
            _LastClickedControl = controlToSelect;
            if ((controlToSelect is VBVisual || controlToSelect is VBVisualGroup || controlToSelect is VBGraphItem) && _SelectionManager == null)
                InstanceSelectionManager();
            if (_SelectionManager != null)
                _SelectionManager.ACUrlCommand("!OnVBControlClicked", _LastClickedControl, IsKeyDown(Key.LeftCtrl));
        }

        private readonly Dictionary<Key, bool> _pressedKeys = new Dictionary<Key, bool>();

        public bool IsKeyDown(Key key)
        {
            return _pressedKeys.TryGetValue(key, out bool isPressed) && isPressed;
        }

        private void DesignPanel_KeyUp(object sender, KeyEventArgs e)
        {
            _pressedKeys[e.Key] = false;
        }

        void DesignPanel_KeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys[e.Key] = true;
        }


        #endregion

        #region Attached-Property IsSelectableProperty

        public enum IsSelectableEnum : short
        {
            Unset = 0,
            True = 1,
            False = 2,
        }

        public static readonly AttachedProperty<IsSelectableEnum> IsSelectableProperty =
            AvaloniaProperty.RegisterAttached<VBDesignBase, IsSelectableEnum>("IsSelectable", typeof(VBDesignBase), IsSelectableEnum.Unset);

        public static void SetIsSelectable(Visual element, IsSelectableEnum value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDesignBase.IsSelectableProperty, value);
        }

        public static IsSelectableEnum GetIsSelectable(Visual element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return element.GetValue(VBDesignBase.IsSelectableProperty);
        }
        #endregion

        #region Dependency-Property SelectedVBControl

        public IVBContent SelectedVBControl
        {
            get { return GetValue(SelectedVBControlProperty); }
            set { SetValue(SelectedVBControlProperty, value); }
        }

        public static readonly StyledProperty<IVBContent> SelectedVBControlProperty =
            AvaloniaProperty.Register<VBDesignBase, IVBContent>(nameof(SelectedVBControl));

        #endregion

        #region Selection-Methods

        [ACMethodInfo("", "en{'HighlightVBControl'}de{'HighlightVBControl'}", 9999)]
        public void HighlightVBControl(IVBContent vbControlToSelect, bool isMultiSelect)
        {
            if (vbControlToSelect == null)
            {
                AdornVBControl(null, isMultiSelect);
                return;
            }

            // Falls geklicktes Control, das hervorgehoben werden soll
            if (_LastClickedControl == vbControlToSelect)
            {
                AdornVBControl(_LastClickedControl, isMultiSelect);
            }
            // Sonst suche im Tree nach Control
            else
            {
                if (vbControlToSelect is Visual)
                {
                    if (VBLogicalTreeHelper.IsChildObjectInLogicalTree(this, vbControlToSelect as Visual, typeof(VBDesignBase)))
                    {
                        AdornVBControl(vbControlToSelect, isMultiSelect);
                        return;
                    }
                }
                // Nicht gefunden, dann wird dieses Control nicht in diesem VBDesign verwaltet sondern in einem anderen
                AdornVBControl(null, isMultiSelect);
            }
        }

        [ACMethodInfo("", "en{'HighlightContentACObject'}de{'HighlightContentACObject'}", 9999)]
        public void HighlightContentACObject(IACObject acObjectToSelect, bool highlightParentIfNotFound)
        {
            if (acObjectToSelect == null)
            {
                SetSelectionAtManager(null);
                AdornVBControl(null, false);
                return;
            }

            if (_LastClickedControl != null)
            {
                if (HasVBControlContent(_LastClickedControl, acObjectToSelect))
                {
                    HighlightVBControl(_LastClickedControl, false);
                    return;
                }
            }
            IVBContent foundContent = FindContentACObjectInLogicalTree(this, acObjectToSelect);
            if (foundContent == null && highlightParentIfNotFound)
            {
                IACObject parentACObject = acObjectToSelect.ParentACObject;
                while (parentACObject != null)
                {
                    foundContent = FindContentACObjectInLogicalTree(this, parentACObject);
                    if (foundContent != null)
                        break;
                    parentACObject = parentACObject.ParentACObject;
                }
            }
            // Falls dieses VBDesign, das selecktierte Element beeinhaltet, dann setze selektiertes Element auf dem Selection-Manager
            // dieser wiederum ruft die Methode HighlightVBControl() auf
            if (foundContent != null)
                SetSelectionAtManager(foundContent);
            // Nicht gefunden, dann wird dieses Control nicht in diesem VBDesign verwaltet sondern in einem anderen
            else
                AdornVBControl(null, false);
        }


        private static IVBContent FindContentACObjectInLogicalTree(Visual depObjStart, IACObject acObjectContent)
        {
            if ((depObjStart == null) || (acObjectContent == null))
                return null;
            IVBContent vbControlStart = depObjStart as IVBContent;
            if (vbControlStart != null)
            {
                if (HasVBControlContent(vbControlStart, acObjectContent))
                    return vbControlStart;
            }
            if (!(depObjStart is Visual))
                return null;
            foreach (Visual childObj in depObjStart.GetVisualChildren())
            {
                if (childObj != null)
                {
                    IVBContent found = FindContentACObjectInLogicalTree(childObj, acObjectContent);
                    if (found != null)
                        return found;
                }
                else
                    continue;
            }
            return null;
        }

        private static bool HasVBControlContent(IVBContent vbControl, IACObject acObjectContent)
        {
            if ((vbControl == null) || (acObjectContent == null))
                return false;
            if (vbControl.ACContentList == null)
                return false;
            if (vbControl.ACContentList.Where(c => c == acObjectContent).Any())
                return true;
            return false;
        }

        public byte[] PrintScreenToImage(object[] parameters)
        {
            return PrintScreenAndRezize(370, 250, parameters);
        }


        public byte[] PrintScreenToIcon(object[] parameters)
        {
            return PrintScreenAndRezize(32, 32, parameters);
        }

        private byte[] PrintScreenAndRezize(double width, double height, object[] parameters)
        {
            string vbContentChild = null;
            if (parameters != null && parameters.Count() > 1)
                vbContentChild = parameters[1] as string;
            Control uiElement = this.Content as Control;

            Canvas canvas = null;
            byte[] arr = new byte[] { 0 };
            if (!String.IsNullOrEmpty(vbContentChild))
            {
                Control childFound = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(uiElement, vbContentChild) as Control;
                if (childFound == null)
                    return arr;
                uiElement = childFound;
            }

            if (uiElement is Canvas)
                canvas = uiElement as Canvas;
            else if (uiElement is ScrollViewer)
            {
                ScrollViewer scrollViewer = uiElement as ScrollViewer;
                if (scrollViewer.Content is Canvas)
                    canvas = scrollViewer.Content as Canvas;
            }
            else if (uiElement is VBVisual)
            {
                VBVisual vbVisual = uiElement as VBVisual;
                canvas = VBVisualTreeHelper.FindChildObjectInVisualTree(vbVisual.Content as Control, typeof(Canvas)) as Canvas;
            }

            // Bitmap handling - keeping original code as requested since it needs special handling in Avalonia
            // TODO: This section needs platform-specific bitmap handling - commenting out problematic parts
            if (canvas != null && !Double.IsNaN(canvas.Width) && !Double.IsNaN(canvas.Height))
            {
                canvas.Arrange(new Rect(canvas.Bounds.Size));
                RenderTargetBitmap rtb = new RenderTargetBitmap(PixelSize.FromSize(new Size((int)canvas.Width, (int)canvas.Height), new Vector(96, 96)));
                rtb.Render(canvas);
                WriteableBitmap wbmp = WriteableFromBitmap(rtb);
                arr = EncodeToJpeg(wbmp);

                double widthPoint = (((Control)canvas.Parent).Bounds.Width - canvas.Width) / 2;
                double heighPoint = (((Control)canvas.Parent).Bounds.Height - canvas.Height) / 2;
                if (widthPoint > 0 && heighPoint > 0)
                    canvas.Arrange(new Rect(new Point(widthPoint, heighPoint), canvas.Bounds.Size));
            }

            return arr;
        }

        private Bitmap CurrentScreenToBitmap()
        {
            Bitmap bitmap = null;

            Control uiElement = this.Content as Control;
            ScrollViewer scrollViewer = null;
            VBCanvas vbCanvas = null;

            if (uiElement is VBCanvas)
                vbCanvas = uiElement as VBCanvas;

            else if (uiElement is ScrollViewer)
                scrollViewer = uiElement as ScrollViewer;

            if (scrollViewer != null && scrollViewer.Content is VBCanvas)
                vbCanvas = scrollViewer.Content as VBCanvas;

            if (vbCanvas != null && !Double.IsNaN(vbCanvas.Width) && !Double.IsNaN(vbCanvas.Height))
            {

                vbCanvas.Arrange(new Rect(vbCanvas.Bounds.Size));
                RenderTargetBitmap rtb = new RenderTargetBitmap(PixelSize.FromSize(new Size((int)vbCanvas.Width, (int)vbCanvas.Height), new Vector(96, 96)));
                rtb.Render(vbCanvas);
                return rtb;
                //using (MemoryStream stream = new MemoryStream())
                //{
                //    BitmapEncoder encoder = new BmpBitmapEncoder();
                //    encoder.Frames.Add(BitmapFrame.Create(rtb));
                //    encoder.Save(stream);
                //    bitmap = new Bitmap(stream);
                //}
            }

            return bitmap;
        }

        public void PrintScreenToClipboard(Bitmap bitmap)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms);
                    var dataObject = new DataObject();
                    dataObject.Set("image/png", ms.ToArray());
                    topLevel.Clipboard.SetDataObjectAsync(dataObject);
                }
            }
            //BitmapSource bitmapSource = Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //Clipboard.SetImage(bitmapSource);
        }

        #endregion

        #region Adorner-Layer
        protected List<AdornerVBControlManager> adornVBControlManagerList;
        public List<AdornerVBControlManager> AdornVBControlManagerList
        {
            get
            {
                if (adornVBControlManagerList == null)
                    adornVBControlManagerList = new List<AdornerVBControlManager>();
                return adornVBControlManagerList;
            }
        }

        public Color Invert(Color color)
        {
            return Color.FromRgb((byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
        }

        protected void AdornVBControl(IVBContent vbControlToAdorn, bool isMultiSelect)
        {
            //this.Background
            if (ShowAdornerLayer && !IsDesignerActive)
            {
                Color selectionColor = Colors.Red;
                Color selectionColor2 = Colors.White;
                if (!isMultiSelect)
                {
                    foreach (var item in AdornVBControlManagerList)
                        item.RemoveAdornerFromElement();
                    AdornVBControlManagerList.Clear();
                }

                AdornerVBControlManager adornManagerForControl = AdornVBControlManagerList.FirstOrDefault(x => x.ControlToAdorn == vbControlToAdorn);
                if (adornManagerForControl != null)
                {
                    adornManagerForControl.RemoveAdornerFromElement();
                    adornManagerForControl.AddAdornerToElement(selectionColor);
                }
                else
                {
                    adornManagerForControl = new AdornerVBControlManager(vbControlToAdorn, selectionColor);
                    AdornVBControlManagerList.Add(adornManagerForControl);
                }

                foreach (var item in AdornVBControlManagerList)
                {
                    if (item.ControlToAdorn != vbControlToAdorn && item.LastUsedColor == selectionColor)
                    {
                        item.RemoveAdornerFromElement();
                        item.AddAdornerToElement(selectionColor2);
                    }
                }
            }
            SelectedVBControl = vbControlToAdorn;
        }

        protected void OnSelectedVBControlChanged()
        {
        }
        #endregion

        #endregion

        #region IACMenuBuilder Member
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            return this.ReflectGetMenu(this);
        }

        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            // Ende der WPF-GetMenu-Rekursion, wenn hier angelagt
            ACMenuItemList acMenuList = this.ReflectGetMenu(this);
            if ((acMenuList != null) && (acMenuList.Count > 0))
            {
                foreach (var acMenu in acMenuList)
                    acMenuItemList.Add(acMenu);
                //acMenuItemList.Add(new ACMenuItem(null, "-", "", null));
            }
        }
        #endregion

        #region CanExecute Dispatcher
        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Additional Dependency Prop

        public static AttachedProperty<string> RegisterVBDecoratorProperty =
            ContentPropertyHandler.RegisterVBDecoratorProperty.AddOwner<VBDesignBase>();

        [Category("VBControl")]
        public string RegisterVBDecorator
        {
            get { return GetValue(RegisterVBDecoratorProperty); }
            set { SetValue(RegisterVBDecoratorProperty, value); }
        }

        public static readonly StyledProperty<string> BSOIsActivatedProperty =
            AvaloniaProperty.Register<VBDesignBase, string>(nameof(BSOIsActivated));


        [Category("VBControl")]
        public string BSOIsActivated
        {
            get { return GetValue(BSOIsActivatedProperty); }
            set { SetValue(BSOIsActivatedProperty, value); }
        }

        #endregion

        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }
    }

}
