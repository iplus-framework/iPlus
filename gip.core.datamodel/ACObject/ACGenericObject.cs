// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACGenericObject.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACGenericObject
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACGenericObject'}de{'ACGenericObject'}", Global.ACKinds.TACObject, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACIdentifier
    // 2 ACCaption
    // 3 ACObjectChilds
    public class ACGenericObject : IACObjectWithInit, IACObject
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACGenericObject"/> class.
        /// </summary>
        public ACGenericObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACGenericObject"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public ACGenericObject(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
        {
            _ACType = acType;
            _ParentACObject = parentACObject;
            _ACIdentifier = acIdentifier;
            if (string.IsNullOrEmpty(_ACIdentifier))
            {
                _ACIdentifier = TypeACClass.ACIdentifier;
            }
            _Content = content;
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="acNameIdentifier">The ac name identifier.</param>
        /// <param name="acCaptionTranslation">The ac caption translation.</param>
        /// <param name="value">The value.</param>
        public void AddProperty(Type type, string acNameIdentifier, string acCaptionTranslation, object value = null)
        {
        }
        #endregion

        #region IACObjectWithInit Member
        /// <summary>
        /// The ACInit method is called directly after construction. 
        /// You can also overwrite the ACInit method. 
        /// When booting or dynamically reloading ACComponent trees, such as loading a workflow, the trees pass through the "Depth-First" + "Pre-Order" algorithm. 
        /// This means that the generation of child ACComponents is always carried out in depth first and then the next ACComponent at the same recursion depth.<para />
        /// The algorithm is executed in the ACInit method of the ACComponent class. 
        /// Therefore, you must always make the base call. 
        /// This means that as soon as you execute your initialization logic after the basic call, you know that the child components were created and that they are in initialization state.
        /// </summary>
        /// <param name="startChildMode">The persisted start mode from database</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        virtual public bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            using (ACMonitor.Lock(TypeACClass.Database.QueryLock_1X000))
            {
                foreach (var acClass in TypeACClass.Childs.Where(c => c.ACStartType > Global.ACStartTypes.None && c.ACStartType <= startChildMode))
                {
                    var child = ACActivator.CreateInstance(acClass, acClass, this, null, startChildMode) as IACObjectWithInit;
                    if (child is ACGenericObject)
                    {
                        ((ACGenericObject)child)._ParentACObject = this;
                        child.ACInit(startChildMode);
                        AddChild(child);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// After all new instances have been initialized by the pre-initialization phase, the post-initialization phase starts.
        /// After passing the same Depth-First + Post-Order algorithm, the ACPostInit method is called.
        /// The parent element is passed if there are no more child elements at the same recursion depth, starting with the lowest recursion depth.<para />
        /// You can also overwrite the ACPostInit method to program the remaining initialization logic. 
        /// If these are dependent on all instances having to exist. 
        /// In the Base-ACPostInit method of the ACComponent, the remaining property bindings are performed so that the target properties then have the value of the bound source property. 
        /// Therefore, you must always execute the base call when overwriting this method.
        /// </summary>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        virtual public bool ACPostInit()
        {
            return true;
        }

        /// <summary>
        /// The termination of an ACComponent instance is initiated by the StopComponent() method.
        /// The ACPreDeInit method informs the affected child instances in advance that deinitialization is taking place.
        /// The ACPreDeInit is called after the "Depth-First" + "Pre-Order" algorithm.        
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        virtual public bool ACPreDeInit(bool deleteACClassTask = false)
        {
            return true;
        }

        /// <summary>
        /// After the notification of all affected instances by calling the ACPreDeInit method, the actual deinitialization starts. 
        /// This runs through the instances according to the "Depth-First" + "Post-Order" algorithm and calls the ACDeInit method. 
        /// You should overwrite this so that you can release references or unsubscribe from events, among other things.<para />
        /// Don't forget to make the base call! The basic ACDeInit method of theCComponent executes some functions to clear up the garbage collector. Before calling this function, first:
        /// the state is set to Destructing.
        /// If the instance is "poolable" (IsDisposable == true), it is given the status DisposingToPool instead.
        /// At the end of the deinitialization process:
        /// the state is set to Destructed,
        /// or for a "poolable" instance on DisposedToPool.In this case, the instance is not cleaned up by the.NET garbage collector, but waits in a pool to be reactivated.
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        public virtual async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _ACType = null;
            _ParentACObject = null;
            _ACObjectChilds = null;
            return true;
        }

        /// <summary>
        /// Adds a child to this instance
        /// </summary>
        /// <param name="acObject">The child</param>
        public void AddChild(IACObject acObject)
        {
            _ACObjectChilds.Add(acObject);
        }

        /// <summary>
        /// Removes the child.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        public void RemoveChild(IACObject acObject)
        {
            _ACObjectChilds.Remove(acObject);
        }

        /// <summary>
        /// Removes all childs.
        /// </summary>
        public void RemoveAllChilds()
        {
            _ACObjectChilds.Clear();
        }

        /// <summary>
        /// The _ AC object childs
        /// </summary>
        List<IACObject> _ACObjectChilds = new List<IACObject>();
        /// <summary>
        /// Gets the AC object childs.
        /// </summary>
        /// <value>The AC object childs.</value>
        public IEnumerable<IACObject> ACObjectChilds
        {
            get
            {
                return _ACObjectChilds;
            }
        }
#endregion

#region IACObject Member

        /// <summary>
        /// The _ AC identifier
        /// </summary>
        protected string _ACIdentifier;

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(1, "", "en{'Identifier'}de{'Identifizierer'}")]
        public virtual string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
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
            string acUrl = "";
            if (ParentACObject == null)
            {
                acUrl = "\\";
            }
            else
            {
                if (ParentACObject != rootACObject)
                {
                    acUrl = ParentACObject.GetACUrl(rootACObject);
                    if (!acUrl.EndsWith("\\"))
                        acUrl += "\\";
                }
                acUrl += ACIdentifier;
            }
            return acUrl;
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
        public virtual bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>Translated description</value>
        [ACPropertyInfo(2)]
        public virtual string ACCaption
        {
            get { return TypeACClass.ACCaption; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public virtual IACType ACType
        {
            get
            {
                return this.ReflectACType(false);
            }
        }

        public virtual IACType ACTypeIfGeneric
        {
            get
            {
                return this.ReflectACType(true);
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
        public virtual IEnumerable<IACObject> ACContentList
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
        public virtual object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public virtual bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Callbackmethod if a Exeption occured during ACInit- and ACPostInit-Phase
        /// </summary>
        /// <param name="reason">Exception</param>
        public virtual void OnInitFailed(Exception reason)
        {
        }

        /// <summary>
        /// The _ parent AC object
        /// </summary>
        public IACObject _ParentACObject = null;
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return _ParentACObject; }
        }

        /// <summary>
        /// The _ AC type
        /// </summary>
        protected ACClass _ACType;
        /// <summary>
        /// Gets the type AC class.
        /// </summary>
        /// <value>The type AC class.</value>
        [ACPropertyInfo(9999, "TypeACClass", "en{'Classinfo'}de{'Klasseninfo'}")]
        public ACClass TypeACClass
        {
            get
            {
                return _ACType;
            }
        }

        protected IACObject _Content = null;
        public IACObject Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the INotifyPropertyChanged.PropertyChanged-Event.
        /// </summary>
        /// <param name="name">Name of the property</param>
        [ACMethodInfo("ACComponent", "en{'PropertyChanged'}de{'PropertyChanged'}", 9999)]
        public virtual void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public virtual IACComponent ParentACComponent => ParentACObject as IACComponent;

        public ACRef<IACComponent> ACRef => new ACRef<IACComponent>(this);

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


        public void OnMemberChanged(EventArgs e = null)
        {
        }

        public void RecycleMemberAndAttachTo(IACComponent recycledComponent)
        {
        }
        #endregion
    }
}
