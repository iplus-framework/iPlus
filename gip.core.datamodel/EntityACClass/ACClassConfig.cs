// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-16-2013
// ***********************************************************************
// <copyright file="ACClassConfig.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>ACClassConfig is the Configuration-Table for ACClass. It implements the interface IACConfig.</summary>
    /// <seealso cref="gip.core.datamodel.IACConfig">Interface that enables to read/write config values.</seealso>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Class configuration'}de{'Klassen-Konfiguration'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.PN_PreConfigACUrl, "en{'Parent WF URL'}de{'WF Eltern-URL'}", "", "", true)]
    [ACPropertyEntity(2, Const.PN_LocalConfigACUrl, "en{'Property URL'}de{'Eigenschafts-URL'}", "", "", true)]
    [ACPropertyEntity(3, "XMLValue", "en{'Value'}de{'Wert'}")]
    [ACPropertyEntity(4, "ValueTypeACClass", "en{'Datatype'}de{'Datentyp'}", Const.ContextDatabaseIPlus + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix, "", true)]
    [ACPropertyEntity(5, "Expression", "en{'Expression'}de{'Ausdruck'}", "", "", true)]
    [ACPropertyEntity(6, "Comment", "en{'Comment'}de{'Bemerkung'}", "", "", true)]
    [ACPropertyEntity(100, Const.PN_KeyACUrl, "en{'Key'}de{'Schlüssel'}", "", "", true)]
    [ACDeleteAction("ACClassConfig_ParentACClassConfig", Global.DeleteAction.CascadeManual)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassConfig.ClassName, "en{'Configuration'}de{'Konfiguration'}", typeof(ACClassConfig), ACClassConfig.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassConfig>) })]
    [NotMapped]
    public partial class ACClassConfig : ICloneable, IACConfig
    {
        public const string ClassName = "ACClassConfig";

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(2, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            //get
            //{
            //    if (Value is IACObject)
            //    {
            //        return (Value as IACObject).ACCaption;
            //    }
            //    return LocalConfigACUrl;
            //}
            get
            {
                return Translator.GetTranslation(ACIdentifier, Comment);
            }
            set
            {
                this.OnACCaptionChanging(value);
                // this.ReportPropertyChanging(Const.ACCaptionPrefix);
                Comment = Translator.SetTranslation(Comment, value);
                //this.ReportPropertyChanged(Const.ACCaptionPrefix);
                OnPropertyChanged(Const.ACCaptionPrefix);
                this.OnACCaptionChanged();
            }
        }
        partial void OnACCaptionChanging(string value);
        partial void OnACCaptionChanged();


        /// <summary>
        /// Returns ACClass
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClass</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return ACClass;
            }
        }

        #endregion

        #region New/Delete
        /// <summary>
        /// News the AC object.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <returns>ACClassConfig.</returns>
        public static ACClassConfig NewACObject(Database database, IACObject parentACObject)
        {
            ACClassConfig entity = new ACClassConfig();
            entity.ACClassConfigID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.Database = database;
            entity.ValueTypeACClass = database.GetACType(typeof(string));
            entity.XMLConfig = "";
            entity.BranchNo = 0;
            if (parentACObject is ACClass)
            {
                entity.ACClass = parentACObject as ACClass;
            }
            database.ACClassConfig.Add(entity);
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        public override MsgWithDetails DeleteACObject(IACEntityObjectContext database, bool withCheck, bool softDelete = false)
        {
            MsgWithDetails msg = null;
            if (this.ACClassPropertyRelation != null)
                this.ACClassPropertyRelation.DeleteACObject(database, withCheck);
            database.Remove(this);
            return msg;
        }

        #endregion

        #region IACObjectEntity Members
        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for new unsaved entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckAdded(string user, IACEntityObjectContext context)
        {
            if (ValueTypeACClass == null)
            {
                List<Msg> messages = new List<Msg>();
                messages.Add(new Msg
                {
                    Source = GetACUrl(),
                    ACIdentifier = "ValueTypeACClass",
                    Message = "ValueTypeACClass is null",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ValueTypeACClass"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            if (XMLConfig == null)
                XMLConfig = "";
            base.EntityCheckAdded(user, context);
            return null;
        }

        /// <summary>
        /// Method for validating values and references in this EF-Object.
        /// Is called from Change-Tracking before changes will be saved for changed entity-objects.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="context">Entity-Framework databasecontext</param>
        /// <returns>NULL if sucessful otherwise a Message-List</returns>
        public override IList<Msg> EntityCheckModified(string user, IACEntityObjectContext context)
        {
            base.EntityCheckModified(user, context);
            return null;
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return Const.PN_LocalConfigACUrl;
            }
        }
        #endregion

        #region IACConfig

        [ACPropertyInfo(101, Const.PN_ConfigACUrl, "en{'WF Property URL'}de{'WF Eigenschaft URL'}")]
        [NotMapped]
        public string ConfigACUrl
        {
            get
            {
                return ACUrlHelper.BuildConfigACUrl(this);
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [ACPropertyInfo(9999, "", "en{'Value'}de{'Wert'}")]
        [NotMapped]
        public object Value
        {
            get
            {
                ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, true);
                if (acPropertyExt == null)
                    return null;
                return acPropertyExt.Value;
            }
            set
            {
                ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, true);
                if ((acPropertyExt != null && acPropertyExt.Value != value)
                    || (acPropertyExt == null && value != null))
                    ACProperties.SetACPropertyExtValue(ACProperties.GetOrCreateACPropertyExtByName(Const.Value), value);
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(ValueTypeACClassID))
            {
                if (this.EntityState == EntityState.Added || this.EntityState == EntityState.Modified)
                {
                    ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, false);
                    if (acPropertyExt != null)
                    {
                        if (acPropertyExt.ObjectType != null && ValueTypeACClass != null)
                        {
                            if (acPropertyExt.ObjectType != ValueTypeACClass.ObjectType)
                            {
                                acPropertyExt.Value = null;
                                acPropertyExt.ObjectType = ValueTypeACClass.ObjectType;
                            }
                        }
                    }
                }
            }
            base.OnPropertyChanged(propertyName);
        }

        [ACPropertyInfo(6, "", "en{'Source']de{'Quelle'}")]
        [NotMapped]
        public IACConfigStore ConfigStore
        {
            get
            {
                return ACClass;
            }
        }

        /// <summary>Sets the Metadata (iPlus-Type) of the Value-Property.</summary>
        /// <param name="typeOfValue">Metadata (iPlus-Type) of the Value-Property.</param>
        public void SetValueTypeACClass(ACClass typeOfValue)
        {
            this.ValueTypeACClass = typeOfValue;
        }


        /// <summary>ACClassConfig-Childs</summary>
        /// <value>ACClassConfig-Childs</value>
        [NotMapped]
        public IEnumerable<IACContainerWithItems> Items
        {
            get
            {
                return ACClassConfig_ParentACClassConfig;
            }
        }

        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        [NotMapped]
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return ACClassConfig1_ParentACClassConfig;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        [NotMapped]
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (ACClassConfig1_ParentACClassConfig == null)
                    return this;
                return ACClassConfig1_ParentACClassConfig.RootContainer;
            }
        }


        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            if (child is ACClassConfig)
            {
                ACClassConfig acClassConfig = child as ACClassConfig;
                acClassConfig.ACClassConfig1_ParentACClassConfig = this;
                ACClassConfig_ParentACClassConfig.Add(acClassConfig);
            }
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            if (child is ACClassConfig)
            {
                return ACClassConfig_ParentACClassConfig.Remove(child as ACClassConfig);
            }
            return false;
        }

        [NotMapped]
        public ACClass VBACClass
        {
            get
            {
                return null;
            }
        }

        [NotMapped]
        public Guid? VBiACClassID { get; set; }

        [NotMapped]
        public Guid? ACClassWFID
        {
            get => null;
        }

