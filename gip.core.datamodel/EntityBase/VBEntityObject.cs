// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : aagincic
// Created          : 05-11-2014
//
// Last Modified By : aagincic
// Last Modified On : 05-11-2014
// ***********************************************************************
// <copyright file="VBEntityObject.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.IdentityModel.Protocols.WsTrust;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Xml.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// VBEntityObject - common iPlus base for entity object - implementing commnon definitions and functionality
    /// (aagincic) TODO:
    /// 1. Implement one by one method
    ///     - remove all with same signature
    ///     - add to different signature methods override keyword
    /// 2. check where is used IACObjectEntityInterface and remove from class (base class is implementator)
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class VBEntityObject : EntityBase, IACObjectEntity, IACEntityProperty
    {

        #region IACObject

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        [DataMember]
        [NotMapped]
        public virtual string ACIdentifier
        {
            get
            {
                return this.ReflectGetACIdentifier();
            }
            set
            {
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        [NotMapped]
        public virtual string ACCaption
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        [NotMapped]
        public virtual IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        [IgnoreDataMember]
        [NotMapped]
        public virtual IEnumerable<IACObject> ACContentList
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
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [ACPropertyInfo(9999)]
        [IgnoreDataMember]
        [NotMapped]
        public virtual IACObject ParentACObject
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public virtual string GetACUrl(IACObject rootACObject = null)
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
        public virtual bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }
        #endregion

        #region IACObjectEntity
        /// <summary>Returns the ACUrl that a reel instance will have in runtime.</summary>
        /// <param name="rootACObject">If null, then a absolute ACUrl will be returned. Else a relative url to the passed object.</param>
        /// <returns>ACUrl as string</returns>
        /// <exception cref="NotImplementedException"></exception>
        [ACMethodInfo("", "", 9999)]
        public virtual string GetACUrlComponent(IACObject rootACObject = null)
        {
            return null;
        }

        /// <summary>
        /// Deletes this entity-object from the database
        /// </summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <param name="withCheck">If set to true, a validation happens before deleting this EF-object. If Validation fails message ís returned.</param>
        /// <param name="softDelete">If set to true a delete-Flag is set in the dabase-table instead of a physical deletion. If  a delete-Flag doesn't exit in the table the record will be deleted.</param>
        /// <returns>If a validation or deletion failed a message is returned. NULL if sucessful.</returns>
        public virtual MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false)
        {
            if (softDelete)
            {
                if (this is IDeleteInfo)
                {
                    ((IDeleteInfo)this).DeleteDate = DateTime.Now;
                    ((IDeleteInfo)this).DeleteName = database.UserName;
                }
                else
                {
                    throw new NotSupportedException(string.Format("Software delete is posible on members they implements IDeleteInfo interface!{0} not implement this interface.", this.GetType().FullName));
                }
            }
            else
            {
                if (withCheck)
                {
                    if (this is IACObject)
                    {
                        MsgWithDetails msg = ((IACObjectEntity)this).IsEnabledDeleteACObject(database);
                        if (msg != null)
                            return msg;
                    }
                }
                database.Remove(this);
            }
            return null;
        }

        /// <summary>Check if entity-object can be deleted from the database</summary>
        /// <param name="database">Entity-Framework databasecontext</param>
        /// <returns>If deletion is not allowed or the validation failed a message is returned. NULL if sucessful.</returns>
        public virtual MsgWithDetails IsEnabledDeleteACObject(IACEntityObjectContext database)
        {
            return this.ReflectIsEnabledDelete();
        }


        /// <summary>
        /// Returns a related EF-Object which is in a Child-Relationship to this.
        /// </summary>
        /// <param name="className">Classname of the Table/EF-Object</param>
        /// <param name="filterValues">Search-Parameters</param>
        /// <returns>A Entity-Object as IACObject</returns>
        public virtual IACObject GetChildEntityObject(string className, params string[] filterValues)
        {
            return null;
        }


        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public virtual IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            SetInsertAndUpdateInfo(user, context);
            return null;
        }

        public void SetInsertAndUpdateInfo(string user, IACEntityObjectContext context)
        {
            this.Context = context;
            if (this.Context == null)
                throw new ArgumentNullException("Context", "Context is null");
            if (this is IInsertInfo)
            {
                if (((IInsertInfo)this).InsertDate == DateTime.MinValue)
                    ((IInsertInfo)this).InsertDate = DateTime.Now;
                if (String.IsNullOrEmpty(((IInsertInfo)this).InsertName))
                    ((IInsertInfo)this).InsertName = user;
            }

            if (this is IUpdateInfo)
            {
                ((IUpdateInfo)this).UpdateDate = DateTime.Now;
                ((IUpdateInfo)this).UpdateName = user;
            }
        }

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public virtual IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context)
        {
            if (this is IUpdateInfo)
            {
                ((IUpdateInfo)this).UpdateDate = DateTime.Now;
                ((IUpdateInfo)this).UpdateName = user;
            }
            return null;
        }
        #endregion

        #region IACEntityProperty
        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.Object.</returns>
        [IgnoreDataMember]
        [NotMapped]
        public virtual object this[string property]
        {
            get
            {
                return ACProperties[property];
            }
            set
            {
                ACProperties[property] = value;
            }
        }

        /// <summary>
        /// Property requiered by IACEntityProperty shema - instance properties should replace this
        /// </summary>
        [DataMember]
        [NotMapped]
        public virtual string XMLConfig
        {
            get
            {
                return null;
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        protected static ConcurrentDictionary<Type, string[]> _PartialPropertyCache = new ConcurrentDictionary<Type, string[]>();

        /// <summary>
        /// Forces to fire INotitfyPropertyChanged from outside
        /// I property-Parameter is null, then all calculated Properties in Partial-Class are notifying
        /// </summary>
        /// <param name="property"></param>
        public virtual void OnEntityPropertyChanged(string property)
        {
            if (property == null)
            {
                string[] propertyNames;
                var thisType = this.GetType();
                if (!_PartialPropertyCache.TryGetValue(this.GetType(), out propertyNames))
                {
                    var typeACPropInfo = typeof(ACPropertyBase);

                    propertyNames = this.GetType().GetProperties()
                                  .Where(c => c.CustomAttributes.Any()
                                    && c.CustomAttributes.Where(d => typeACPropInfo.IsAssignableFrom(d.AttributeType)).Any())
                                  .Select(c => c.Name)
                                  .ToArray();
                    _PartialPropertyCache.TryAdd(thisType, propertyNames);
                }
                foreach (var propName in propertyNames)
                {
                    OnPropertyChanged(propName);
                }
            }
            else if(!string.IsNullOrEmpty(property))
                OnPropertyChanged(property);
        }

        // ********************************* Processing point *************************** //
        [IgnoreDataMember]
        [NotMapped]
        public virtual ACPropertyManager ACProperties
        {
            get
            {
                if (_ACPropertyManager == null)
                {
                    _ACPropertyManager = new ACPropertyManager(this, ACType as gip.core.datamodel.ACClass);
                }
                return _ACPropertyManager;
            }
        }

        #endregion

        #region Documentation and BSO
        //public const string C_DocumentationList_KeyACUrl = ".\\ACClassProperty(DocumentationList)";
        //[ACPropertyPointConfig(9999, "", typeof(ACComposition), "en{'Documentation'}de{'Dokumentation'}")]
        //public IEnumerable<IACConfig> DocumentationList
        //{
        //    get
        //    {
        //        return GetConfigForConfigPoint(C_DocumentationList_KeyACUrl);
        //    }
        //}

        [ACPropertyPointConfig(9999, "", typeof(ACComposition), "en{'Bussinessobject'}de{'Bussinessobjekt'}")]
        [NotMapped]
        public IEnumerable<IACConfig> BussinessobjectList
        {
            get
            {
                return this.GetConfigByKeyACUrl(Const.KeyACUrl_BusinessobjectList);
            }
        }
        #endregion

        #region private members
        [IgnoreDataMember]
        [NotMapped]
        ACPropertyManager _ACPropertyManager = null;

        IACEntityObjectContext _context;
        EntityKey _DetachedKey = null;

        [NotMapped]
        [XmlIgnore]
        public IACEntityObjectContext Context
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
            }
        }
        #endregion end private members

        #region Methods

        /// <summary>
        /// Refreshes the VBEntityObject if not in modified state. Else it leaves it untouched.
        /// </summary>
        public void AutoRefresh()
        {
            if (this.Context != null)
                this.Context.AutoRefresh(this);
        }

        public void AutoRefresh(IACEntityObjectContext context)
        {
            context.AutoRefresh(this);
        }

        public void Refresh()
        {
            if (this.Context != null)
                this.Context.AutoRefresh(this);
        }

        public void Refresh(IACEntityObjectContext context)
        {
            if (context != null)
                context.AutoRefresh(this);
        }

        internal void OnDetached()
        {
            _DetachedKey = EntityKey;
            _context = null;
        }

        #endregion

        [NotMapped]
        public EntityState EntityState
        {
            get
            {
                if (_context == null)
                    return EntityState.Detached;
                else
                {
                    var entry = _context.Entry(this);
                    if (entry == null)
                        return EntityState.Detached;
                    else
                        return entry.State;
                }
            }
        }

        public virtual void OnObjectMaterialized(IACEntityObjectContext context)
        {
            _DetachedKey = null;
            Context = context;
        }

        [NotMapped]
        public EntityKey EntityKey
        {
            get
            {
                if (_context == null)
                    return _DetachedKey;
                if (_DetachedKey != null)
                    return _DetachedKey;
                var entry = _context.Entry(this);
                IKey key = entry.Metadata.FindPrimaryKey();
                _DetachedKey = new EntityKey(this.GetType().AssemblyQualifiedName, 
                                               key.Properties.Select(c => new KeyValuePair<string, object>(c.Name, entry.Property(c.Name).CurrentValue)));
                return _DetachedKey;
            }
        }
    }
}



/*
    Some note:
 * =========================
    * Class list they uses setter for ACCaption:
        ACClass
        ACClassDesign
        ACClassMessage
        ACClassMethod
        ACClassProperty
        ACClassText
        VBDesign
        VBDynamic
        VBDynamicContent


    ** Common implementation of ACType - maybe performance issue?? (now is removed):
        private static ACClass _ReflectedACType = null;
        public IACType ACType
        {
            get
            {
                if (_ReflectedACType == null)
                    _ReflectedACType = this.ReflectACType() as ACClass;
                return _ReflectedACType;
            }
        }
 * 
*/