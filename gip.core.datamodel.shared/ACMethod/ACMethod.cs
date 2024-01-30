using System.Runtime.CompilerServices;
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACMethod.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Ein ACMethod ist ein ACValueList, welche zustätzlich noch "Methodennamen" und "ClienJobPoint" als Eigenschaften bereit stellt.
    /// TODO: ACMethod: Überlegen ob der ACMethod weitere Informationen enthalten sollte, welche die Priorisierung, Aussführungszeitpunkt
    /// oder andere Informationen für die ausführende Server-JobPoint enthält, welche dort zu berücksichtigen sind.
    /// Es könnte evtl. eine Eigenschaft "IScheduleInfo ScheduleInfo" o.ä. hinzugefügt werden, welche individuell
    /// die bei der Joblistenabarbeitung mit ausgewertet wird.
    /// Alternativ könnte bei der Deklaration ([XMLACMethodInfo("...) angegeben werden das nicht ein ACMethod, sondern eine
    /// davon abgeleitete Klasse in der ACClassMethod.XMLACMethod Spalte bereit gestellt wird.
    /// TODO: ACMethod: Ein ACJob sollte beim Aufrufenden (Client) und beim Aufgerufenen (Server, Kopie) gespeichert werden.
    /// Die Events an die entsprechende JobPointSubcr sollte immer mittels "ACValueList" (oder einer spezialisierten
    /// Ableitung) erfolgen in welcher immer die orignale ACJobID und der aktuelle ACState gestetzt sind.
    /// </summary>
    [DataContract]
#if NETFRAMEWORK
    [KnownType(typeof(ACValue))]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACMethod'}de{'ACMethod'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACMethod : ACMethodDescriptor, INotifyPropertyChanged, IACObject, IACEntityProperty
#else
    public class ACMethod : ACMethodDescriptor, INotifyPropertyChanged
