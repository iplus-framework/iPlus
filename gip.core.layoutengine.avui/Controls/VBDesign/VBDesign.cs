using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui.UIExtensions;
using gip.ext.designer.avui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace gip.core.layoutengine.avui
{
    // Die in VBContent angegebene ACUrl muß immer auf ein IACObjekt verweisen, das IACObjectDesign implementiert
    // Anhand des VBContent wird die Eigenschaft "ContentACObject" bestückt. Das dargestellte Layout ist immer ContentACObject.XMLDesign.
    // -Es gibt drei Varianten:
    //  1. Nicht gesetzt
    //     Als ContentACObject wird das ContextACObject, also der DataContext des übergeordneten WPF-Objekts verwendet.
    //     Die Auswertung erfolgt einmalig bei der Initialisierung
    //  2. *<ACIdentifier für ACClassDesign>
    //     Ausgehend vom ContextACObject wird eine IACObjectDesign gesucht. 
    //     Die Auswertung erfolgt einmalig bei der Initialisierung
    //  3. <ACIdentifier für ACClassDesign>
    //     Ausgehend vom ContextACObject wird eine Eigenschaften (meist an einer ACComponent) gebunden, welche vom IACObjectDesign ist. 
    //     Ändert sich die Eigenschaft, wird auch die Darstellung aktualisiert.


    /// <summary>
    /// The VBDesign is used to display any class that <see cref="IACObjectDesign"/> implements (e.g. Partslist, ACClassDesign)
    /// </summary>
    /// <summary xml:lang="de">
    /// Das VBDesign dient zur Darstellung von beliebigen Klasse, welche <see cref="IACObjectDesign"/> implementiert (z.B. Partslist, ACClassDesign)
    /// </summary>
    [TemplatePart(Name = "PART_BorderFreeze", Type = typeof(Control))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDesign'}de{'VBDesign'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDesign : VBDesignBase, IVBContent, IVBSerialize, IACMenuBuilderWPFTree
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBDesign.
        /// </summary>
        public VBDesign()
            : base()
        {
            this.Focusable = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            //// Businessobjects-Root must me bound later, because at this time this first VBDesign is not loaded in the logical tree.
            //// Therefore the Datacontext could not be determinde from child VBDesigns, when the application starts
            //if (this.ContextACObject == null 
            //    || this.ContextACObject.ACType == null 
            //    || this.ContextACObject.ACType.ACKind != Global.ACKinds.TACBusinessobjects)
                InitBinding();
            //this.MouseDown += new MouseButtonEventHandler(VBDesign_MouseDown);
            //this.AddHandler(VBDesign.MouseDownEvent, new MouseButtonEventHandler(VBDesign_MouseDown), true);
        }


        #endregion

        public event EventHandler OnContextACObjectChanged;

        #region Control Loaded-Event
        protected bool _Loaded = false;
        /// <summary>
        /// Handles the Loaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arguments parameter.</param>
        protected override void VBDesignBase_Loaded(object sender, RoutedEventArgs e)
        {
            base.VBDesignBase_Loaded(sender, e);
            if (this.Content is Visual)
            {
                var control = VBVisualTreeHelper.FindChildObjectInVisualTree<Boolean>(this.Content as Visual, "AutoFocus", true);
                if (control != null && control is InputElement)
                {
                    MethodInfo pi = control.GetType().GetMethod(nameof(InputElement.Focus));
                    if (pi != null)
                    {
                        pi.Invoke(control, new object[] { NavigationMethod.Unspecified, NavigationMethod.Unspecified });
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a VBControl.
        /// </summary>
        internal override void InitVBControl()
        {
            base.InitVBControl();
            if (DisableContextMenu)
                ContextFlyout = null;
            InitBinding();
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind themselves to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            if (_Loaded)
            {
                _LoadDesignLocked = true;
                this.ClearValue(DataContextProperty);
                this.ClearValue(VBDesign.ContentACObjectProperty);
                this.ClearValue(VBDesign.MasterPageSelProperty);

                _DesignManager = null;
                CurrentTargetVBDataObject = null;
            }
            base.DeInitVBControl(bso);
            _LoadDesignLocked = false;
            //_Loaded = false;
        }

        private void InitBinding()
        {
            if (_Loaded || DataContext == null)
                return;

            if (!string.IsNullOrEmpty(this.AutoStartACComponent))
            {
                if (AutoStartParameter == null || !AutoStartParameter.Any())
                {
                    var binding = new Binding
                    {
                        Source = BSOACComponent.ACUrlCommand(AutoStartACComponent, null)
                    };
                    this.Bind(DataContextProperty, binding);
                }
                else
                {
                    var binding = new Binding
                    {
                        Source = BSOACComponent.ACUrlCommand(AutoStartACComponent, this.AutoStartParameter)
                    };
                    this.Bind(DataContextProperty, binding);
                }
                if (BSOACComponent.ACType.ACKind == Global.ACKinds.TACBusinessobjects)
                {
                    var binding = new Binding
                    {
                        Source = DataContext
                    };
                    this.Bind(VBDesign.BSOACComponentProperty, binding);
                }
            }

            if (ContextACObject == null)
                return;
            if (!(ContextACObject is IACComponent))
                return;
            if (OnContextACObjectChanged != null)
                OnContextACObjectChanged(this, new EventArgs());

            var bindingVarioWPF = new Binding
            {
                Source = this.Root().RootPageWPF,
                Path = "VBDesignEditing",
                Mode = BindingMode.OneWay
            };
            this.Bind(VBDesign.MasterPageSelProperty, bindingVarioWPF);

            if (string.IsNullOrEmpty(VBContent))
            {
                _LoadDesignLocked = true;
                if (Parent is Grid ParentGrid && ParentGrid.Name == "MainGridMobile")
                    ContentACObject = (ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMainMobile);
                else
                    ContentACObject = (ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                _LoadDesignLocked = false;
                LoadDesign();
            }
            else if (VBContent[0] == '*')
            {

                _LoadDesignLocked = true;
                ContentACObject = (ContextACObject as IACComponent).GetDesign(VBContent.Substring(1));
                if (ContentACObject == null)
                {
                    Database.Root.Messages.LogError("VBDesign", "InitBinding(1)", String.Format("Design {0} not found for Context {1}", VBContent, ContextACObject.GetACUrl()));
                    _Loaded = true;
                    return;
                    //throw new Exception("Kein Design vorhanden");
                    //ContentACObject = (ContextACObject as IACComponent).GetACClassDefaultDesign();
                }

                _LoadDesignLocked = false;
                LoadDesign();
            }
            else
            {
                var binding = new Binding
                {
                    Source = DataContext,
                    Path = VBContent,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDesign.ContentACObjectProperty, binding);
                LoadDesign();
            }

            _Loaded = true;
        }

        #endregion

        #region ACComponent Members, Start/Stop, Autoload
        /// <summary>
        /// Gets or sets the name of ACComponent which should be started automatically.
        /// </summary>
        public string AutoStartACComponent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parameters for AutoStart as ACValueList.
        /// </summary>
        public ACValueList AutoStartParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Stops the ACComponent which is started automatically.
        /// </summary>
        public void StopAutoStartComponent()
        {
            if (!(ContextACObject is IACComponent))
                return;
            string objectUrl = ContextACObject.GetACUrl();
            string keyACIdentifier = ContextACObject.ACIdentifier;

            // Alle geöffneten Dialoge schließen
            VBLogicalTreeHelper.RemoveAllDialogs(ContextACObject as IACComponent);

            if (!(ContextACObject as IACComponent).Stop())
                return;
            // Stop ACObject
            //this.Root().ACUrlCommand(objectUrl.Replace(keyACIdentifier, "~" + keyACIdentifier), null);
        }

        /// <summary>
        /// Gets the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        /// <returns></returns>
        public override ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = base.GetMenu(vbContent, vbControl);

            if (ContextACObject.ACType.ACKind == Global.ACKinds.TACBSO)
            {

                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    if (Database.Root.Environment.License.MayUserDevelop)
                    {
                        if (ContextACObject.ACType.ACIdentifier != "BSOiPlusStudio")
                        {
                            ACClass acClass = Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == "BSOiPlusStudio").First();
                            if (acClass.GetRight(acClass) == Global.ControlModes.Enabled)
                            {
                                ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                                acMethod.ParameterValueList["AutoLoad"] = ContextACObject.ACType.GetACUrl();

                                ACValueItem category = Global.ContextMenuCategoryList.FirstOrDefault(c => (short)c.Value == (short)Global.ContextMenuCategory.Utilities);
                                ACMenuItem parent = new ACMenuItem(null, category.ACCaption, category.Value.ToString(), category.SortIndex, null, true);
                                acMenuItemList.Insert(0, new ACMenuItem("Show " + ContextACObject.ACType.ACIdentifier + " in iPlus Delevopment Environment", Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, (short)MISort.IPlusStudio, acMethod.ParameterValueList, parent.ACUrl));
                            }
                        }
                    }
                }
            }
            return acMenuItemList;
        }

        #endregion

        #region Static Layout from ACClassDesign
        private bool _LoadDesignLocked = false;
        /// <summary>
        /// Gets or sets the ContentACObject.
        /// </summary>
        public IACObjectDesign ContentACObject
        {
            get
            {
                return (IACObjectDesign)GetValue(ContentACObjectProperty);
            }
            set
            {
                SetValue(ContentACObjectProperty, value);
            }
        }
        /// <summary>
        /// Represents the dependency property for ContentACObject.
        /// </summary>
        public static readonly StyledProperty<IACObjectDesign> ContentACObjectProperty =
            AvaloniaProperty.Register<VBDesign, IACObjectDesign>(nameof(ContentACObject));

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == ContentACObjectProperty)
            {
                if (!_LoadDesignLocked)
                    LoadDesign();
            }
            else if (change.Property == MasterPageSelProperty)
            {
                RootPageWPF_VBDesignEditingEvent();
            }
        }

        public bool CoerceRefreshDesign
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a design info.
        /// </summary>
        public void InitDesignInfo()
        {
            if (ContextACObject == null)
                return;
            if (!String.IsNullOrEmpty(VBContent))
                return;
            if (ContentACObject == null)
                ContentACObject = (ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
        }
        #endregion

        #region Overidden Member
        /// <summary>
        /// Loads a design.
        /// </summary>
        [ACMethodCommand("", "", 9999)]
        public void LoadDesign()
        {
            string xaml = "";

            Visual parentObj = this.GetVisualParent();
            if (parentObj != null)
            {
                VBDesign parentDesign = parentObj.GetVBDesign();
                if (parentDesign != null)
                {
                    if (parentDesign.IsDesignerActive)
                    {
                        try
                        {
                            ContentControl contentControl = new ContentControl();
                            ResourceDictionary dict = new ResourceDictionary();
                            dict.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://gip.core.layoutengine.avui/Controls/VBRibbon/Icons/Design.xaml", UriKind.Absolute)));
                            contentControl.Theme = (ControlTheme)dict["IconDesignStyleGip"];
                            Content = contentControl;
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("VBDesign", "LoadDesign", msg);
                        }

                        return;
                    }
                }
            }

            if (ContentACObject != null)
            {
                xaml = ContentACObject.XAMLDesign;
                if (!String.IsNullOrEmpty(xaml))
                {
                    if (ContextACObject != null && ContentACObject is ACClassDesign)
                    {
                        ACClassDesign = ContentACObject as ACClassDesign;
                        var query = ACClassDesign.VBUserACClassDesign_ACClassDesign.Where(c => c.VBUserID == this.Root().Environment.User.VBUserID && c.ACClassDesign != null);
                        if (query.Any())
                        {
                            VBUserACClassDesign userDesign = query.First();
                            if (!String.IsNullOrEmpty(userDesign.XAMLDesign))
                                xaml = userDesign.XAMLDesign;
                        }
                    }
                }
            }

            Content = null;
            Visual uiElement = null;
            if (string.IsNullOrEmpty(xaml))
            {
                ContentControl contentControl = new ContentControl();
                ResourceDictionary dict = new ResourceDictionary();
                dict.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://gip.core.layoutengine.avui/Controls/VBRibbon/Icons/Design.xaml", UriKind.Absolute)));
                contentControl.Theme = (ControlTheme)dict["IconDesignStyleGip"];
                uiElement = contentControl;
            }
            else if (ACClassDesign != null && ACClassDesign.BAMLDesign != null && ACClassDesign.IsDesignCompiled)
                uiElement = Layoutgenerator.LoadLayout(ACClassDesign, ContextACObject, BSOACComponent, ContentACObject.ACIdentifier);
            else
                uiElement = Layoutgenerator.LoadLayout(xaml, ContextACObject, BSOACComponent, ContentACObject.ACIdentifier);

            Content = uiElement;
            if (DesignModeAllways)
            {
                DesignModeOn();
            }
        }

        public override void OnACUrlMessageReceived()
        {
            base.OnACUrlMessageReceived();

            if (!this.IsLoaded)
                return;
            var acUrlMessage = ACUrlCmdMessage;
            if (acUrlMessage == null
                || acUrlMessage.ACParameter == null
                || !acUrlMessage.ACParameter.Any()
                || !(acUrlMessage.ACParameter[0] is IACComponent)
                || acUrlMessage.TargetVBContent != this.VBContent)
                return;
            switch (acUrlMessage.ACUrl)
            {
                case Const.CmdShowHideVBContentInfo:
                    ShowHideVBContentInfo();
                    break;
            }
        }

        #endregion

        #region Freeze Screen and User-Dependent Serialization

        /// <summary>
        /// Updates a desing of current user.
        /// </summary>
        /// <param name="xUserContent">The xUserContent parameter.</param>
        /// <param name="persistUserContent">The persistUserContent parameter. Determines is user custom content saved or not.</param>
        public void UpdateDesignOfCurrentUser(XElement xUserContent, bool persistUserContent)
        {
            if (ContentACObject == null)
                return;
            if (!(ContentACObject is ACClassDesign))
                return;
            string designXAML = ContentACObject.XAMLDesign;
            if (String.IsNullOrEmpty(designXAML))
                return;
            XElement xDefaultContent = Layoutgenerator.LoadLayoutAsXElement(designXAML);
            if (xDefaultContent == null)
                return;
            CompareDesignsAndUpdate(xUserContent, xDefaultContent);
            if (ContextACObject is IACComponent)
            {
                ACClassDesign acClassDesign = ContentACObject as ACClassDesign;
                if (acClassDesign != null && acClassDesign.Database != null)
                {
                    VBUserACClassDesign userDesign = acClassDesign.VBUserACClassDesign_ACClassDesign.FirstOrDefault(c => c.VBUserID == this.Root().Environment.User.VBUserID);
                    if (userDesign == null)
                        userDesign = VBUserACClassDesign.NewVBUserACClassDesign(acClassDesign.Database.ContextIPlus, this.Root().Environment.User, acClassDesign);
                    if (persistUserContent)
                        userDesign.XAMLDesign = xUserContent.ToString();
                    else
                        userDesign.XAMLDesign = xDefaultContent.ToString();
                    var msg = acClassDesign.Database.ACSaveChanges();
                    if (msg != null)
                        acClassDesign.Database.ACUndoChanges();
                }
            }
        }

        private bool CompareDesignsAndUpdate(XElement xUserElement, XElement xDefaultContent)
        {
            bool defaultContentChanged = false;
            if ((xUserElement == null) || (xDefaultContent == null))
                return false;
            var queryXName = xUserElement.Attributes().Where(c => c.Name == ACxmlnsResolver.xNamespaceWPF + "Name");
            if ((queryXName != null) && (queryXName.Any()))
            {
                string xNameOfUserElement = queryXName.First().Value;
                if (!String.IsNullOrEmpty(xNameOfUserElement))
                {
                    //var queryOriginalElement = xDefaultContent.Descendants().Where
                    //            (c => c.Name == xUserElement.Name
                    //               && c.Attributes().Where(
                    //                                  d => d.Name == "Name" 
                    //                                    && d.Value == xNameOfUserElement).Any());
                    var queryOriginalElement = xDefaultContent.Descendants(xUserElement.Name).Where
                                (c => c.Attributes().Where(
                                                      d => d.Name == ACxmlnsResolver.xNamespaceWPF + "Name"
                                                        && d.Value == xNameOfUserElement).Any());
                    if (queryOriginalElement.Any())
                    {
                        XElement originalXElement = queryOriginalElement.First();
                        foreach (XAttribute xUserAttribute in xUserElement.Attributes())
                        {
                            var queryOriginalAttr = originalXElement.Attributes().Where(c => c.Name == xUserAttribute.Name);
                            if (queryOriginalAttr.Any())
                            {
                                XAttribute originalAttr = queryOriginalAttr.First();
                                if (originalAttr.Value != xUserAttribute.Value)
                                {
                                    originalAttr.Value = xUserAttribute.Value;
                                    defaultContentChanged = true;
                                }
                            }
                        }
                    }
                }
            }

            foreach (XElement xChild in xUserElement.Elements())
            {
                if (CompareDesignsAndUpdate(xChild, xDefaultContent))
                    defaultContentChanged = true;
            }
            return defaultContentChanged;
        }
        #endregion

        #region Overridden IACInteractiveObject Member
        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public override void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            switch (acCommand.GetACUrl())
                            {
                                case Const.CmdDesignModeOn:
                                    DesignModeOn();
                                    return;
                                case Const.CmdDesignModeOff:
                                    DesignModeOff();
                                    return;
                                case Const.CmdPreSetShapeConfig:
                                    PreSetShapeConfig();
                                    return;
                                case Const.CmdResetShapeConfig:
                                    ResetShapeConfig();
                                    return;
                                case Const.CmdAssignShapeConfig:
                                    AssignShapeConfig();
                                    return;
                            }
                        }
                    }
                    break;
            }

            (ContextACObject as IACComponent).ACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public override bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            switch (acCommand.GetACUrl())
                            {
                                case Const.CmdDesignModeOn:
                                    return IsEnabledDesignModeOn();
                                case Const.CmdDesignModeOff:
                                    return IsEnabledDesignModeOff();
                                case Const.CmdPreSetShapeConfig:
                                    return IsEnabledPreSetShapeConfig();
                                case Const.CmdResetShapeConfig:
                                    return IsEnabledResetShapeConfig();
                                case Const.CmdAssignShapeConfig:
                                    return IsEnabledAssignShapeConfig();
                            }
                        }
                    }
                    break;
            }
            return (ContextACObject as IACComponent).IsEnabledACActionToTarget(CurrentTargetVBDataObject, actionArgs);
        }
        #endregion

        #region overriden IACObject Member
        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBDesign, bool>(nameof(ShowCaption), false);

        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        string _ACCaption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public override string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                if (ContentACObject != null)
                    return ContentACObject.ACCaption;
                if (ContextACObject != null)
                    return ContextACObject.ACCaption;
                return "";
            }
            set
            {
                _ACCaption = value;
            }
        }

        private string _CustomizedACCaption;
        /// <summary>
        /// To know is set manually - not by ACCaption.get()
        /// </summary>
        public string CustomizedACCaption
        {
            get
            {
                return _CustomizedACCaption;
            }
            set
            {
                _CustomizedACCaption = value;
            }
        }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public override IEnumerable<IACObject> ACContentList
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


        #endregion

        #region IVBSerialize Member

        /// <summary>
        /// Adds a serializable attributes.
        /// </summary>
        /// <param name="xElement"></param>
        public void AddSerializableAttributes(XElement xElement)
        {
            if (!String.IsNullOrEmpty(AutoStartACComponent))
                xElement.Add(new XAttribute("AutoStartACComponent", AutoStartACComponent));
            if (!String.IsNullOrEmpty(VBContent))
                xElement.Add(new XAttribute("VBContent", VBContent));
        }

        #endregion

        #region IVBContent Member

        /// <summary>
        /// Determines is control enabled or disabled.
        /// </summary>
        public bool Enabled
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
            }
        }
        #endregion

        #region IACInteractiveObject Member

        #endregion

        #region Designer
        /// <summary>
        /// Represents the dependency property for ControlSelectionState.
        /// </summary>
        public static readonly StyledProperty<ControlSelectionState> ControlSelectionStateProperty =
            AvaloniaProperty.Register<VBDesign, ControlSelectionState>(nameof(ControlSelectionState), ControlSelectionState.Off);

        /// <summary>
        /// Gets or sets the control selection state.
        /// </summary>
        public ControlSelectionState ControlSelectionState
        {
            get { return GetValue(ControlSelectionStateProperty); }
            set { SetValue(ControlSelectionStateProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for MasterPageSel.
        /// </summary>
        public static readonly StyledProperty<WPFControlSelectionEventArgs> MasterPageSelProperty = 
            AvaloniaProperty.Register<VBDesign, WPFControlSelectionEventArgs>(nameof(MasterPageSel));

        /// <summary>
        /// Gets or sets the master page selection.
        /// </summary>
        public WPFControlSelectionEventArgs MasterPageSel
        {
            get { return GetValue(MasterPageSelProperty); }
            set { SetValue(MasterPageSelProperty, value); }
        }

        void RootPageWPF_VBDesignEditingEvent()
        {
            if (MasterPageSel == null)
                return;
            ActivateDesignModeSelectionFrame(MasterPageSel.ControlSelectionState);
        }

        /// <summary>
        /// Activates a design mode selection frame.
        /// </summary>
        /// <param name="activate">The control selection state parameter.</param>
        public void ActivateDesignModeSelectionFrame(ControlSelectionState activate)
        {
            switch (activate)
            {
                case ControlSelectionState.Off:
                    if (this.IsDesignerActive)
                    {
                        DesignModeOff();
                    }
                    ControlSelectionState = activate;
                    break;
                case ControlSelectionState.FrameSearch:
                    if (this.IsDesignerActive)
                    {
                        DesignModeOff();
                    }
                    ControlSelectionState = activate;
                    break;
                case ControlSelectionState.FrameSelected:
                    if (this.IsDesignerActive)
                        ControlSelectionState = ControlSelectionState.Off;
                    else
                        ControlSelectionState = activate;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the OnPointerPressed event.
        /// </summary>
        /// <param name="e">The event arguments parameter.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (ControlSelectionState == ControlSelectionState.FrameSearch)
            {
                e.Handled = true;
                if (IsDesignerActive)
                    return;
                if (IsEnabledDesignModeOn())
                {

                    using (ACMonitor.Lock((ACType as ACClass).Database.QueryLock_1X000))
                    {
                        var query = (ACType as ACClass).ACClassMethod_ACClass.Where(c => c.ACIdentifier == "DesignModeOn");
                        if (query.Any())
                        {
                            if ((ACType as ACClass).RightManager.GetControlMode(query.First()) != Global.ControlModes.Enabled)
                                return;
                        }
                    }

                    DesignModeOn();
                }
            }
            else
                base.OnPointerPressed(e);
        }

        /// <summary>
        /// Handles the OnPointerReleased event for right-click context menu.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                if (ContextACObject == null || BSOACComponent == null)
                    return;
                if (DisableContextMenu)
                {
                    e.Handled = true;
                    return;
                }

                if (DesignEditor != null && DesignEditor.DesignSurface.DesignContext != null && DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection != null)
                {
                    Control uiElement = DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.Component as Control;
                    if (uiElement is VBVisual || uiElement is VBVisualGroup || uiElement is VBEdge)
                    {
                        uiElement.RaiseEvent(e);
                        if (e.Handled)
                            return;
                    }
                }

                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this as IACInteractiveObject, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {

                    VBContextMenu vbContextMenu = new VBContextMenu(this as IACInteractiveObject, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open();
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        IACComponentDesignManager _DesignManager = null;
        /// <summary>
        /// Gets the design manager.
        /// </summary>
        /// <param name="create">Determines is create a new design manager.</param>
        /// <returns>The design manager.</returns>
        public IACComponentDesignManager GetDesignManager(bool create = false)
        {

            if (_DesignManager == null)
            {
                if (typeof(IACComponentDesignManager).IsAssignableFrom(ContentACObject.GetType()))
                {
                    _DesignManager = ContentACObject as IACComponentDesignManager;
                }
                else if (create || DesignModeAllways)
                {
                    _DesignManager = GetACComponentByKey(BSOACComponent, "VBDesignerXAML") as IACComponentDesignManager;
                }
            }
            if (_DesignManager != null)
                _DesignManager.VBDesignControl = this;
            return _DesignManager;
        }

        /// <summary>
        /// Presets a configuration for the shape.
        /// </summary>
        [ACMethodInteraction("", "en{'Set Shapeconfig'}de{'Shapekonfiguration setzen'}", (short)MISort.PreSetShapeConfig, false)]
        public void PreSetShapeConfig()
        {
            if (DesignEditor == null || DesignEditor.DesignSurface.DesignContext == null || DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection == null)
                return;

            if (!(DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View is Shape))
                return;
            Shape shape = DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View as Shape;

            DrawShapesAdornerBase.s_ShapeStroke = shape.Stroke.Clone();
            DrawShapesAdornerBase.s_ShapeStrokeThickness = shape.StrokeThickness;
            DrawShapesAdornerBase.s_ShapeFill = shape.Fill.Clone();
            DrawShapesAdornerBase.s_ShapeStrokeDashArray = shape.StrokeDashArray.ToList();
            DrawShapesAdornerBase.s_ShapeStrokeDashOffset = shape.StrokeDashOffset;
            DrawShapesAdornerBase.s_ShapeStrokeLineJoin = shape.StrokeJoin;
            DrawShapesAdornerBase.s_ShapeStretch = shape.Stretch;
            IPen pen = shape.GetPen();
            if (pen != null)
            {
                DrawShapesAdornerBase.s_ShapeStrokeMiterLimit = pen.MiterLimit;
                DrawShapesAdornerBase.s_ShapeStrokeStartLineCap = pen.LineCap;
            }
        }

        /// <summary>
        /// Determines is enabled PreSetShapeConfig or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledPreSetShapeConfig()
        {
            if (!IsDesignerActive)
                return false;
            if (DesignEditor == null || DesignEditor.DesignSurface.DesignContext == null || DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection == null)
                return false;

            if (!(DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View is Shape))
                return false;
            return true;
        }

        /// <summary>
        /// Resets a configuration for the shape.
        /// </summary>
        [ACMethodInteraction("", "en{'Reset Shapeconfig'}de{'Shapekonfiguration zurück setzen'}", (short)MISort.ResetShapeConfig, false)]
        public void ResetShapeConfig()
        {
            Shape shape = DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View as Shape;

            DrawShapesAdornerBase.s_ShapeStroke = Brushes.LightSlateGray;
            DrawShapesAdornerBase.s_ShapeStrokeThickness = 1;
            DrawShapesAdornerBase.s_ShapeFill = null;
            // Note: Reset to default Avalonia values
            DrawShapesAdornerBase.s_ShapeStrokeDashArray = null;
            DrawShapesAdornerBase.s_ShapeStrokeDashOffset = 0;
            DrawShapesAdornerBase.s_ShapeStrokeLineJoin = PenLineJoin.Miter;
            DrawShapesAdornerBase.s_ShapeStrokeMiterLimit = 10;
            DrawShapesAdornerBase.s_ShapeStrokeStartLineCap = PenLineCap.Flat;

            shape.Stroke = DrawShapesAdornerBase.s_ShapeStroke;
            shape.StrokeThickness = DrawShapesAdornerBase.s_ShapeStrokeThickness;
            shape.Fill = DrawShapesAdornerBase.s_ShapeFill;
            shape.StrokeDashArray = new AvaloniaList<double>(DrawShapesAdornerBase.s_ShapeStrokeDashArray);
            if (DrawShapesAdornerBase.s_ShapeStrokeDashOffset.HasValue)
                shape.StrokeDashOffset = DrawShapesAdornerBase.s_ShapeStrokeDashOffset.Value;
            if (DrawShapesAdornerBase.s_ShapeStrokeLineJoin.HasValue)
                shape.StrokeJoin = DrawShapesAdornerBase.s_ShapeStrokeLineJoin.Value;
            // TODO: Not converted from WPF:
            //Pen pen = new Pen();
            //if (DrawShapesAdornerBase.s_ShapeStrokeMiterLimit.HasValue)
            //    pen.MiterLimit = DrawShapesAdornerBase.s_ShapeStrokeMiterLimit.Value;
            //if (DrawShapesAdornerBase.s_ShapeStrokeStartLineCap.HasValue)
            //    pen.LineCap = DrawShapesAdornerBase.s_ShapeStrokeStartLineCap.Value;

        }

        /// <summary>
        /// Determines is enabled ResetShapeConfig or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledResetShapeConfig()
        {
            if (!IsDesignerActive)
                return false;
            if (DesignEditor == null || DesignEditor.DesignSurface.DesignContext == null || DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection == null)
                return false;

            if (!(DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View is Shape))
                return false;
            return true;
        }

        /// <summary>
        /// Assigns a configuration for the shape.
        /// </summary>
        [ACMethodInteraction("", "en{'Assign Shapeconfig'}de{'Shapekonfiguration zuordnen'}", (short)MISort.AssignShapeConfig, false)]
        public void AssignShapeConfig()
        {
            Shape shape = DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View as Shape;

            shape.Stroke = DrawShapesAdornerBase.s_ShapeStroke;
            shape.StrokeThickness = DrawShapesAdornerBase.s_ShapeStrokeThickness;
            shape.Fill = DrawShapesAdornerBase.s_ShapeFill;
            shape.StrokeDashArray = new AvaloniaList<double>(DrawShapesAdornerBase.s_ShapeStrokeDashArray);
            if (DrawShapesAdornerBase.s_ShapeStrokeDashOffset.HasValue)
                shape.StrokeDashOffset = DrawShapesAdornerBase.s_ShapeStrokeDashOffset.Value;
            if (DrawShapesAdornerBase.s_ShapeStrokeLineJoin.HasValue)
                shape.StrokeJoin = DrawShapesAdornerBase.s_ShapeStrokeLineJoin.Value;
            // TODO: Not converted from WPF:
            //Pen pen = new Pen();
            //if (DrawShapesAdornerBase.s_ShapeStrokeMiterLimit.HasValue)
            //    pen.MiterLimit = DrawShapesAdornerBase.s_ShapeStrokeMiterLimit.Value;
            //if (DrawShapesAdornerBase.s_ShapeStrokeStartLineCap.HasValue)
            //    pen.LineCap = DrawShapesAdornerBase.s_ShapeStrokeStartLineCap.Value;
        }

        /// <summary>
        /// Determines is enabled AssignShapeConfig or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledAssignShapeConfig()
        {
            if (!IsDesignerActive)
                return false;
            if (DesignEditor == null || DesignEditor.DesignSurface.DesignContext == null || DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection == null)
                return false;

            if (!(DesignEditor.DesignSurface.DesignContext.Services.Selection.PrimarySelection.View is Shape))
                return false;
            return true;
        }

        /// <summary>
        /// Opens the design editor.
        /// </summary>
        [ACMethodInteraction("", "en{'Designmode on'}de{'Designmodus an'}", (short)MISort.DesignModeOn, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.Utilities)]
        public void DesignModeOn()
        {
            if (!IsEnabledDesignModeOn())
                return;
            IACComponentDesignManager designManager = GetDesignManager(true);

            if (designManager == null)
                return;
            designManager.ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.DesignModeOn));
            ControlSelectionState = ControlSelectionState.Off;
            VBDesignEditor designEditor = new VBDesignEditor();

            designEditor.VBContent = designManager.GetACUrl(ContextACObject) + "\\DesignXAML";

            if (ContentACObject is IACComponentPWNode)
            {
                // TODO: Überprüfen, ob das so richtig ist
                designManager.CurrentDesign = (ContentACObject as IACComponentPWNode).WFContext;
            }
            else
            {
                designManager.CurrentDesign = this.ContentACObject as IACObjectDesign;
            }
            this.Content = designEditor;
            IsDesignerActive = true;

            string dockingManagerName = "";
            VBDockingManager parentDockingManager = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDockingManager)) as VBDockingManager;
            if (parentDockingManager != null)
                dockingManagerName = parentDockingManager.Name;

            designManager.ShowDesignManager(dockingManagerName);
            IACObject propertyWindow = designManager.PropertyWindow;
            if (propertyWindow != null)
            {
                if (propertyWindow is VBDockingContainerBase)
                {
                    VBDockingContainerBase container = propertyWindow as VBDockingContainerBase;
                    if ((container.VBDesignContent != null) && container.VBDesignContent is Control)
                    {
                        (container.VBDesignContent as Control).Loaded += VBPropertyWindowContent_Loaded;
                    }
                }
            }

            IACObject logicalTreeWindow = designManager.LogicalTreeWindow;
            if (logicalTreeWindow != null)
            {
                if (logicalTreeWindow is VBDockingContainerBase)
                {
                    VBDockingContainerBase container = logicalTreeWindow as VBDockingContainerBase;
                    if ((container.VBDesignContent != null) && container.VBDesignContent is Control)
                    {
                        (container.VBDesignContent as Control).Loaded += VBLogicalTreeWindowContent_Loaded;
                    }
                }
            }

            this.Root().RootPageWPF.VBDesignEditingActivated(this);
        }

        /// <summary>
        /// Determines is enabled DesignModeOn or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledDesignModeOn()
        {
            if (this.DataContext == null || this.DataContext.ToString() == "gip.bso.iplus.BSOiPlusStudio")
                return false;
            if (!Database.Root.Environment.License.MayUserDevelop)
            {
                string key = "VBDesignerXAML";
                VBDesignBase vbDesignBaseWithInstanceInfo = FindVBDesignWithInstanceInfoByKey(BSOACComponent, key);
                if (vbDesignBaseWithInstanceInfo == null)
                    return false;

                VBInstanceInfo instanceInfo = vbDesignBaseWithInstanceInfo.InstanceInfoList.GetInstanceInfoByKey(key);
                if (instanceInfo == null)
                    return false;
                if (!instanceInfo.ACIdentifier.StartsWith("VBDesignerWorkflow") && !instanceInfo.ACIdentifier.StartsWith("VBDesignerMaterialWF") &&
                     !Database.Root.Environment.License.MayUserDevelop)
                {
                    return false;
                }
            }
            if (DesignModeAllways)
                return false;
            return !IsDesignerActive;
        }

        void VBPropertyWindowContent_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDesignerActive && (DesignEditor != null))
            {
                Visual vbPropertyWindowContent = sender as Visual;
                var depObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(vbPropertyWindowContent, typeof(VBPropertyGridView)) as Visual;
                if (depObj != null)
                {
                    VBPropertyGridView propertyGridView = depObj as VBPropertyGridView;
                    if ((propertyGridView != null) && (DesignEditor.PropertyGridView == null))
                    {
                        DesignEditor.PropertyGridView = propertyGridView;
                    }
                }
            }
        }

        void VBLogicalTreeWindowContent_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDesignerActive && (DesignEditor != null))
            {
                Visual vbPropertyWindowContent = sender as Visual;
                var depObj = VBLogicalTreeHelper.FindChildObjectInLogicalTree(vbPropertyWindowContent, typeof(VBDesignItemTreeView)) as Visual;
                if (depObj != null)
                {
                    VBDesignItemTreeView logicalTreeView = depObj as VBDesignItemTreeView;
                    if ((logicalTreeView != null) && (DesignEditor.DesignItemTreeView == null))
                    {
                        DesignEditor.DesignItemTreeView = logicalTreeView;
                    }
                }
            }
        }

        /// <summary>
        /// Closes the design editor.
        /// </summary>
        [ACMethodInteraction("", "en{'Designmode off'}de{'Designmodus aus'}", (short)MISort.DesignModeOff, false, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.NoCategory)]
        public void DesignModeOff()
        {
            IACComponentDesignManager designManager = GetDesignManager(true);
            if (designManager == null)
                return;

            IACObject propertyWindow = designManager.PropertyWindow;
            if (propertyWindow != null)
            {
                if (propertyWindow is VBDockingContainerBase)
                {
                    VBDockingContainerBase container = propertyWindow as VBDockingContainerBase;
                    if ((container.VBDesignContent != null) && container.VBDesignContent is Control)
                    {
                        (container.VBDesignContent as Control).Loaded -= VBPropertyWindowContent_Loaded;
                    }
                }
            }

            IACObject logicalTreeWindow = designManager.LogicalTreeWindow;
            if (logicalTreeWindow != null)
            {
                if (logicalTreeWindow is VBDockingContainerBase)
                {
                    VBDockingContainerBase container = logicalTreeWindow as VBDockingContainerBase;
                    if ((container.VBDesignContent != null) && container.VBDesignContent is Control)
                    {
                        (container.VBDesignContent as Control).Loaded -= VBLogicalTreeWindowContent_Loaded;
                    }
                }
            }


            if (this.Content is VBDesignEditor)
            {
                (this.Content as VBDesignEditor).OnCloseAndSaveChanges();
            }
            designManager.HideDesignManager();
            Content = null;
            IsDesignerActive = false;
            LoadDesign();
            adornVBControlManagerList = null;
            if (designManager.ParentACComponent != null)
            {
                designManager.ParentACComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.DesignModeOff));
                designManager.ParentACComponent.StopComponent(designManager);
            }
            _DesignManager = null;
        }

        /// <summary>
        /// Determines is enabled DesignModeOff or disabled.
        /// </summary>
        /// <returns>True if is enabled, otherwise false.</returns>
        public bool IsEnabledDesignModeOff()
        {
            if (DesignModeAllways)
                return false;
            return IsDesignerActive;
        }

        /// <summary>
        /// Determines is allways in a design mode.
        /// </summary>
        public bool DesignModeAllways
        {
            get;
            set;
        }

        private VBDesignEditor DesignEditor
        {
            get
            {
                if (this.Content == null)
                    return null;
                if (this.Content is VBDesignEditor)
                    return this.Content as VBDesignEditor;
                return null;
            }
        }

        /// <summary>
        /// Target object for ACDropData
        /// </summary>
        protected IACInteractiveObject CurrentTargetVBDataObject
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Gets or sets the ACClassDesign.
        /// </summary>
        public ACClassDesign ACClassDesign { get; set; }

        #region Behavior

        #region Behavior -> FocusNames

        public static readonly StyledProperty<string> FocusNamesProperty =
            AvaloniaProperty.Register<VBDesign, string>(nameof(FocusNames));

        [Category("VBControl")]
        public string FocusNames
        {
            get { return GetValue(FocusNamesProperty); }
            set { SetValue(FocusNamesProperty, value); }
        }

        #endregion

        #endregion

        #region Adorner

        private bool _AdornedVBContentInfo = false;

        public void ShowHideVBContentInfo()
        {
            if (!_AdornedVBContentInfo)
            {
                Control fe = this.Content as Control;
                IEnumerable<IVBContent> controls = VBVisualTreeHelper.FindChildObjects<IVBContent>(fe);

                if (controls != null && controls.Any())
                {
                    Avalonia.Controls.Primitives.AdornerLayer layer = Avalonia.Controls.Primitives.AdornerLayer.GetAdornerLayer(this);
                    VBDesignVBContentInfo contentInfo = fe.Resources.Values.OfType<VBDesignVBContentInfo>().FirstOrDefault();

                    if (layer != null)
                    {
                        foreach (IVBContent control in controls)
                        {
                            Control element = control as Control;
                            if (element == null || string.IsNullOrEmpty(control.VBContent) || element is VBConnector)
                                continue;

                            if (control.VBContent == "this")
                                continue;

                            VBDesignVBContentAdorner adorner = new VBDesignVBContentAdorner(element, contentInfo);
                            layer.Children.Add(adorner);
                            element.UpdateLayout();
                        }
                        _AdornedVBContentInfo = true;
                    }
                }
            }
            else
            {
                Visual depObj = this.Content as Visual;
                IEnumerable<IVBContent> controls = VBVisualTreeHelper.FindChildObjects<IVBContent>(depObj);

                if (controls != null && controls.Any())
                {
                    // Note: Avalonia doesn't have AdornerLayer like WPF
                    Avalonia.Controls.Primitives.AdornerLayer layer = Avalonia.Controls.Primitives.AdornerLayer.GetAdornerLayer(this);

                    foreach (IVBContent control in controls)
                    {
                        Control element = control as Control;
                        if (element == null)
                            continue;

                        IEnumerable<Adorner> adorners = layer.GetAdorners(element);
                        if (adorners != null && adorners.Any())
                        {
                            foreach (Adorner adorner in adorners)
                            {
                                VBDesignVBContentAdorner contentAdorner = adorner as VBDesignVBContentAdorner;
                                if (contentAdorner == null)
                                    continue;

                                layer.Children.Remove(contentAdorner);
                            }
                        }
                    }

                    //layer.UpdateLayout();
                    _AdornedVBContentInfo = false;
                }
            }
        }

        #endregion
    }

}
