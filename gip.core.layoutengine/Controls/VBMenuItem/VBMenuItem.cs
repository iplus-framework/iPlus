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
using System.IO;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Transactions;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control element for displaying a menu item.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung eines Menüeintrags.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBMenuItem'}de{'VBMenuItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBMenuItem : MenuItem, IACInteractiveObject, IACObject
    {
        #region c´tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "MenuItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenuItem/Themes/MenuItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "MenuItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBMenuItem/Themes/MenuItemStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMenuItem), new FrameworkPropertyMetadata(typeof(VBMenuItem)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBMenuItem.
        /// </summary>
        public VBMenuItem()
        {
        }

        /// <summary>
        /// Creates a new instance of VBMenuItem.
        /// </summary>
        /// <param name="acComponent">The acComponent parameter.</param>
        /// <param name="acCommand">The acCommand parameter.</param>
        public VBMenuItem(IACObject acComponent, ACCommand acCommand)
        {
            ContextACObject = acComponent;
            ACCommand = acCommand;

            if(acCommand is ACMenuItem)
            {
                ACMenuItem menuItem = acCommand as ACMenuItem;
                if(!string.IsNullOrEmpty(menuItem.IconACUrl))
                    LoadIcon(menuItem.IconACUrl);
            }

            if (!string.IsNullOrEmpty(acCommand.GetACUrl()))
            {
                Command = AppCommands.AddApplicationCommand(ACCommand);
                CommandBindings.Add(new CommandBinding(Command, VBMenuItem_Click, VBMenuItem_IsEnabled));
            }
        }

        private void VBMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
            ACAction(actionArgs);
        }

        private void VBMenuItem_IsEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ACCommand.IsAutoEnabled)
            {
                e.CanExecute = true;
                return;
            }
            if (ContextACObject != null)
            {
                ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                e.CanExecute = IsEnabledACAction(actionArgs);
            }
            else
            {
                e.CanExecute = false;
            }
        }
        #endregion

        internal void LoadIcon(string iconACUrl)
        {
            object icon = null;

            IACComponent acComponent = Layoutgenerator.CurrentACComponent;
            if (acComponent == null)
                acComponent = Layoutgenerator.Root;

            Application wpfApplication = null;
            if (acComponent != null && acComponent.Root.RootPageWPF != null && acComponent.Root.RootPageWPF.WPFApplication != null)
                wpfApplication = acComponent.Root.RootPageWPF.WPFApplication as Application;

            if (wpfApplication != null)
               icon = wpfApplication.Resources[iconACUrl];

            if (icon != null && icon is BitmapImage)
                Icon = new Image() { Source = icon as BitmapImage, MaxHeight = 16, MaxWidth = 16 };
            else if (icon != null && icon is string)
                Icon = null;
            else
            {
                ACClassDesign design = null;
                string iconACIdentifier = iconACUrl.Split('(').Last().Split(')').First();

                using (ACMonitor.Lock(Database.GlobalDatabase.QueryLock_1X000))
                    design = Database.GlobalDatabase.ACClassDesign.Where(c => c.ACUsageIndex == (short)Global.ACUsages.DUIcon && c.ACIdentifier == iconACIdentifier)
                                                                  .ToList().FirstOrDefault(c => c.GetACUrl(null) == iconACUrl);

                if (design != null && design.DesignBinary != null)
                {
                    using (var ms = new MemoryStream(design.DesignBinary))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        if(wpfApplication != null)
                            wpfApplication.Resources.Add(iconACUrl, bitmapImage);
                        Icon = new Image() { Source = bitmapImage, MaxHeight = 16, MaxWidth = 16 };
                    }
                }
                else if (wpfApplication != null)
                    wpfApplication.Resources.Add(iconACUrl, "");
            }
        }

        #region IACInteractiveObject Member

        /// <summary>
        /// Gets or sets the VBContentValueType.
        /// </summary>
        public IACType VBContentValueType
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBMenuItem));

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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
            set;
        }

        IACObject _ContextACObject;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return _ContextACObject;
            }
            set
            {
                _ContextACObject = value;
            }
        }

        IACInteractiveObject HandlerACElement
        {
            get
            {
                if (ACCommand == null || ACCommand.HandlerACElement == null)
                    return ContextACObject as IACInteractiveObject;

                return ACCommand.HandlerACElement;
            }
        }

        ACCommand _ACCommand;
        /// <summary>
        /// Gets or sets the ACCommand.
        /// </summary>
        public ACCommand ACCommand
        {
            get
            {
                return _ACCommand;
            }
            set
            {
                _ACCommand = value;
                if (_ACCommand != null)
                {
                    this.Header = _ACCommand.ACCaption;
                }
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
                List<IACObject> acContentList = new List<IACObject>();
                acContentList.Add(_ACCommand);

                return acContentList;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            actionArgs.DropObject = this;
            if (HandlerACElement != null)
                HandlerACElement.ACAction(actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            if (actionArgs == null)
                return false;
            actionArgs.DropObject = this;
            if (HandlerACElement == null)
                return false;
            return HandlerACElement.IsEnabledACAction(actionArgs);
        }
        #endregion

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        #region IACObject
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
        #endregion

        #region IACObject Member

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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
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

        /// <summary>
        /// Determines is autoenabled or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsAutoEnabled
        {
            get;
            set;
        }
    }
}
