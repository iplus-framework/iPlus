// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-19-2012
// ***********************************************************************
// <copyright file="IACComponent.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Enum ConnectionQuality
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ConnectionQuality'}de{'ConnectionQuality'}", Global.ACKinds.TACEnum)]
    public enum ConnectionQuality : short
    {
        /// <summary>
        /// The no connections
        /// </summary>
        NoConnections = 0,
        /// <summary>
        /// The good
        /// </summary>
        Good = 1,
        /// <summary>
        /// The instable
        /// </summary>
        Instable = 2,
        /// <summary>
        /// The bad
        /// </summary>
        Bad = 3,
    }


    /// <summary>
    /// Enum ACObjectConnectionState
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACObjectConnectionState'}de{'ACObjectConnectionState'}", Global.ACKinds.TACEnum)]
    public enum ACObjectConnectionState : short
    {
        /// <summary>
        /// The dis connected
        /// </summary>
        DisConnected = 0,
        /// <summary>
        /// The connected
        /// </summary>
        Connected = 1,
        /// <summary>
        /// The values received
        /// </summary>
        ValuesReceived = 2,
    }


    /// <summary>
    /// Enum ACOperationModes
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACOperationModes'}de{'ACOperationModes'}", Global.ACKinds.TACEnum)]
    public enum ACOperationModes : short
    {
        /// <summary>
        /// The live
        /// </summary>
        Live = 0,
        /// <summary>
        /// The test
        /// </summary>
        Test = 1,
        /// <summary>
        /// The simulation
        /// </summary>
        Simulation = 2,
    }


    /// <summary>
    /// ACInitState describes the Lifecycle of a ACComponent<para />
    /// Normal Lifecycle with Stop:     Constructing - Constructed - Initializing - Initialized ... Destructing - Destructed<para />
    /// Dispose to Pool:                ...Initialized - DisposingToPool - DisposedToPool...<para />
    /// Recycle from Pool:              ...DisposedToPool - RecyclingFromPool - RecycledFromPool - Initialized...<para />
    /// Reload from Definition:         ...Initialized - Reloading - Reloaded - Initialized... />
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACInitState'}de{'ACInitState'}", Global.ACKinds.TACEnum)]
    public enum ACInitState
    {
        /// <summary>
        /// State when Constructor() of a ACComponent ist invoked
        /// </summary>
        Constructing,

        /// <summary>
        /// State after Construct()-Method was called
        /// </summary>
        Constructed,


        /// <summary>
        /// If Component is poolable:
        /// State when ACDeInit()-Method is called
        /// </summary>
        DisposingToPool,

        /// <summary>
        /// If Component is poolable:
        /// State when ACDeInit()-Method is completed and Component is in Pool
        /// </summary>
        DisposedToPool,


        /// <summary>
        /// If Component is poolable:
        /// State when Recycle()-Method is called and Component is taken from pool
        /// </summary>
        RecyclingFromPool,

        /// <summary>
        /// If Component is poolable:
        /// State when Recycle()-Method is completed
        /// </summary>
        RecycledFromPool,


        /// <summary>
        /// State when Reload()-Method is called
        /// </summary>
        Reloading,

        /// <summary>
        /// State when Reload()-Method is completed
        /// </summary>
        Reloaded,


        /// <summary>
        /// State when ACInit() starts
        /// </summary>
        Initializing,

        /// <summary>
        /// State when ACPostInit() is completed
        /// </summary>
        Initialized,


        /// <summary>
        /// If Component is notpoolable:
        /// State when ACDeInit()-Method is called
        /// </summary>
        Destructing,

        /// <summary>
        /// If Component is not poolable:
        /// State when ACDeInit()-Method is completed and Component is unloaded and release for .NET-Garbagecollector
        /// </summary>
        Destructed,
    }


    /// <summary>Enum for describing the current printing state</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintingPhase'}de{'ACPrintingPhase'}", Global.ACKinds.TACEnum)]
    public enum ACPrintingPhase : short
    {
        /// <summary>Phase when a XPSDocument is generated and before the ReportPaginator (DocumentPaginator) starts to fill the XAML-Report with values.</summary>
        Started = 0,
        /// <summary>  ReportPaginator could not complete his work and the XPS-Document was not generated.</summary>
        Cancelled = 1,
        /// <summary>  ReportPaginator has completed his work and the XPS-Document was generated.</summary>
        Completed = 2,
    }


    /// <summary>
    /// Enum ACStartCompOptions
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Options for starting ACComponents'}de{'Optionen zum Starten von ACComponents'}", Global.ACKinds.TACEnum)]
    [Flags]
    public enum ACStartCompOptions : short
    {
        /// <summary>
        /// The component will definitely start 
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Only Components will be started which lobal.ACStartTypes is set to AutomaticOnDemand or Automatic
        /// </summary>
        OnlyAutomatic = 0x1,

        /// <summary>
        /// Only for Proxies:  Block querying server if instance is loaded on server-side => 
        /// Component will not be started
        /// </summary>
        NoServerReqFromProxy = 0x2,
    }
    

    /// <summary>
    /// Interface for ACComponent
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACComponent'}de{'IACComponent'}", Global.ACKinds.TACInterface)]
    public interface IACComponent : IACObjectWithInit, IACMember, IACInteractiveObject
    {
        #region Instance, Childs and Parent

        /// <summary>The primary IACObject that IACComponent encapsulates.</summary>
        /// <value>Content of type IACObject</value>
        IACObject Content { get; }

        /// <summary>
        /// Entity which represents the persisted object-state of a ACComponent-Instance
        /// </summary>
        ACClassTask ContentTask { get; }


        /// <summary>
        /// Root of all Application-Trees
        /// </summary>
        IRoot Root { get; }


        /// <summary>
        /// Returns the type of this Component
        /// ATTENTION: Usage of this Entity must be ensured with using the QueryLock_1X000 of the Global Database-Context
        /// Otherwise it's NOT THREAD-SAFE!
        /// </summary>
        ACClass ComponentClass { get; }


        /// <summary>
        /// If Proxy, then a real instance exists on Serverside.
        /// This instance can only be a ACComponentProxy or a derivation of it
        /// </summary>
        bool IsProxy { get; }


        /// <summary>
        /// Operation-Mode
        /// </summary>
        ACOperationModes ACOperationMode { get; }


        /// <summary>
        /// Main-State of a Component (Controls the Lifecycle)
        /// </summary>
        ACInitState InitState { get; }


        /// <summary>
        /// In order for your ACComponent-Class to be poolable, you must reset all your class members and remove references in the overridden ACDeInit method. 
        /// This corresponds to the state that is present after creation by the constructor.
        /// If this is the case, return true.
        /// </summary>
        bool IsPoolable { get; }


        /// <summary>
        /// Connection-State (Only for proxies)
        /// </summary>
        ACObjectConnectionState ConnectionState { get; }


        /// <summary>
        /// Thread-Safe access to Childs. Returns a new List for threads-safe iteration.
        /// </summary>
        [ACPropertyInfo(9999)]
        IEnumerable<IACComponent> ACComponentChilds { get; }


        /// <summary>
        /// If this instance is a proxy, then the server is queried for child instances and then the missing instances generated and attached below this proxy as children.
        /// After that, the ACComponentChilds-Property will be returned.
        /// </summary>
        [ACPropertyInfo(9999)]
        IEnumerable<IACComponent> ACComponentChildsOnServer { get; }


        /// <summary>
        /// Searches for a instance in the Parent-Relation which .NET-Type is a derivation or same .NET-Type as the passed type
        /// </summary>
        /// <param name="type">.NET Type</param>
        /// <returns>First occurrence or null if not found</returns>
        IACComponent FindParentComponent(Type type);


        /// <summary>
        /// Searches for a instance in the Parent-Relation which ACClass is derived from the passed ACClass
        /// </summary>
        /// <param name="type">iPlus-Type</param>
        /// <returns>First occurrence or null if not found</returns>
        IACComponent FindParentComponent(ACClass type);


        /// <summary>
        /// Searches for a instance in the Parent-Relation which is of type TResult or a derivation of it
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <returns>Null if not found</returns>
        TResult FindParentComponent<TResult>() where TResult : IACComponent;


        /// <summary>
        /// Searches for a instance in the Parent-Relation which is of type TResult or a derivation of it 
        /// and matches the passed search-condition
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <param name="selector">Search Condition</param>
        /// <returns>Null if not found</returns>
        TResult FindParentComponent<TResult>(Func<IACComponent, bool> selector) where TResult : IACComponent;


        /// <summary>Gets a child that matches the passed acIdentifier.</summary>
        /// <param name="acIdentifier">The acIdentifier. It's the clasname of the Child.</param>
        /// <param name="includeDerivedDynamicChilds">
        /// If the acIdentifier is a classname of a base-class that wasn't defined in the Application-tree because it was added dynamically during the runtime, then find a child that is a derivation of it. 
        /// In this case the child has a different ACIdentifier in the ACMemberList because it have the classname of the derivation. 
        /// If set to true than the search for this derivation should take place if no instance was found that matched the passed acIdentifier.
        /// </param>
        /// <returns>The child as IACComponent</returns>
        IACComponent GetChildComponent(string acIdentifier, bool includeDerivedDynamicChilds = false);


        /// <summary>
        /// Searches for a child-instances in the subtree which ACClass is derived from the passed ACClass
        /// </summary>
        /// <param name="acClass">iPlus-Type</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        List<IACComponent> FindChildComponents(ACClass acClass, int maxChildDepth = 0);


        /// <summary>
        /// Searches for a child-instances in the subtree which is of type TResult or a derivation of it
        /// DEPRECATED! Please use FindChildComponents-Method with selector instead
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        List<TResult> FindChildComponents<TResult>(int maxChildDepth = 0) where TResult : IACComponent;


        /// <summary>
        /// Searches for a child-instances in the subtree which is of type TResult or a derivation of it
        /// and matches the passed search-condition (selector) and matches the passed ignore-condition (deselector) 
        /// If ignore-condition is matched, the recursive search-alogrithm breaksand doesn't continue to look further in the current subtree
        /// </summary>
        /// <typeparam name="TResult">Class</typeparam>
        /// <param name="selector">search-condition, can be null</param>
        /// <param name="deselector">ignore/break-condition, can be null. If deselector equals the selector, then the first matching node will be returned.</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        List<TResult> FindChildComponents<TResult>(Func<IACComponent, bool> selector, Func<IACComponent, bool> deselector = null, int maxChildDepth = 0) where TResult : IACComponent;


        /// <summary>
        /// Searches for a child-instances in the subtree which .NET-Type is a derivation or same .NET-Type as the passed type
        /// </summary>
        /// <param name="typeOfComponent">.NET Type</param>
        /// <param name="maxChildDepth">Max search depth in subtree</param>
        /// <returns>A List of found children</returns>
        List<IACComponent> FindChildComponents(Type typeOfComponent, int maxChildDepth = 0);


        /// <summary>
        /// Starts/Creates a new Instance as a child of this instance
        /// </summary>
        /// <param name="acClass">Type which should be instantiated</param>
        /// <param name="content">This parameter is passed to the second parameter "IACObject content" of a ACComponent-Constructor.</param>
        /// <param name="acParameter">This parameter is passed to the fourth parameter "ACValueList parameter" of a ACComponent-Constructor. 
        /// Call the ACClass.TypeACSignature()-Method of the corresponding iPlus type that you want to instance to get the right ACValueList (from your passed acClass parameter)</param>
        /// <param name="startChildMode">Controls if further childrens should be created automatically</param>
        /// <param name="asProxy"></param>
        /// <param name="acNameInstance">Identifier which should given to the new created instance</param>
        /// <returns></returns>
        IACObjectWithInit StartComponent(ACClass acClass, object content, ACValueList acParameter, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, bool asProxy = false, string acNameInstance = "");


        /// <summary>
        /// Starts/Creates a new Instance as a child of this instance
        /// </summary>
        /// <param name="acName">This parameter, you pass the name of the Child-Class/instance, 
        /// which was defined by the development environment in the application tree below the class in which this instance is currently located (this-pointer). 
        /// If no class could be found in the application tree with this ACIdentifier, then it is checked whether it is a business object class (derivation of ACBSO). 
        /// If so, then a new dynamic "child instance" is created below the instance in which this instance is currently located.</param>
        /// <param name="content">This parameter is passed to the second parameter "IACObject content" of a ACComponent-Constructor.</param>
        /// <param name="acParameter">Ths parameter must contain the parameters that are passed to the fourth constructor parameter as "ACValueList parameter". Here you can do this in two different ways:
        /// A) object[] acParameter contains only one element of the type ACValueList, which already contains all the required parameters according to ACClassConstructorInfo. 
        /// To do this, call the ACClass.TypeACSignature()-Method of the corresponding iPlus type that you want to instance. 
        /// This method returns a ACValueList filled with the necessary parameters, which you pass as the only element in the acparameter array.
        /// B) object[] acParameter contains exactly the parameters required, according to ACClassConstructorInfo.
        /// The StartComponent method internally converts the array into an ACValueList.</param>
        /// <param name="startOptions">Start-Options</param>
        /// <returns></returns>
        IACObjectWithInit StartComponent(string acName, object content, object[] acParameter, ACStartCompOptions startOptions = ACStartCompOptions.Default);


        /// <summary>
        /// Starts/Creates a new Instance through a ACChildInstanceInfo
        /// </summary>
        /// <param name="instanceInfo">Info which was retrieved from the server first</param>
        /// <param name="startChildMode">Controls if further childrens should be created automatically</param>
        /// <param name="asProxy">Force to create instance as a proxy</param>
        /// <returns></returns>
        IACObjectWithInit StartComponent(ACChildInstanceInfo instanceInfo, Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic, bool asProxy = false);


        /// <summary>
        /// Queries the server for childs and get a tree of ACChildInstanceInfo-Instances
        /// </summary>
        /// <param name="maxChildDepth">Limits the search to a defined depth. If maxChildDepth=0 then the search goes to the deepest child in the tree</param>
        /// <param name="onlyWorkflows">If set to true, then only derivations of PWBase will be returned (Workflow-Nodes)</param>
        /// <param name="acIdentifier">If set, then only child with this acIdentifer will be returned</param>
        /// <returns>Childs on server-side</returns>
        IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, bool onlyWorkflows, string acIdentifier = "");


        /// <summary>
        /// Queries the server for childs and get a tree of ACChildInstanceInfo-Instances
        /// </summary>
        /// <param name="maxChildDepth">Limits the search to a defined depth. If maxChildDepth=0 then the search goes to the deepest child in the tree</param>
        /// <param name="searchParam">Complex filter criteria</param>
        /// <returns>Childs on server-side</returns>
        IEnumerable<ACChildInstanceInfo> GetChildInstanceInfo(int maxChildDepth, ChildInstanceInfoSearchParam searchParam);


        /// <summary>
        /// Stops a Child-Component by acIdentifier
        /// </summary>
        /// <param name="acIdentifier">ID of Child</param>
        /// <param name="deleteACClassTask">Removes this component from the persisted Application-Tree. 
        /// The component will not started any more while iPlus-Service is restarting next time. 
        /// This should only be done for dynamic instances like Workflow-Classes (Derivation of PWBase)</param>
        /// <returns>True if component was found and stopped successfully</returns>
        bool StopComponent(string acIdentifier, bool deleteACClassTask = false);


        /// <summary>
        /// Stops the passed Component
        /// </summary>
        /// <param name="acComponent">Component to stop</param>
        /// <param name="deleteACClassTask">Removes this component from the persisted Application-Tree. 
        /// The component will not started any more while iPlus-Service is restarting next time. 
        /// This should only be done for dynamic instances like Workflow-Classes (Derivation of PWBase)</param>
        /// <returns>True if passed component was stopped successfully</returns>
        bool StopComponent(IACComponent acComponent, bool deleteACClassTask = false);


        /// <summary>
        /// Stops itself
        /// </summary>
        /// <returns>True if stop was successfull</returns>
        bool Stop();


        /// <summary>
        /// Reloads itself
        /// </summary>
        void Reload();


        /// <summary>
        /// Method, that is called when a ACComponent was taken from the ACComponent-Pool and the members must be initialized.
        /// </summary>
        /// <param name="content">Main object with the component works with. In most cases this is ACCkassTask.</param>
        /// <param name="parentACObject">Reference to the parent component in the application tree</param>
        /// <param name="parameter">Construction parameters</param>
        /// <param name="acIdentifier">Optional: ACIdentifier</param>
        void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "");


        /// <summary>
        /// Returns the absolute ACUrl of this component.
        /// </summary>
        /// <value>Absolute ACUrl</value>
        [ACPropertyInfo(9999)]
        string ACUrl { get; }
        
        #endregion


        #region Methods

        /// <summary>THREAD-SAFE: Returns a new List of all methods (metadata) of this Instance</summary>
        /// <value>List of ACClassMethod</value>
        IEnumerable<ACClassMethod> ACClassMethods { get; }


        /// <summary> Executes a method via passed method-name</summary>
        /// <param name="acMethodName">Name of the method (Only registered methods with ACClassMethodInfo or ACMethod's).</param>
        /// <param name="acParameter">  Parameterlist for method</param>
        /// <returns>NULL, if void-Method else the result as boxed value</returns>
        object ExecuteMethod(string acMethodName, params Object[] acParameter);

        #endregion


        #region Members, Properties and Points

        #region Members
        /// <summary>
        /// NOT THREAD-SAFE! Returns all Members of this Instance (Properties, Points and Childs)
        /// For querying this List use using (ACMonitor.Lock(LockMemberList_20020))
        /// </summary>
        IList<IACMember> ACMemberList { get; }


        /// <summary>
        /// Access the ACMemberList of a Component through a string-index.
        /// It's useful to formulate WPF-Bindings with a indexed Path, e.g: Text="{Binding Path=ContentACComponent.ACMemberDict[CONV].ACMemberDict[AggrNo].Value}"
        /// </summary>
        /// <value>IACMember</value>
        ACMemberIndexer<string, IACMember> ACMemberDict { get; }


        /// <summary>Access the ACMemberList by the ACIdentifier</summary>
        /// <param name="acIdentifier">Name of the member</param>
        /// <returns>IACMember</returns>
        IACMember GetMember(string acIdentifier);


        /// <summary>
        /// Checks whether this IACMember-Instance belongs to this component.
        /// </summary>
        /// <param name="member">IACMember-Instance</param>
        /// <returns><c>true</c> if member belongs to this component; otherwise, <c>false</c>.</returns>
        bool HasMember(IACMember member);
        #endregion


        #region Properties
        /// <summary>
        /// THREAD-SAFE: Returns a new List of all Properties of this Instance
        /// </summary>
        List<IACPropertyBase> ACPropertyList { get; }


        /// <summary>
        /// Finds a property in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the property</param>
        /// <returns>Reference to the property. Returns NULL if property could not be found.</returns>
        IACPropertyBase GetProperty(string acIdentifier);


        /// <summary>
        /// Finds a network-capable property in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the property</param>
        /// <returns>Reference to the property. Returns NULL if property could not be found.</returns>
        IACPropertyNetBase GetPropertyNet(string acIdentifier);


        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged-Event.
        /// </summary>
        /// <param name="name">Name of the property</param>
        void OnPropertyChanged([CallerMemberName] string name = "");
        #endregion


        #region Points
        /// <summary> Returns all members that are from type IACPointBase</summary>
        /// <value> A thread safe list of all members from type IACPointBase</value>
        List<IACPointBase> ACPointList { get; }


        /// <summary> Returns all members that are from type IACPointNetBase</summary>
        /// <value> A thread safe list of all members from type IACPointNetBase</value>
        List<IACPointNetBase> ACPointNetList { get; }

        
        /// <summary>
        /// Finds a the point in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the point</param>
        /// <returns>Point as IACPointBase</returns>
        IACPointBase GetPoint(string acIdentifier);


        /// <summary>
        /// Finds a network-capable point in this instance
        /// </summary>
        /// <param name="acIdentifier">Name of the point</param>
        /// <returns>Point as IACPointNetBase</returns>
        IACPointNetBase GetPointNet(string acIdentifier);
        #endregion

        #endregion


        #region Designs

        /// <summary>Gets a design</summary>
        /// <param name="kindOfDesign">Search-Filter for ACClassDesign.ACKindIndex</param>
        /// <param name="usageOfDesign">Search-Filter for ACClassDesign.ACUsageIndex</param>
        /// <returns>ACClassDesign if found</returns>
        ACClassDesign GetDesign(Global.ACKinds kindOfDesign, Global.ACUsages usageOfDesign);


        /// <summary>Gets a design</summary>
        /// <param name="acIdentifier">ACIdentifier of the Design (ACClassDesign.ACIdentifier)</param>
        /// <returns></returns>
        ACClassDesign GetDesign(string acIdentifier);


        /// <summary>Gets a design</summary>
        /// <param name="kindOfDesign">Search-Filter for ACClassDesign.ACKindIndex</param>
        /// <param name="useDefault">Search-Filter for ACClassDesign.IsDefault</param>
        /// <returns>ACClassDesign if found</returns>
        ACClassDesign GetDesign(Global.ACKinds kindOfDesign, bool useDefault = true);

        #endregion


        #region Reference-Handling

        /// <summary>
        /// A reference-point contains information about objects, that refer to this instance.
        /// In most cases this a Smat-Pointers (ACRef) and WPF-References (Hashcode of a WPF-Object encapsulated in the WPFReferencesToComp-class)
        /// </summary>
        /// <value>A list of references of type IACObject</value>
        IACPointReference<IACObject> ReferencePoint { get; }

        /// <summary>
        /// Called when a reference was added to the ReferencePoint
        /// </summary>
        /// <param name="acObject">The reference as IACObject</param>
        void OnAddReference(IACObject acObject);

        /// <summary>
        /// Called when a reference was removed from the ReferencePoint
        /// </summary>
        /// <param name="acObject">The reference as IACObject</param>
        void OnRemoveReference(IACObject acObject);

        #endregion


        #region Database
        /// <summary>
        /// Returns the database context that this instance works with. If this instance hasn't overridden the context it returns the context of its parent context. If no parent instance has overwritten the database context, the global context is returned.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        IACEntityObjectContext Database { get; }
        #endregion


        #region Validation and Interaction

        /// <summary>
        /// Gets the Control-Mode for a WPF-Control (that implements IVBContent).
        /// This method should evaluates the RightControlMode-Property (User-Rights) and calls CheckPropertyMinMax() to validate the value.
        /// Every WPF-Control (IVBContent) calls this method every time when the bound value (binding via VBContent) changes and the BSOACComponent is this instance.
        /// </summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        Global.ControlModesInfo GetControlModes(IVBContent vbControl);

        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs);

        /// <summary>
        /// Called from a WPF-Control when a relevant interaction-event as occured (e.g. Drag-And-Drop) and the related component should check if this interaction-event should be handled.
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        bool IsEnabledACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs);

        /// <summary>
        /// Retrieves a collection of property names that should be observed when IsEnabledMethod should be called.
        /// Implement this method to specify which properties, when changed, should trigger a reevaluation of the IsEnabledMethod from a IVBContent UI Control via System.Reactive.ReactiveCommand.
        /// </summary>
        /// <param name="acMethodName">Name of the method that should be called from a IVBContent UI Control via System.Reactive.ReactiveCommand</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of strings representing the names of the properties to observe. 
        /// If IsEnabledMethod exsits an the returned collections is empty than the IsEnabledMethod is called via CommandBinding CanExecuteRoutedEventHandler (old standard behaviour) </returns>
        IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName);

        /// <summary>Occurs when ACAction(ACActionArgs actionArgs) is invoked.</summary>
        event ACActionEventHandler ACActionEvent;

        IMessages Messages { get; }
        #endregion


        #region Printing

        /// <summary>Prints the state of the component to a XPSDocument.</summary>
        void PrintSelf();

        /// <summary>Called more times from the report engine (ReportData) during the generation of a XPSDocument for printing.</summary>
        /// <param name="reportEngine">The report engine.</param>
        /// <param name="printingPhase">The printing phase.</param>
        void OnPrintingPhase(object reportEngine, ACPrintingPhase printingPhase);
        
        #endregion


        #region CriticalSection
        /// <summary>
        /// Lock for querying Memberlist
        /// </summary>
        ACMonitorObject LockMemberList_20020 { get; }
        #endregion
    }

    public delegate bool HandleExecuteACMethodStatic(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter);
}
