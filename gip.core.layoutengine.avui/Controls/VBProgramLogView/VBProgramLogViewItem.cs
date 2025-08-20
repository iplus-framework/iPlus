using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBProgramLogViewItem'}de{'VBProgramLogViewItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBProgramLogViewItem : ItemsControl, IACObject
    {
        
        #region ctor's  

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ProgramLogViewItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBProgramLogView/Themes/ProgramLogViewItemStyleGip.xaml" }
            //new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
            //                             styleName = "VBProgramLogViewStyleAero", 
            //                             styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBProgramLogView/Themes/VBProgramLogViewStyleAero.xaml" },
        };

        private bool _ThemeApplied = false;

        static VBProgramLogViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(typeof(VBProgramLogViewItem)));  
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get { return _styleInfoList; }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get { return _styleInfoList; }
        }

        #endregion  

        #region Properties

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ValueOffset.
        /// </summary>
        public double ValueOffset
        {
            get { return (double)GetValue(ValueOffsetProperty); }
            set { SetValue(ValueOffsetProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ValueBrush.
        /// </summary>
        public Brush ValueBrush
        {
            get { return (Brush)GetValue(ValueBrushProperty); }
            set { SetValue(ValueBrushProperty, value); }
        }

        /// <summary>
        /// Determines is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// Determines is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        #endregion

        #region DP

        /// <summary>
        /// Represents the dependency property for Title.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Represents the dependency property for Value.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(Const.Value, typeof(double), typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Represents the dependency property for ValueOffset.
        /// </summary>
        public static readonly DependencyProperty ValueOffsetProperty = DependencyProperty.Register("ValueOffset", typeof(double), typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Represents the dependency property for ValueBrush.
        /// </summary>
        public static readonly DependencyProperty ValueBrushProperty = DependencyProperty.Register("ValueBrush", typeof(Brush), typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Represents the dependency property for IsExpanded.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = TreeViewItem.IsExpandedProperty.AddOwner(typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Represents the dependency property for IsSelected.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = TreeViewItem.IsSelectedProperty.AddOwner(typeof(VBProgramLogViewItem), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region Protected

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (!_ThemeApplied)
                _ThemeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, false);
        }

        /// <summary>
        /// Gets the container for item override.
        /// </summary>
        /// <returns>The new instance of VBProgramLogViewItem.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new VBProgramLogViewItem();
        }

        /// <summary>
        /// Determines is item overrides it's own container.
        /// </summary>
        /// <param name="item">The item parameter.</param>
        /// <returns>True if is item VBProgramLogViewItem, otherwise false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is VBProgramLogViewItem;
        }

        /// <summary>
        /// Prepares container for item override.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="item">The item parameter.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element != item)
            {
                base.PrepareContainerForItemOverride(element, item);
                if (element is VBProgramLogViewItem)
                    ((VBProgramLogViewItem)element).PrepareContainer((VBProgramLogViewItem)element, item);
            }
        }

        #endregion

        #region Friend

        internal void PrepareContainer(VBProgramLogViewItem Element, object Item)
        {
            if (Item is ACProgramLog)
            {
                int Level = 0;
                ACProgramLog LogItem = (ACProgramLog)Item;

                Element.Title = LogItem.ACUrl;
                Element.Value = LogItem.DurationSec * 10;
                Element.ValueOffset = LogItem.StartDate.Value.Subtract(GetRootProgramLog(LogItem, ref Level).StartDate.Value).TotalSeconds * 10;
                Element.ItemsSource = LogItem.ACProgramLog_ParentACProgramLog.OrderBy("StartDate", true);
                Element.ValueBrush = Brushes.Lime;
            }
        }

        #endregion

        #region IACObject

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
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
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { throw new NotImplementedException(); }
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

        #endregion

        #region Private

        private ACProgramLog GetRootProgramLog(ACProgramLog Item, ref int Level)
        {
            Level = 0;

            while (Item.ACProgramLog1_ParentACProgramLog != null)
            {
                Item = Item.ACProgramLog1_ParentACProgramLog;
                Level++;
            }

            return Item;
        }

        #endregion

    }
}
