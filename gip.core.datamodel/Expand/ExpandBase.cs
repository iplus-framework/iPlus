using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioMaterial, "en{'ExpandBase'}de{'ExpandBase'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, true)]
    public abstract class ExpandBase : IVBDataCheckbox, IACContainerWithItems, IACObject, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region c'tors

        public ExpandBase(IACObject item, int index = 1, double treeQuantityRatio = 1, ExpandBase parent = null)
        {
            Item = item;
            Sequence = index;
            TreeQuantityRatio = treeQuantityRatio;
            IsEnabled = true;
            Parent = parent;
        }

        #endregion

        #region Properties

        #region Properties -> Tree structure

        public IACObject Item { get; set; }
        public int Sequence
        {
            get;
            set;
        }

        public ExpandBase Root
        {
            get
            {
                if (Parent == null)
                    return this;
                else
                    return Parent.Root;
            }
        }
        public ExpandBase Parent { get; set; }

        private List<ExpandBase> _Children;
        public List<ExpandBase> Children
        {
            get
            {
                if (_Children == null)
                    _Children = new List<ExpandBase>();
                return _Children;
            }
            set
            {
                _Children = value;
            }
        }

        public string TreeVersion
        {
            get
            {
                string treeVersion = Sequence.ToString();
                if (Parent != null)
                    treeVersion = Parent.TreeVersion + "." + treeVersion;
                return treeVersion;
            }
        }


        public double TreeQuantityRatio { get; set; }

        #endregion

        #region Properties -> IVBDataCheckbox

        private bool _IsEnabled;
        [ACPropertyInfo(9999, "", "en{'Enabled'}de{'Ist möglich'}")]
        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        [ACPropertyInfo(9999)]
        public string DataContentCheckBox
        {
            get { return "IsChecked"; }
        }


        private bool _IsChecked;


        [ACPropertyInfo(9999)]
        public bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        #endregion

        #region IACObject
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return Children; }
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
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return false;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return Parent; }
        }

        IACContainerWithItems IACContainerWithItems.ParentContainer
        {
            get
            {
                return Parent;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (Parent == null)
                    return this;
                return Parent.RootContainer;
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
        #endregion

        #region IACContainerWithItems

        /// <summary>Container-Childs</summary>
        /// <value>Container-Childs</value>
        [ACPropertyInfo(9999)]
        public IEnumerable<IACContainerWithItems> Items
        {
            get { return Children; }
        }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public virtual void Add(IACContainerWithItems child)
        {
            throw new NotImplementedException();
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public virtual bool Remove(IACContainerWithItems child)
        {
            throw new NotImplementedException();
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        /// <exception cref="NotImplementedException"></exception>
        public virtual object Value
        {
            get; set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public virtual core.datamodel.ACClass ValueTypeACClass
        {
            get
            {
                IACObject iObject = Value as IACObject;
                return iObject == null ? null : iObject.ACType as core.datamodel.ACClass;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public virtual string ACIdentifier
        {
            get { return ACCaption; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public virtual string ACCaption
        {
            get
            {
                return null;
            }
        }

        #endregion

        #endregion

        #region Methods

        public virtual void LoadTree()
        {

        }

        /// <summary>
        /// Return complete list of all included partslist
        /// </summary>
        /// <returns></returns>
        public List<ExpandResult> BuildTreeList(List<ExpandResult> list = null)
        {
            if (list == null)
                list = new List<ExpandResult>();
            ExpandResult expandItem = new ExpandResult();
            expandItem.TreeVersion = TreeVersion;
            expandItem.Item = this;
            list.Add(expandItem);
            if (Children.Any())
            {
                foreach (var item in Children)
                    item.BuildTreeList(list);
            }
            return list;
        }

        public void DoAction(Action<ExpandBase> action)
        {
            action(this);
            foreach(ExpandBase child in Children)
                action(child);
        }

        #endregion

    }
}
