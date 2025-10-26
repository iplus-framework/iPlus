using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBProgramLogViewItem'}de{'VBProgramLogViewItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBProgramLogViewItem : SelectingItemsControl, IACObject
    {
 
        #region Properties
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public double Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ValueOffset.
        /// </summary>
        public double ValueOffset
        {
            get { return GetValue(ValueOffsetProperty); }
            set { SetValue(ValueOffsetProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ValueBrush.
        /// </summary>
        public IBrush ValueBrush
        {
            get { return GetValue(ValueBrushProperty); }
            set { SetValue(ValueBrushProperty, value); }
        }

        /// <summary>
        /// Determines is expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get { return GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        #endregion

        #region Styled Properties

        /// <summary>
        /// Represents the styled property for Title.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty = 
            AvaloniaProperty.Register<VBProgramLogViewItem, string>(nameof(Title));

        /// <summary>
        /// Represents the styled property for Value.
        /// </summary>
        public static readonly StyledProperty<double> ValueProperty = 
            AvaloniaProperty.Register<VBProgramLogViewItem, double>(nameof(Value), 0.0);

        /// <summary>
        /// Represents the styled property for ValueOffset.
        /// </summary>
        public static readonly StyledProperty<double> ValueOffsetProperty = 
            AvaloniaProperty.Register<VBProgramLogViewItem, double>(nameof(ValueOffset), 0.0);

        /// <summary>
        /// Represents the styled property for ValueBrush.
        /// </summary>
        public static readonly StyledProperty<IBrush> ValueBrushProperty = 
            AvaloniaProperty.Register<VBProgramLogViewItem, IBrush>(nameof(ValueBrush), Brushes.LightGray);

        /// <summary>
        /// Represents the styled property for IsExpanded.
        /// </summary>
        public static readonly StyledProperty<bool> IsExpandedProperty = 
            TreeViewItem.IsExpandedProperty.AddOwner<VBProgramLogViewItem>();


        #endregion

        #region Protected

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            return CreateContainerItem(item);
        }

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<VBProgramLogViewItem>(item, out recycleKey);
        }

        internal static VBProgramLogViewItem CreateContainerItem(object item)
        {
            VBProgramLogViewItem element = new VBProgramLogViewItem();
            if (item is ACProgramLog logItem)
            {
                int level = 0;
                element.Title = logItem.ACUrl;
                element.Value = logItem.DurationSec * 10;
                // Fix: Call GetRootProgramLog on the new element instance
                element.ValueOffset = logItem.StartDate.Value.Subtract(element.GetRootProgramLog(logItem, ref level).StartDate.Value).TotalSeconds * 10;
                element.ItemsSource = logItem.ACProgramLog_ParentACProgramLog.OrderBy("StartDate", true);
                element.ValueBrush = Brushes.Lime;
            }
            return element;
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
