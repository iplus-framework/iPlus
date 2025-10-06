using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using gip.core.layoutengine.avui.VisualControlAnalyser;
using System.Transactions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace gip.core.layoutengine.avui
{
    ///<summary>
    /// Represents a main dialog box.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Hauptdialogfeld dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBWindowDialogRoot'}de{'VBWindowDialogRoot'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBWindowDialogRoot : VBWindowDialog, IVBDialog, IACObject 
    {
        /// <summary>
        /// Creates a new instance of VBWindowDialogRoot.
        /// </summary>
        public VBWindowDialogRoot(AvaloniaObject caller) : base(caller)
        {
            this.SizeToContent = SizeToContent.Height;
            this.MaxHeight = System.Windows.SystemParameters.WorkArea.Height;
        }

        /// <summary>
        /// Creates a new instance of VBWindowDialogRoot.
        /// </summary>
        /// <param name="acObject">The acObject parameter.</param>
        /// <param name="uiElement">The uiElement parameter.</param>
        /// <param name="dockManager">The dockManager parameter.</param>
        public VBWindowDialogRoot(IACObject acObject, Control uiElement, VBDockingManager dockManager) : base(dockManager)
        {
            this.SizeToContent = SizeToContent.Height;
            this.MaxHeight = System.Windows.SystemParameters.WorkArea.Height;

            DataContext = acObject;
            BSOACComponent = acObject as IACBSO;
            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBWindowDialogRoot.ACCompInitStateProperty, binding);
            }

            if (ACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = ACComponent;
                binding.Path = Const.ACUrlCmdMessage;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBWindowDialogRoot.ACUrlCmdMessageProperty, binding);
            }

            _Control = uiElement;
            _DockManager = dockManager;
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            if (!_Loaded)
                return;
            _Loaded = false;
            if (bso == null && this.BSOACComponent != null)
                bso = BSOACComponent;

            VBLogicalTreeHelper.DeInitVBControls(bso, this.Content);
            VBVisualTreeHelper.DeInitVBControls(bso, this.Content);
            this.ClearAllBindings();

            if (IsSet(DataContextProperty))
                DataContext = null;
            if (IsSet(BSOACComponentProperty))
                BSOACComponent = null;
            _Control = null;
            _DockManager = null;

            _RootPanelDialog = null;
            
            base.DeInitVBControl(bso);
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
            {
                try
                {
                    Close();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBWindowDialogRoot", "InitStateChanged", msg);
                }
                DeInitVBControl(BSOACComponent);
            }
        }

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null && ACUrlCmdMessage.ACUrl == Const.CmdFindGUI)
            {
                try
                {
                    IACObject invoker = (IACObject)ACUrlCmdMessage.ACParameter[0];
                    string filterVBControlClassName = (string)ACUrlCmdMessage.ACParameter[1];
                    string filterFrameworkElementName = (string)ACUrlCmdMessage.ACParameter[2];
                    string filterVBContent = (string)ACUrlCmdMessage.ACParameter[3];
                    string filterACNameOfComponent = (string)ACUrlCmdMessage.ACParameter[4];
                    bool withDialogStack = (bool)ACUrlCmdMessage.ACParameter[5];

                    bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
                    bool filterFrameworkElementNameSet = !String.IsNullOrEmpty(filterFrameworkElementName);
                    bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
                    bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
                    if (!filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                        return;

                    if (ACUrlHelper.IsSearchedGUIInstance(ACIdentifier, filterVBControlClassName, filterFrameworkElementName, filterVBContent, filterACNameOfComponent))
                    {
                        invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBWindowDialogRoot", "OnACUrlMessagereceived", msg);
                }
            }
        }

        #region private members
        // Root für den Viewbereich
        Panel _RootPanelDialog = null;
        //protected string _ACContext, _LayoutXML;
        protected Control _Control;
        public Control ContentControl
        {
            get
            {
                return _Control;
            }
        }
        protected VBDockingManager _DockManager;
        #endregion

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (_DockManager != null)
                _DockManager.CloseAndRemoveVBDesign(_Control);

            //BindingOperations.ClearBinding(this, VBWindowDialogRoot.ACUrlCmdMessageProperty);
            //BindingOperations.ClearBinding(this, VBWindowDialogRoot.ACCompInitStateProperty);
            this.ClearAllBindings();
            if (_DockManager != null)
                _DockManager.OnCloseTopDialog(this);
            DeInitVBControl();
            base.OnClosing(e);
        }

        #region IBSOContext Members

        /// <summary>
        /// Gets the ACComponenet.
        /// </summary>
        public IACComponent ACComponent
        {
            get
            {
                return DataContext as IACComponent;
            }
        }

        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBWindowDialogRoot>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty = AvaloniaProperty.Register<VBWindowDialogRoot, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));
        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = AvaloniaProperty.Register<VBWindowDialogRoot, ACInitState>(nameof(ACCompInitState));
        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();
            else if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null)
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            base.OnPropertyChanged(change);

        }

        #endregion

        bool _Loaded = false;
        /// <summary>
        /// Handles the OnLoaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);
            if (_Loaded)
                return;

            _RootPanelDialog = new VBDockPanel();

            _RootPanelDialog.Children.Clear();

            if (_Control != null)
                _RootPanelDialog.Children.Add(_Control);

            _RootPanelDialog.DataContext = null;
            _RootPanelDialog.DataContext = ACComponent;

            PART_tbTitle.Text = Title;
            this.Content = _RootPanelDialog;
            _Loaded = true;

            if (VBDockingManager.GetCloseButtonVisibility(_Control) == Global.ControlModes.Enabled)
                PART_CloseButton.IsVisible = true;
            else
                PART_CloseButton.IsVisible = false;
        }


        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (Owner != null && Position.Y + this.Bounds.Height > Owner.Bounds.Height)
            {
                double calc = (Owner.Bounds.Height - Bounds.Height) / 2;
                Position = new PixelPoint(Position.X, calc > 0 ? (int)calc : 1);
            }
        }

        #region IDialog Members

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        public void CloseDialog()
        {
            this.Close();
        }

        #endregion

        #region IACObject Member
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
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return null;
        }


        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
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

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return _ACUrl; 
        }

        string _ACUrl = "";
        /// <summary>
        /// Gets or sets the ACUrl.
        /// </summary>
        public string ACUrl
        {
            get
            {
                return _ACUrl; 
            }
            protected set
            {
                _ACUrl = value;
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="rootACObject"></param>
        /// <returns></returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            throw new NotImplementedException();
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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get
            {
                return null;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return this.Name;
            }
        }
        #endregion

        #region IACObjectWithBinding Member


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