#endif
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethod"/> class.
        /// </summary>
        public ACMethod() : base()
        {
            if (_ParameterValueList != null)
            {
                _ParameterValueList.ListChanged += new ListChangedEventHandler(ParameterValueList_ListChanged);
                _ParameterValueList.ParentACMethod = this;
            }
            else if (_ResultValueList != null)
            {
                _ResultValueList.ListChanged += new ListChangedEventHandler(ResultValueList_ListChanged);
                _ResultValueList.ParentACMethod = this;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ACMethod"/> class.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        public ACMethod(string methodName) : base(methodName)
        {
            if (_ParameterValueList != null)
            {
                _ParameterValueList.ListChanged += new ListChangedEventHandler(ParameterValueList_ListChanged);
                _ParameterValueList.ParentACMethod = this;
            }
            else if (_ResultValueList != null)
            {
                _ResultValueList.ListChanged += new ListChangedEventHandler(ResultValueList_ListChanged);
                _ResultValueList.ParentACMethod = this;
            }
        }
        #endregion

        #region Virtual Methods Registration
#if NETFRAMEWORK
        private class VirtualMethodsNameDict : Dictionary<string, List<ACMethodWrapper>>
        {
            public void RegisterVirtualMethod(ACMethodWrapper acMethodWrapper)
            {
                List<ACMethodWrapper> existingList = null;
                if (!TryGetValue(acMethodWrapper.Method.ACIdentifier, out existingList))
                {
                    existingList = new List<ACMethodWrapper>();
                    Add(acMethodWrapper.Method.ACIdentifier, existingList);
                }
                existingList.Add(acMethodWrapper);
            }

            public ACMethodWrapper FindWrapper(ACMethod forACMethod)
            {
                List<ACMethodWrapper> existingList = null;
                if (!TryGetValue(forACMethod.ACIdentifier, out existingList))
                {
                    if (forACMethod.ACIdentifier.Contains(ACUrlHelper.AttachedMethodIDConcatenator))
                    {
                        string[] parts = forACMethod.ACIdentifier.Split(ACUrlHelper.AttachedMethodIDConcatenator);
                        if (!TryGetValue(parts[0].Trim(), out existingList))
                            return null;
                    }
                    else
                        return null;
                }
                if (existingList.Count == 1)
                    return existingList.FirstOrDefault();
                var query = existingList.Where(c => c.Method.ParameterValueList.Count == forACMethod.ParameterValueList.Count && c.Method.ResultValueList.Count == forACMethod.ResultValueList.Count);
                if (query.Count() == 1)
                    return query.FirstOrDefault();
                foreach (ACMethodWrapper wrapperToCheck in query)
                {
                    int i = 0;
                    bool nextWrapper = false;
                    foreach (var acValue in wrapperToCheck.Method.ParameterValueList)
                    {
                        i++;
                        if (acValue.ACIdentifier != forACMethod.ParameterValueList[i-1].ACIdentifier)
                        {
                            nextWrapper = true;
                            break;
                        }
                    }
                    if (!nextWrapper)
                    {
                        i = 0;
                        foreach (var acValue in wrapperToCheck.Method.ResultValueList)
                        {
                            i++;
                            if (acValue.ACIdentifier != forACMethod.ResultValueList[i-1].ACIdentifier)
                            {
                                nextWrapper = true;
                                break;
                            }
                        }
                    }
                    if (!nextWrapper)
                        return wrapperToCheck;
                }

                return existingList.FirstOrDefault();
            }
        }

        private class VirtualMethodsDict : Dictionary<string, ACMethodWrapper>
        {
            public void RegisterVirtualMethod(ACMethodWrapper acMethodWrapper, VirtualMethodsNameDict globalNamesRegistry)
            {
                ACMethodWrapper existingWrapper = null;
                if (!TryGetValue(acMethodWrapper.Method.ACIdentifier, out existingWrapper))
                {
                    Add(acMethodWrapper.Method.ACIdentifier, acMethodWrapper);
                    if (globalNamesRegistry != null)
                        globalNamesRegistry.RegisterVirtualMethod(acMethodWrapper);
                }
            }
            public ACMethod GetVirtualMethod(string virtualMethodName, bool getClone)
            {
                ACMethodWrapper existingWrapper = null;
                if (!TryGetValue(virtualMethodName, out existingWrapper))
                    return null;
                if (getClone)
                    return (ACMethod)existingWrapper.Method.Clone();
                return existingWrapper.Method;
            }
        }

        private class AssemblyMethodDict : Dictionary<string, VirtualMethodsDict>
        {
            public void RegisterVirtualMethod(string assemblyMethodName, ACMethodWrapper acMethodWrapper, VirtualMethodsNameDict globalNamesRegistry)
            {
                VirtualMethodsDict virtMethodDict = null;
                if (!TryGetValue(assemblyMethodName, out virtMethodDict))
                {
                    virtMethodDict = new VirtualMethodsDict();
                    Add(assemblyMethodName, virtMethodDict);
                }
                virtMethodDict.RegisterVirtualMethod(acMethodWrapper, globalNamesRegistry);
            }

            public ACMethod GetVirtualMethod(string assemblyMethodName, string virtualMethodName, bool getClone)
            {
                VirtualMethodsDict virtMethodDict = null;
                if (!TryGetValue(assemblyMethodName, out virtMethodDict))
                    return null;
                return virtMethodDict.GetVirtualMethod(virtualMethodName, getClone);
            }

            public IReadOnlyList<ACMethodWrapper> GetVirtualMethodInfos(string assemblyMethodName)
            {
                VirtualMethodsDict virtMethodDict = null;
                if (!TryGetValue(assemblyMethodName, out virtMethodDict))
                    return null;
                return virtMethodDict.Values.ToList().AsReadOnly();
            }
        }

        private class TypesDict : Dictionary<Type, AssemblyMethodDict>
        {
            public void RegisterVirtualMethod(Type typeOfACComponent, string assemblyMethodName, ACMethodWrapper acMethodWrapper, VirtualMethodsNameDict globalNamesRegistry)
            {
                AssemblyMethodDict assemblyMethodDict = null;
                if (!TryGetValue(typeOfACComponent, out assemblyMethodDict))
                {
                    assemblyMethodDict = new AssemblyMethodDict();
                    Add(typeOfACComponent, assemblyMethodDict);
                }
                assemblyMethodDict.RegisterVirtualMethod(assemblyMethodName,acMethodWrapper, globalNamesRegistry);
            }

            public ACMethod GetVirtualMethod(Type typeOfACComponent, string assemblyMethodName, string virtualMethodName, bool getClone)
            {
                AssemblyMethodDict assemblyMethodDict = null;
                if (!TryGetValue(typeOfACComponent, out assemblyMethodDict))
                {
                    // This invokes the static constructor. The static constructor adds methods to this Dictionary
                    if (typeof(IACComponent).IsAssignableFrom(typeOfACComponent))
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeOfACComponent.TypeHandle);
                    // Try again
                    if (!TryGetValue(typeOfACComponent, out assemblyMethodDict))
                        return null;
                }
                return assemblyMethodDict.GetVirtualMethod(assemblyMethodName, virtualMethodName, getClone);
            }

            public IReadOnlyList<ACMethodWrapper> GetVirtualMethodInfos(Type typeOfACComponent, string assemblyMethodName)
            {
                AssemblyMethodDict assemblyMethodDict = null;
                if (!TryGetValue(typeOfACComponent, out assemblyMethodDict))
                    return null;
                return assemblyMethodDict.GetVirtualMethodInfos(assemblyMethodName);
            }
        }

        static TypesDict _MethodsRegistry;
        static object _LockDict = new object();

        static VirtualMethodsNameDict _NamesRegistry;
        static bool _NamesRegistryFilledWithReflection = false;

        public static void RegisterVirtualMethod(Type typeOfACComponent, string assemblyMethodName, ACMethod acMethod, string acMethodDescTranslation, Type pwClass)
        {
            RegisterVirtualMethod(typeOfACComponent, assemblyMethodName, new ACMethodWrapper(acMethod, acMethodDescTranslation, pwClass));
        }

        public static void RegisterVirtualMethod(Type typeOfACComponent, string assemblyMethodName, ACMethodWrapper acMethodWrapper)
        {
            if (typeOfACComponent == null)
                throw new ArgumentNullException("typeOfACComponent is null");
            if (String.IsNullOrEmpty(assemblyMethodName))
                throw new ArgumentNullException("assemblyMethodName is emtpy");
            if (acMethodWrapper == null)
                throw new ArgumentNullException("ACMethodWrapper is null");
            if (acMethodWrapper.Method == null || String.IsNullOrEmpty(acMethodWrapper.Method.ACIdentifier))
                throw new ArgumentNullException("ACMethodWrapper not property initialized");
            lock (_LockDict)
            {
                if (_MethodsRegistry == null)
                    _MethodsRegistry = new TypesDict();
                if (_NamesRegistry == null)
                    _NamesRegistry = new VirtualMethodsNameDict();
                _MethodsRegistry.RegisterVirtualMethod(typeOfACComponent, assemblyMethodName, acMethodWrapper, _NamesRegistry);
            }
        }

        public static ACMethod GetVirtualMethod(Type typeOfACComponent, string assemblyMethodName, string virtualMethodName, bool getClone=true)
        {
            if (_MethodsRegistry == null)
                return null;
            lock (_LockDict)
            {
                return _MethodsRegistry.GetVirtualMethod(typeOfACComponent, assemblyMethodName, virtualMethodName, getClone);
            }
        }

        public static IReadOnlyList<ACMethodWrapper> GetVirtualMethodInfos(Type typeOfACComponent, string assemblyMethodName)
        {
            if (_MethodsRegistry == null)
                return null;
            lock (_LockDict)
            {
                return _MethodsRegistry.GetVirtualMethodInfos(typeOfACComponent, assemblyMethodName);
            }
        }

        public static void InheritFromBase(Type thisType, string assemblyMethodName)
        {
            if (thisType == null || thisType.BaseType == null || String.IsNullOrEmpty(assemblyMethodName))
                throw new ArgumentNullException();
            IReadOnlyList<ACMethodWrapper> wrappers = ACMethod.GetVirtualMethodInfos(thisType.BaseType, assemblyMethodName);
            if (wrappers != null)
            {
                foreach (var w in wrappers)
                {
                    ACMethod.RegisterVirtualMethod(thisType, assemblyMethodName, w);
                }
            }
        }

        public static List<ACMethodWrapper> OverrideFromBase(Type thisType, string assemblyMethodName)
        {
            if (thisType == null || thisType.BaseType == null || String.IsNullOrEmpty(assemblyMethodName))
                throw new ArgumentNullException();
            List<ACMethodWrapper> clonedWrappers = new List<ACMethodWrapper>();
            IReadOnlyList<ACMethodWrapper> wrappers = ACMethod.GetVirtualMethodInfos(thisType.BaseType, assemblyMethodName);
            if (wrappers != null)
            {
                foreach (var w in wrappers)
                {
                    ACMethodWrapper w2 = (ACMethodWrapper)w.Clone();
                    clonedWrappers.Add(w2);
                    ACMethod.RegisterVirtualMethod(thisType, assemblyMethodName, w2);
                }
            }
            return clonedWrappers;
        }
#endif

        #endregion

        #region Properties

        private bool? _UseCultureInfoForConversion;
        public bool? UseCultureInfoForConversion
        {
            get
            {
                return _UseCultureInfoForConversion;
            }
            set
            {
                _UseCultureInfoForConversion = value;
            }
        }


        public bool FullSerialization { get; set; }
#if NETFRAMEWORK
        /// <summary>
        /// The _ database
        /// </summary>
        protected IACEntityObjectContext _Database = null;
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        public virtual IACEntityObjectContext Database
        {
            get
            {
                return _Database;
            }
            set
            {
                _Database = value;
            }
        }
#endif

        /// <summary>
        /// The _ parameter value list
        /// </summary>
        ACValueList _ParameterValueList;
        /// <summary>
        /// Gets or sets the parameter value list.
        /// </summary>
        /// <value>The parameter value list.</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(9999, "", "en{'Parameter'}de{'Parameter'}")]
#endif
        public ACValueList ParameterValueList
        {
            get
            {
                if (_ParameterValueList == null)
                {
                    _ParameterValueList = new ACValueList(this);
                    _ParameterValueList.ListChanged += new ListChangedEventHandler(ParameterValueList_ListChanged);
                }
                if (_ParameterValueList.ParentACMethod != this)
                    _ParameterValueList.ParentACMethod = this;

                return _ParameterValueList;
            }
            set
            {
                if (_ParameterValueList != null)
                    _ParameterValueList.ListChanged -= ParameterValueList_ListChanged;
                
                _ParameterValueList = value;
                if (_ParameterValueList != null && _ParameterValueList.ParentACMethod != this)
                    _ParameterValueList.ParentACMethod = this;

                if (_ParameterValueList != null)
                    _ParameterValueList.ListChanged += new ListChangedEventHandler(ParameterValueList_ListChanged);

                OnPropertyChanged("ParameterValueList");
            }
        }


        /// <summary>
        /// The _ result value list
        /// </summary>
        ACValueList _ResultValueList;
        /// <summary>
        /// Gets or sets the result value list.
        /// </summary>
        /// <value>The result value list.</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(9999, "", "en{'Result'}de{'Ergebnis'}")]
#endif
        public ACValueList ResultValueList
        {
            get
            {
                if (_ResultValueList == null)
                {
                    _ResultValueList = new ACValueList(this);
                    _ResultValueList.ListChanged += new ListChangedEventHandler(ResultValueList_ListChanged);
                }
                if (_ResultValueList.ParentACMethod != this)
                    _ResultValueList.ParentACMethod = this;
                return _ResultValueList;
            }
            set
            {
                if (_ResultValueList != null)
                    _ResultValueList.ListChanged -= ResultValueList_ListChanged;

                _ResultValueList = value;
                if (_ResultValueList != null && _ResultValueList.ParentACMethod != this)
                    _ResultValueList.ParentACMethod = this;

                if (_ResultValueList != null)
                    _ResultValueList.ListChanged += new ListChangedEventHandler(ResultValueList_ListChanged);

                OnPropertyChanged("ResultValueList");
            }
        }

        /// <summary>
        /// The _ valid message
        /// </summary>
        protected MsgWithDetails _ValidMessage;
        /// <summary>
        /// Gets the valid message.
        /// </summary>
        /// <value>The valid message.</value>
        public MsgWithDetails ValidMessage
        {
            get
            {
                if (_ValidMessage == null)
                {
                    _ValidMessage = new MsgWithDetails();
                }
                return _ValidMessage;
            }
        }

        /// <summary>
        /// The _ auto remove
        /// </summary>
        bool _AutoRemove = false;
        /// <summary>
        /// If Async-Call is Processed/Finished on Server-Side then Callback to Client-Object will be made.
        /// If Client-Object is not reachable any more or it is stopped:
        /// If AutoRemove is not set, ProcessingState remains Completed until Client-Object is reachable again
        /// -&gt; Server-Object waits with processing of the next Async-Call in it's Point-List
        /// This Mode is in case of, if Client-Point is also Storable e.g. Model or Workflow-Objects
        /// If AutoRemove is set, and Client-Object is not reachable. Then Entry in Point will be automatically Removed.
        /// This Mode is in case of, if Client-Point is not Storable and is a dynamic Instance e.g. BSO-Objects.
        /// </summary>
        /// <value><c>true</c> if [auto remove]; otherwise, <c>false</c>.</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(9999, "", "en{'AutoRemove'}de{'AutoRemove'}")]
#endif
        public bool AutoRemove
        {
            get
            {
                return _AutoRemove;
            }
            set
            {
                _AutoRemove = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Attaching
#if NETFRAMEWORK
        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        public void AttachTo(IACObject acObject)
        {
            if (acObject == null)
                return;
            if (ParameterValueList != null)
                ParameterValueList.AttachTo(acObject);
            if (ResultValueList != null)
                ResultValueList.AttachTo(acObject);
        }

        public void Detach(bool detachFromDBContext = false)
        {
            if (ParameterValueList != null)
                ParameterValueList.Detach(detachFromDBContext);
            if (ResultValueList != null)
                ResultValueList.Detach(detachFromDBContext);
            _Database = null;
        }
#endif
#endregion


        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;
            if (!ParameterValueList.IsValid(this.ValidMessage))
                return false;
            return true;
        }

        /// <summary>
        /// Copies from if different.
        /// </summary>
        /// <param name="from">From.</param>
        public override void CopyFromIfDifferent(ACMethodDescriptor from)
        {
            base.CopyFromIfDifferent(from);
            ACMethod acMethod = from as ACMethod;
            if (acMethod == null)
                return;
            if (this._AutoRemove != acMethod.AutoRemove)
                this._AutoRemove = acMethod.AutoRemove;
            // TODO: ParameterList, ResultValueList?
        }

        public override void CopyFrom(ACMethodDescriptor from)
        {
            if (from == null)
                return;
            base.CopyFrom(from);
            ACMethod acMethod = from as ACMethod;
            if (acMethod == null)
                return;
            this._AutoRemove = acMethod.AutoRemove;
            _ParameterValueList = (ACValueList) acMethod.ParameterValueList.Clone();
            if (_ParameterValueList != null)
            {
                _ParameterValueList.ListChanged += new ListChangedEventHandler(ParameterValueList_ListChanged);
                _ParameterValueList.ParentACMethod = this;
            }

            _ResultValueList = (ACValueList) acMethod.ResultValueList.Clone();
            if (_ResultValueList != null)
            {
                _ResultValueList.ListChanged += new ListChangedEventHandler(ResultValueList_ListChanged);
                _ResultValueList.ParentACMethod = this;
            }
        }

        public virtual void CopyParamValuesFrom(ACMethod from)
        {
            foreach (var acValue in ParameterValueList)
            {
                var acValueFrom = from.ParameterValueList.GetACValue(acValue.ACIdentifier);
                if (acValueFrom != null 
                    && acValueFrom.ObjectFullType == acValue.ObjectFullType
                    && acValueFrom.Value != null)
                {
                    acValue.Value = acValueFrom.Value;
                }
            }
        }

        public override object Clone()
        {
            ACMethod clone = new ACMethod();
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void ResultValueList_ListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged("ResultValueList");
        }

        protected virtual void ParameterValueList_ListChanged(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged("ParameterValueList");
        }


        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ACIdentifier; }
        }

