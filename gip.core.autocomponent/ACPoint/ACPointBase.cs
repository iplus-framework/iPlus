using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPointBase'}de{'ACPointBase'}", Global.ACKinds.TACClass)]
    public abstract class ACPointBase<T> : IACObject, IACPoint<T> where T : IACObject 
    {
        #region Events and Delegates
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region c'tors
        public ACPointBase(IACComponent parent, string acPropertyName) :
            this(parent, acPropertyName, 0)
        {
        }

        public ACPointBase(IACComponent parent, string acPropertyName, uint maxCapacity)
        {
            _ACComponent = new ACRef<IACComponent>(parent,this,true);
            _ACTypeInfo = parent.ComponentClass.GetMember(acPropertyName);
            _MaxCapacity = maxCapacity;
            if (_ACComponent.ValueT != null && _ACComponent.ValueT is ACComponent)
                (_ACComponent.ValueT as ACComponent).AddOrReplaceACMember(this);
        }
        
        /// <summary>
        /// Constructor for automatic Instantiation over Reflection in ACInitACPoint()
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="maxCapacity"></param>
        public ACPointBase(IACComponent parent, ACClassProperty acClassProperty, uint maxCapacity)
        {
            _ACComponent = new ACRef<IACComponent>(parent,this,true);
            _ACTypeInfo = acClassProperty;
            _MaxCapacity = maxCapacity;
            if (_ACComponent.ValueT != null)
                (_ACComponent.ValueT as ACComponent).AddOrReplaceACMember(this);
        }

        #endregion

        #region IACMember Member

        /// <summary>Gets or sets the value of a member as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value 
        {
            get
            {
                return this;
            }
            set
            {
                OnMemberChanged();
            }
        }

        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.
        /// </param>
        public void OnMemberChanged(EventArgs e = null)
        {
        }

        [IgnoreDataMember]
        protected ACRef<IACComponent> _ACComponent = null;
        /// <summary>Smart-Pointer to the Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        [DataMember]
        public ACRef<IACComponent> ACRef
        {
            get
            {
                return _ACComponent;
            }
            protected set
            {
                _ACComponent = value;
            }
        }

        protected IACType _ACTypeInfo;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return _ACTypeInfo;
            }
        }

        /// <summary>iPlus-Type (Metadata) of this point.</summary>
        /// <value>ACClassProperty</value>
        [IgnoreDataMember]
        public ACClassProperty PropertyInfo
        {
            get
            {
                return ACType as ACClassProperty;
            }
        }


        /// <summary>Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public IACComponent ParentACComponent 
        {
            get
            {
                return _ACComponent.ValueT;
            }
        }

        /// <summary>
        /// This method is called from the iPlus-Framework for each member of a ACComponent when a component was recycled from the component-pool (ACInitState.RecycledFromPool) instead of a new creation.
        /// </summary>
        /// <param name="recycledComponent">The recycled component.</param>
        public void RecycleMemberAndAttachTo(IACComponent recycledComponent)
        {
            if (recycledComponent != null && _ACComponent != null)
            {
                _ACComponent.Value = recycledComponent;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                if (this.ToString() == "gip.core.autocomponent.ACPointReference")
                    return "ACPointReference";
                if (_ACTypeInfo == null) return null;
                return _ACTypeInfo.ACIdentifier;
            }
        }

        #endregion

        #region IACUrl
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
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public string GetACUrlComponent(IACObject rootACObject = null) 
        {
            return null;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return _ACComponent != null ? _ACComponent.ValueT : null;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get;
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
        #endregion

        #region IACConnectionPointBase Member

        protected uint _MaxCapacity = 0; // Infinite
        /// <summary>Maximum capacity of the point (of the ConnectionList). 0 = Unlimited</summary>
        /// <value>The maximum capacity.</value>
        public uint MaxCapacity
        {
            get
            {
                return _MaxCapacity;
            }
        }

        /// <summary>Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        public virtual void ACDeInit(bool deleteACClassTask = false)
        {
        }

        /// <summary>The ConnectionList as serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        public virtual string ValueSerialized(bool xmlIndented = false)
        {
            return ConnectionList.ToString();
        }


        #endregion

        #region EventHandling Methods
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        #region IACPoint<T> Member

        /// <summary>
        /// List of relations to other objects.
        /// It's use to describe the relationships to other objects.
        /// </summary>
        public abstract IEnumerable<T> ConnectionList { get; }

        [ACPropertyInfo(800, "", "en{'Info about Connectionentries'}de{'Informationen über Einträge'}")]
        public virtual string ConnectionListInfo 
        { 
            get
            {
                try
                {
                    if (ConnectionList == null || !ConnectionList.Any())
                        return base.ToString() + "(Empty)";
                    StringBuilder sb = new StringBuilder();
                    int index = 0;
                    foreach (var entry in ConnectionList)
                    {
                        sb.AppendLine(String.Format("[{0}]: {1}", index, entry.ACCaption));
                        index++;
                    }
                    return sb.ToString();
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACComponent", "ConnectionListInfo", msg);
                }
                return base.ToString();
            }
        }
        #endregion
    }
}
