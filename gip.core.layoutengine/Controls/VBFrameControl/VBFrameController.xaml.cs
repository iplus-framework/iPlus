using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFrameController'}de{'VBFrameController'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public partial class VBFrameController : Frame, IACInteractiveObject, IACObject
    {
        public VBFrameController()
        {
            InitializeComponent();
        }

        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.LoadCompleted += VBFrameController_LoadCompleted;
            this.Navigated += VBFrameController_Navigated;
            this.DataContextChanged += VBFrameController_DataContextChanged;
            this.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            this.JournalOwnership = System.Windows.Navigation.JournalOwnership.OwnsJournal;
        }

        private void VBFrameController_Navigated(object sender, NavigationEventArgs e)
        {
            if (clearNavigationOnLoad && CanGoBack)
            {
                this.CloseAndRemoveVBDesign(this.Content as VBPage);
            }
        }

        private void VBFrameController_LoadCompleted(object sender, NavigationEventArgs e)
        {
        }

        private void VBFrameController_ContentRendered(object sender, EventArgs e)
        {
            if (clearNavigationOnLoad && CanGoBack)
            {
                ClearNavigation();
            }
            clearNavigationOnLoad = false;
            if (this.Content == null && CanGoBack)
            {
                this.ClearEmptyBackEntry();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        public List<UIElement> _VBDesignList = new List<UIElement>();
        public List<UIElement> VBDesignList
        {
            get
            {
                return _VBDesignList;
            }
            set
            {
                _VBDesignList = value;
            }
        }

        private bool clearNavigationOnLoad = false;

        #region IACUrl Member

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return this.Name;
            }
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

        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return null;
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
            if (acUrl == Const.EventDeInit)
            {
                //RemoveFromComponentReference();
                //DeInitVBControl();
            }
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
            if (acUrl == Const.EventDeInit)
            {
                return true;
            }
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
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

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public virtual string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        #endregion

        #region IACInteractiveObject
        
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            return;
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

        #endregion

        #region Design

        [ACMethodInfo("", "en{'Show Dialog'}de{'Dialog'}", 9999)]
        public void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDesignList.Add(vbDesign);
            ShowVBDesign(vbDesign, acCaption, false);
        }

        [ACMethodInfo("", "en{'Show Window'}de{'Window'}", 9999)]
        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;

            VBDesignList.Add(vbDesign);
            ShowVBDesign(vbDesign);
        }

        public void ShowDesign(string acUrl, IACBSO bsoObject, string acCaption)
        {
            if (ContextACObject != null)
            {
                if (acUrl.IndexOf('#') != -1)
                {
                    string checkACUrl = acUrl.Replace("#", "\\?");
                    var x = ContextACObject.ACUrlCommand(checkACUrl);
                    if (x != null)
                        return;
                }
                VBDesign vbDesign = new VBDesign();
                vbDesign.Name = String.Format("BSO{0}", VBDesignList.Count);
                if (!string.IsNullOrEmpty(acCaption))
                    vbDesign.ACCaption = acCaption;

                vbDesign.DataContext = bsoObject;
                vbDesign.VBContent = acUrl;
                VBDesignList.Add(vbDesign);
                ShowVBDesign(vbDesign, "", false);
            }
        }

        public void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption, string title = "", bool ribbonVisibilityOff = false, Global.VBDesignDockState dockState = Global.VBDesignDockState.Tabbed)
        {
            if (this.Content != null)
                ClearAllHistory();
            if (ContextACObject != null)
            {
                clearNavigationOnLoad = true;
                if (acUrl.IndexOf('#') != -1)
                {
                    string checkACUrl = acUrl.Replace("#", "\\?");
                    var x = ContextACObject.ACUrlCommand(checkACUrl);
                    if (x != null)
                        return;
                }
                VBDesign vbDesign = new VBDesign();
                vbDesign.Name = String.Format("BSO{0}", VBDesignList.Count);
                if (!string.IsNullOrEmpty(acCaption))
                    vbDesign.ACCaption = acCaption;

                vbDesign.AutoStartACComponent = acUrl;
                vbDesign.AutoStartParameter = parameterList;

                VBDesignList.Add(vbDesign);
                ShowVBDesign(vbDesign, title, true);
            }
        }

        private void ShowVBDesign(UIElement uiElement, string acCaption = "", bool isBusinessObject = false)
        {
            if (uiElement == null)
                return;
            IVBContent uiElementAsDataContent = null;
            if (uiElement is IVBContent)
            {
                uiElementAsDataContent = (uiElement as IVBContent);
                if (uiElementAsDataContent.ContextACObject == null)
                {
                    if (uiElement is FrameworkElement)
                    {
                        if ((uiElement as FrameworkElement).DataContext == null)
                            (uiElement as FrameworkElement).DataContext = this.ContextACObject;
                    }
                }
                if (uiElementAsDataContent.ContextACObject == null)
                    return;
            }

            if (uiElement is VBDesign)
            {
                VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
                // Rechtepr�fung ob Design ge�ffnet werden darf
                if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
                {
                    if (uiElementAsDataContent.ContextACObject is IACComponent)
                    {
                        if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                            return;
                    }
                }
            }

            this.Navigate(new VBPage(this, uiElement, isBusinessObject, acCaption));
        }

        #endregion

        #region VBRibbon

        public VBRibbonMobile vBRibbonMobile;

        public void CreateVBRibbon()
        {
            if (this.vBRibbonMobile == null)
                vBRibbonMobile = new VBRibbonMobile();
        }

        public void ShowVBRibbon(VBGrid grid)
        {
            if (vBRibbonMobile == null)
                CreateVBRibbon();

            var page = this.Content as VBPage;
            vBRibbonMobile.DataContext = page.ContextACObject;
            grid.BSOACComponent = this.ContextACObject as IACBSO;
            grid.Children.Add(vBRibbonMobile);
        }

        public void RemoveVBRibbon(VBGrid grid)
        {
            if (vBRibbonMobile == null)
                return;

            grid.Children.Remove(vBRibbonMobile);
        }

        #endregion

        #region Navigation

        void VBFrameController_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null && e.OldValue != null)
            {
                IACBSO bso = e.OldValue as IACBSO;
                if (bso != null)
                {

                }
                //DeInitVBControl(bso);
            }
        }

        public void ClearEmptyBackEntry()
        {
            this.GoBack();
            while (this.NavigationService.RemoveBackEntry() != null) ;
        }

        public void ClearContent()
        {
            this.CloseAndRemoveVBDesign(this.Content as VBPage);
            while (this.NavigationService.RemoveBackEntry() != null) ;
            if (this.CanGoForward)
            {
                ClearFowardStack();
            }
            this.Content = null;
        }

        public void ClearNavigation()
        {
            while (this.NavigationService.RemoveBackEntry() != null) ;
            if (this.CanGoForward)
            {
                ClearFowardStack();
            }
        }

        public void ClearAllHistory()
        {
            this.CloseAndRemoveVBDesign(this.Content as VBPage);
            this.Content = null;
            var entriesToRemove = new List<JournalEntry>();
            foreach (JournalEntry entry in this.BackStack)
                entriesToRemove.Add(entry);
            foreach (JournalEntry entry in entriesToRemove)
            {
                var prop = entry.GetType().GetProperty("KeepAliveRoot", BindingFlags.Instance | BindingFlags.NonPublic);
                VBPage entryPage = prop.GetValue(entry) as VBPage;
                this.CloseAndRemoveVBDesign(entryPage, true);
            }
            if (this.CanGoForward)
            {
                ClearFowardStack();
            }
        }

        public void ClearFowardStack()
        {
            this.GoForward();
            this.ClearContent();
        }

        public void CloseAndRemoveVBDesign(VBPage page, bool stopStartBSO = false)
        {
            if (page == null)
                return;
            UIElement uiElement = page.VBDesignContent;
            if (uiElement == null)
                return;
            if (!(uiElement is VBDesign))
                return;
            VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
            if (!this.NavigationService.CanGoBack && page.isBusinessObject || stopStartBSO)
                uiElementAsDataDesign.StopAutoStartComponent();
            VBDesignList.Remove(uiElement);
            this.Focus();
        }

        public bool CheckIfEntryEmpty(JournalEntry entry)
        {
            var prop = entry.GetType().GetProperty("KeepAliveRoot", BindingFlags.Instance | BindingFlags.NonPublic);
            VBPage entryPage = prop.GetValue(entry) as VBPage;
            if (entryPage.VBDesignContent is VBDesign vBDesign && vBDesign.ACCompInitState == ACInitState.Constructing)
                return true;
            else
                return false;
        }

        #endregion

    }
}