#endregion

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        [NotMapped]
        public Database Database
        {
            get;
            set;
        }


        /// <summary>
        /// Sets the default value.
        /// </summary>
        public void SetDefaultValue()
        {
            if (this.ValueTypeACClass == null)
                return;
            Type objectType = this.ValueTypeACClass.ObjectType;
            if (objectType == null)
                return;
            switch (objectType.FullName)
            {
                case TypeAnalyser._TypeName_SByte:
                    Value = (SByte)0;
                    break;
                case TypeAnalyser._TypeName_Byte:
                    Value = (Byte)0;
                    break;
                case TypeAnalyser._TypeName_Int16:
                    Value = (Int16)0;
                    break;
                case TypeAnalyser._TypeName_Int32:
                    Value = (Int32)0;
                    break;
                case TypeAnalyser._TypeName_Int64:
                    Value = (Int64)0;
                    break;
                case TypeAnalyser._TypeName_UInt16:
                    Value = System.UInt16.MinValue;
                    break;
                case TypeAnalyser._TypeName_UInt32:
                    Value = System.UInt32.MinValue;
                    break;
                case TypeAnalyser._TypeName_UInt64:
                    Value = System.UInt64.MinValue;
                    break;
                case TypeAnalyser._TypeName_Double:
                    Value = (Double)0;
                    break;
                case TypeAnalyser._TypeName_Single:
                    Value = (Single)0;
                    break;
                case TypeAnalyser._TypeName_String:
                    Value = null;
                    break;
                case TypeAnalyser._TypeName_Decimal:
                    Value = (Decimal)0;
                    break;
                case TypeAnalyser._TypeName_DateTime:
                    Value = DateTime.MinValue;
                    break;
                case TypeAnalyser._TypeName_Guid:
                    Value = Guid.Empty;
                    break;
                case TypeAnalyser._TypeName_Boolean:
                    Value = true;
                    break;
                case TypeAnalyser._TypeName_TimeSpan:
                    Value = System.TimeSpan.MinValue;
                    break;
                default:
                    {
                        object newValue = null;
                        if (!objectType.IsValueType)
                        {
                            try
                            {
                                var constructor = objectType.GetConstructor(Type.EmptyTypes);
                                if (constructor != null)
                                {
                                    newValue = constructor.Invoke(null);
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACClassConfig", "SetDefaultValue", msg);
                                newValue = null;
                            }
                        }
                        if (Value != newValue)
                        {
                            ACPropertyExt acPropertyExt = ACProperties.GetOrCreateACPropertyExtByName(Const.Value, false, true);
                            if (acPropertyExt != null)
                            {
                                if (acPropertyExt.ObjectType != objectType)
                                {
                                    acPropertyExt.ObjectType = objectType;
                                }
                            }

                            Value = newValue;
                        }
                    }
                    break;
            }
        }

#region Clone

        public object Clone()
        {
            ACClassConfig clonedObject = new ACClassConfig();
            clonedObject.ACClassConfigID = this.ACClassConfigID;
            clonedObject.ACClassID = this.ACClassID;
            clonedObject.ACClassPropertyRelationID = this.ACClassPropertyRelationID;
            clonedObject.ParentACClassConfigID = this.ParentACClassConfigID;
            clonedObject.ValueTypeACClassID = this.ValueTypeACClassID;
            clonedObject.KeyACUrl = this.KeyACUrl;
            clonedObject.PreConfigACUrl = this.PreConfigACUrl;
            clonedObject.LocalConfigACUrl = this.LocalConfigACUrl;
            clonedObject.Expression = this.Expression;
            clonedObject.Comment = this.Comment;
            clonedObject.XMLConfig = this.XMLConfig;
            clonedObject.BranchNo = this.BranchNo;
            return clonedObject;
        }

#endregion
    }
}