#if NETFRAMEWORK
        private static gip.core.datamodel.ACClass _ReflectedACType = null;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public virtual IACType ACType
        {
            get
            {
                if (_ReflectedACType == null)
                    _ReflectedACType = this.ReflectACType() as gip.core.datamodel.ACClass;
                return _ReflectedACType;
            }
        }

        public ACClass TypeAsACClass
        {
            get
            {
                return this.ACType as ACClass;
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
                return this.ReflectGetACContentList();
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
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
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
#endif

        public object this[string property]
        {
            get
            {
                //return ParameterValueList[property];
                ACValue param = ParameterValueList.GetACValue(property);
                if (param == null)
                {
                    param = ResultValueList.GetACValue(property);
#if NETFRAMEWORK
                    if (param == null && TypeAsACClass != null)
                    {
                        ACClassProperty acProp = TypeAsACClass.Properties.Where(c => c.ACIdentifier == property).FirstOrDefault();
                        if (acProp != null)
                        {
                            param = new ACValue(property, acProp.ObjectFullType);
                            ParameterValueList.Add(param);
                        }
                    }
#endif
                }
                if (param == null)
                    return null;
                return param.Value;
            }
            set
            {
                ACValue param = ParameterValueList.GetACValue(property);
                if (param == null)
                {
                    param = ResultValueList.GetACValue(property);
#if NETFRAMEWORK
                    if (param == null && TypeAsACClass != null)
                    {
                        ACClassProperty acProp = TypeAsACClass.Properties.Where(c => c.ACIdentifier == property).FirstOrDefault();
                        if (acProp != null)
                        {
                            param = new ACValue(property, acProp.ObjectFullType, value);
                            ParameterValueList.Add(param);
                        }
                    }
                    else
                        param.Value = value;
#else
                    if (param != null)
                        param.Value = value;
#endif
                }
                else
                    param.Value = value;
                OnPropertyChanged(property);
            }
        }

#if NETFRAMEWORK
        public virtual string XMLConfig
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
#endif


        public void OnEntityPropertyChanged(string property)
        {
            OnPropertyChanged(property);
        }


#if NETFRAMEWORK
        public ACPropertyManager ACProperties
        {
            get { throw new NotImplementedException(); }
        }

        private ACMethodWrapper _Wrapper;
        public string GetACCaptionForACValue(ACValue acValue)
        {
            if (acValue == null)
                throw new ArgumentNullException("acValue");
            if (_Wrapper == null)
            {
                if (_NamesRegistry != null)
                {
                    _Wrapper = _NamesRegistry.FindWrapper(this);
                }
                if ((_NamesRegistry == null || _Wrapper == null) && !_NamesRegistryFilledWithReflection)
                {
                    try
                    {
                        gip.core.datamodel.Database.GlobalDatabase.EnterCS();
                        var query = gip.core.datamodel.Database.GlobalDatabase.ACClassMethod
                            .Where(c => c.ParentACClassMethodID.HasValue && c.ACKindIndex == (short)Global.ACKinds.MSMethod && !String.IsNullOrEmpty(c.ACClass.AssemblyQualifiedName))
                            .Select(c => c.ACClass.AssemblyQualifiedName).Distinct();
                        _NamesRegistryFilledWithReflection = true;
                        foreach (string assemblyQName in query)
                        {
                            Type type = Type.GetType(assemblyQName);
                            if (type != null)
                            {
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACMethod", "GetACCaptionForACValue", msg);
                    }
                    finally
                    {
                        gip.core.datamodel.Database.GlobalDatabase.LeaveCS();
                    }
                }
                if (_NamesRegistry == null)
                    return acValue.ACIdentifier;
                _Wrapper = _NamesRegistry.FindWrapper(this);
            }
            if (_Wrapper == null)
                return acValue.ACIdentifier;
            if (this.ParameterValueList.Contains(acValue))
                return _Wrapper.GetParameterACCaption(acValue.ACIdentifier);
            if (this.ResultValueList.Contains(acValue))
                return _Wrapper.GetResultParamACCaption(acValue.ACIdentifier);
            ACValue acValueClone = this.ParameterValueList.Where(c => c.ACIdentifier == acValue.ACIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetParameterACCaption(acValue.ACIdentifier);
            acValueClone = this.ResultValueList.Where(c => c.ACIdentifier == acValue.ACIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetResultParamACCaption(acValue.ACIdentifier);
            return acValue.ACIdentifier;
        }

        public string GetACCaptionTransForACValue(ACValue acValue)
        {
            if (acValue == null)
                throw new ArgumentNullException("acValue");
            if (_Wrapper == null)
            {
                if (_NamesRegistry != null)
                {
                    _Wrapper = _NamesRegistry.FindWrapper(this);
                }
                if ((_NamesRegistry == null || _Wrapper == null) && !_NamesRegistryFilledWithReflection)
                {
                    try
                    {
                        gip.core.datamodel.Database.GlobalDatabase.EnterCS();
                        var query = gip.core.datamodel.Database.GlobalDatabase.ACClassMethod
                            .Where(c => c.ParentACClassMethodID.HasValue && c.ACKindIndex == (short)Global.ACKinds.MSMethod && !String.IsNullOrEmpty(c.ACClass.AssemblyQualifiedName))
                            .Select(c => c.ACClass.AssemblyQualifiedName).Distinct();
                        _NamesRegistryFilledWithReflection = true;
                        foreach (string assemblyQName in query)
                        {
                            Type type = Type.GetType(assemblyQName);
                            if (type != null)
                            {
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACMethod", "GetACCaptionForACValue", msg);
                    }
                    finally
                    {
                        gip.core.datamodel.Database.GlobalDatabase.LeaveCS();
                    }
                }
                if (_NamesRegistry == null)
                    return acValue.ACIdentifier;
                _Wrapper = _NamesRegistry.FindWrapper(this);
            }
            if (_Wrapper == null)
                return acValue.ACIdentifier;
            if (this.ParameterValueList.Contains(acValue))
                return _Wrapper.GetParameterACCaptionTrans(acValue.ACIdentifier);
            if (this.ResultValueList.Contains(acValue))
                return _Wrapper.GetResultParamACCaptionTrans(acValue.ACIdentifier);
            ACValue acValueClone = this.ParameterValueList.Where(c => c.ACIdentifier == acValue.ACIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetParameterACCaptionTrans(acValue.ACIdentifier);
            acValueClone = this.ResultValueList.Where(c => c.ACIdentifier == acValue.ACIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetResultParamACCaptionTrans(acValue.ACIdentifier);
            return acValue.ACIdentifier;
        }

        public string GetACCaptionForACIdentifier(string acIdentifier)
        {
            if (String.IsNullOrEmpty(acIdentifier))
                throw new ArgumentNullException("acIdentifier");
            if (_Wrapper == null)
            {
                if (_NamesRegistry != null)
                {
                    _Wrapper = _NamesRegistry.FindWrapper(this);
                }
                if ((_NamesRegistry == null || _Wrapper == null) && !_NamesRegistryFilledWithReflection)
                {
                    try
                    {
                        gip.core.datamodel.Database.GlobalDatabase.EnterCS();
                        var query = gip.core.datamodel.Database.GlobalDatabase.ACClassMethod
                            .Where(c => c.ParentACClassMethodID.HasValue && c.ACKindIndex == (short)Global.ACKinds.MSMethod && !String.IsNullOrEmpty(c.ACClass.AssemblyQualifiedName))
                            .Select(c => c.ACClass.AssemblyQualifiedName).Distinct();
                        _NamesRegistryFilledWithReflection = true;
                        foreach (string assemblyQName in query)
                        {
                            Type type = Type.GetType(assemblyQName);
                            if (type != null)
                            {
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACMethod", "GetACCaptionForACIdentifier", msg);
                    }
                    finally
                    {
                        gip.core.datamodel.Database.GlobalDatabase.LeaveCS();
                    }
                }
                if (_NamesRegistry == null)
                    return acIdentifier;
                _Wrapper = _NamesRegistry.FindWrapper(this);
            }
            if (_Wrapper == null)
                return acIdentifier;
            if (this.ParameterValueList.Where(c => c.ACIdentifier == acIdentifier).Any())
                return _Wrapper.GetParameterACCaption(acIdentifier);
            if (this.ResultValueList.Where(c => c.ACIdentifier == acIdentifier).Any())
                return _Wrapper.GetResultParamACCaption(acIdentifier);
            ACValue acValueClone = this.ParameterValueList.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetParameterACCaption(acIdentifier);
            acValueClone = this.ResultValueList.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
            if (acValueClone != null)
                return _Wrapper.GetResultParamACCaption(acIdentifier);
            return acIdentifier;
        }
#endif

    }
}
