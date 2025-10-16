using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFlowDocumentScrollViewer'}de{'VBFlowDocumentScrollViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFlowDocumentScrollViewer : ScrollViewer, IVBContent
    //Avalonia TODO: Old WPF Declaraion: public class VBFlowDocumentScrollViewer : FlowDocumentScrollViewer, IVBContent
    {
        public VBFlowDocumentScrollViewer()
        {
        }

        protected override void OnInitialized()
        {
            Loaded += VBFlowDocumentScrollViewer_Loaded;
            base.OnInitialized();
        }

        private void VBFlowDocumentScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
        }

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;

        IACType _ACTypeInfo = null;

        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", VBContent);
                return;
            }
            _ACTypeInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            if (String.IsNullOrEmpty(VBContent))
                return;

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = dcPath;
            binding.Mode = BindingMode.OneWay;
            this.Bind(DocumentContentProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBFlowDocumentScrollViewer.ACCompInitStateProperty, binding);
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBFlowDocumentScrollViewer>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBFlowDocumentScrollViewer, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBFlowDocumentScrollViewer, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        /// <summary>
        /// Invoked on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        public string DocumentContent
        {
            get { return GetValue(DocumentContentProperty); }
            set { SetValue(DocumentContentProperty, value); }
        }

        // Using a StyledProperty as the backing store for DocumentContent. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> DocumentContentProperty =
            AvaloniaProperty.Register<VBFlowDocumentScrollViewer, string>(nameof(DocumentContent));

        /// <summary>
        /// The actual flow document being displayed
        /// </summary>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == DocumentContentProperty)
            {
                //if (string.IsNullOrEmpty(DocumentContent))
                //{
                //    Document = null;
                //    return;
                //}

                //try
                //{
                //    // Try to load the XAML content as an AvaloniaUI control using AvaloniaRuntimeXamlLoader
                //    var control = AvaloniaRuntimeXamlLoader.Load(DocumentContent) as FlowDocument;
                //    Document = control;
                //}
                //catch (Exception ex)
                //{
                //    // Log the error and clear the content
                //    this.Root()?.Messages?.LogError("VBFlowDocumentScrollViewer", "OnPropertyChanged", 
                //        $"Failed to load DocumentContent: {ex.Message}");
                //    Document = null;
                //}
            }
            else if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }

        }



        #region IVBContent members

        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
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

        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a StyledProperty as the backing store for VBContent. This enables animation, styling, binding, etc...
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBFlowDocumentScrollViewer, string>(nameof(VBContent));


        public IACObject ContextACObject => DataContext as IACObject;

        public IACObject ParentACObject => this.Parent as IACObject;

        public IACType ACType => this.ReflectACType();

        public IEnumerable<IACObject> ACContentList => this.ReflectGetACContentList();

        public string ACIdentifier => this.ReflectGetACIdentifier();

        public string ACCaption => ACIdentifier;

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

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public void DeInitVBControl(IACComponent bso)
        {
            Content = null;
            _documentControl = null;
            Loaded -= VBFlowDocumentScrollViewer_Loaded;
            this.ClearAllBindings();
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

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

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        #endregion
    }
}
