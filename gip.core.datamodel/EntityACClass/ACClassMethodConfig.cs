using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel
{
    /// <summary>ACClassConfig is the Configuration-Table for ACClassMethod. It implements the interface IACConfig.</summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACClassMethod.config'}de{'ACClassMethod.config'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true)]
    [ACPropertyEntity(1, Const.PN_PreConfigACUrl, "en{'Parent WF URL'}de{'WF Eltern-URL'}","", "", true)]
    [ACPropertyEntity(2, Const.PN_LocalConfigACUrl, "en{'Property URL'}de{'Eigenschafts-URL'}","", "", true)]
    [ACPropertyEntity(4, "Expression", "en{'Expression'}de{'Ausdruck'}","", "", true)]
    [ACPropertyEntity(5, "Comment", "en{'Comment'}de{'Bemerkung'}","", "", true)]
    [ACPropertyEntity(100, Const.PN_KeyACUrl, "en{'Key'}de{'Schlüssel'}","", "", true)]
    [ACPropertyEntity(496, Const.EntityInsertDate, Const.EntityTransInsertDate)]
    [ACPropertyEntity(497, Const.EntityInsertName, Const.EntityTransInsertName)]
    [ACPropertyEntity(498, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACPropertyEntity(499, Const.EntityUpdateName, Const.EntityTransUpdateName)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACClassMethodConfig.ClassName, "en{'MaterialWFACClassMethod.config'}de{'MaterialWFACClassMethod.config'}", typeof(ACClassMethodConfig), ACClassMethodConfig.ClassName, Const.PN_LocalConfigACUrl, Const.PN_LocalConfigACUrl)]
    [ACSerializeableInfo(new Type[] { typeof(ACRef<ACClassMethodConfig>) })]
    public partial class ACClassMethodConfig : IACConfig
    {
        public const string ClassName = "ACClassMethodConfig";

        #region New/Delete
        /// <summary>
        /// Handling von Sequencenummer wird automatisch bei der Anlage durchgeführt
        /// </summary>
        public static ACClassMethodConfig NewACObject(Database database, IACObject parentACObject)
        {
            ACClassMethodConfig entity = new ACClassMethodConfig();
            entity.ACClassMethodConfigID = Guid.NewGuid();
            entity.DefaultValuesACObject();
            entity.XMLConfig = "";
            if (parentACObject is ACClassMethod)
            {
                entity.ACClassMethod = parentACObject as ACClassMethod;
            }
            entity.SetInsertAndUpdateInfo(database.UserName, database);
            return entity;
        }

        #endregion

        #region IACUrl Member

        public override string ToString()
        {
            return ACCaption;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999, "", "en{'Description'}de{'Bezeichnung'}")]
        [NotMapped]
        public override string ACCaption
        {
            get
            {
                if (Value is IACObject)
                {
                    return (Value as IACObject).ACCaption;
                }
                return this.LocalConfigACUrl;
            }
        }

        /// <summary>
        /// Returns ACClassMethod
        /// NOT THREAD-SAFE USE QueryLock_1X000!
        /// </summary>
        /// <value>Reference to ACClassMethod</value>
        [ACPropertyInfo(9999)]
        [NotMapped]
        public override IACObject ParentACObject
        {
            get
            {
                return ACClassMethod;
            }
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
                    Message = "ValueTypeACClass",
                    //Message = Database.Root.Environment.TranslateMessage(this, "Error50000", "ValueTypeACClass"), 
                    MessageLevel = eMsgLevel.Error
                });
                return messages;
            }
            base.EntityCheckAdded(user, context);
            return null;
        }

        [NotMapped]
        static public string KeyACIdentifier
        {
            get
            {
                return "ACUrl";
            }
        }
        #endregion

        #region IEntityProperty Members

        bool bRefreshConfig = false;
        protected override void OnPropertyChanging<T>(T newValue, string propertyName, bool afterChange)
        {
            if (propertyName == nameof(XMLConfig))
            {
                string xmlConfig = newValue as string;
                if (afterChange)
                {
                    if (bRefreshConfig)
                        ACProperties.Refresh();
                }
                else
                {
                    bRefreshConfig = false;
                    if (this.EntityState != EntityState.Detached && (!(String.IsNullOrEmpty(xmlConfig) && String.IsNullOrEmpty(XMLConfig)) && xmlConfig != XMLConfig))
                        bRefreshConfig = true;
                }
            }
            base.OnPropertyChanging(newValue, propertyName, afterChange);
        }

        #endregion

        #region Properties

        [ACPropertyInfo(999)]
        [NotMapped]
        public string ParameterACCaption
        {
            get
            {
                ACValue paramValue = null;
                if (!LocalConfigACUrl.Contains("SMStarting") && !LocalConfigACUrl.Contains("Rules"))
                {
                    core.datamodel.ACClassMethod method = ACClassWF.RefPAACClassMethod;
                    if (method != null && method.ACMethod != null)
                    {
                        paramValue = method.ACMethod.ParameterValueList.FirstOrDefault(c => c.ACIdentifier == this.LocalConfigACUrl.Split('\\').Last());
                    }
                }
                else if (LocalConfigACUrl.Contains("SMStarting"))
                {
                    core.datamodel.ACClassMethod method = ACClassWF.PWACClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == "SMStarting");
                    if (method == null)
                    {
                        method = ACClassWF.PWACClass.ACClass1_BasedOnACClass.ACClassMethod_ACClass.FirstOrDefault(c => c.ACIdentifier == "SMStarting");
                    }
                    if (method != null && method.ACMethod != null)
                    {
                        paramValue = method.ACMethod.ParameterValueList.FirstOrDefault(c => c.ACIdentifier == this.LocalConfigACUrl.Split('\\').Last());
                    }
                }
                else if (LocalConfigACUrl.Contains("Rules"))
                {
                    return LocalConfigACUrl.Split('\\').Last();
                }
                if (paramValue != null)
                    return paramValue.ACCaption;
                else
                    return this.LocalConfigACUrl;
            }
        }

        [ACPropertyInfo(999)]
        [NotMapped]
        public string ComplexValue
        {
            get
            {
                if (Value != null)
                    return Value.ToString();

                string returnValue = "";
                ACPropertyExt propExt = ACProperties.GetOrCreateACPropertyExtByName(this.ValueTypeACClass.ACIdentifier, false, true);
                if (propExt != null)
                {
                    if (propExt.Value is RuleValueList)
                    {
                        RuleValueList values = propExt.Value as RuleValueList;
                        if (values != null && values.Items != null)
                        {
                            foreach (var value in values.Items)
                            {
                                foreach (string url in value.ACClassACUrl)
                                {
                                    string newUrl = url;
                                    if (url.Contains(Const.ContextDatabase) && url.Contains(ACProject.ClassName) && url.Contains(ACClass.ClassName))
                                        newUrl = url.Split('\\').Last().Split('(').Last().Split(')').First();
                                    else if (url.Contains(TypeAnalyser._TypeName_Boolean))
                                        newUrl = url.Split('\\').Last();
                                    returnValue += newUrl;
                                    if (value.ACClassACUrl.Count() > value.ACClassACUrl.IndexOf(url) + 1)
                                        returnValue += Environment.NewLine;
                                }
                            }
                        }
                    }
                }
                return returnValue;
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

        [ACPropertyInfo(6, "", "en{'Source']de{'Quelle'}")]
        [NotMapped]
        public IACConfigStore ConfigStore
        {
            get
            {
                if (_ACConfigStoreCache != null) return _ACConfigStoreCache;
                return (IACConfigStore)ACClassMethod;
            }
        }

        /// <summary>Sets the Metadata (iPlus-Type) of the Value-Property.</summary>
        /// <param name="typeOfValue">Metadata (iPlus-Type) of the Value-Property.</param>
        public void SetValueTypeACClass(ACClass typeOfValue)
        {
            this.ValueTypeACClass = typeOfValue;
        }


        [NotMapped]
        public IACConfigStore _ACConfigStoreCache = null;

        /// <summary>ACClassMethodConfig-Childs</summary>
        /// <value>ACClassMethodConfig-Childs</value>
        [NotMapped]
        public IEnumerable<IACContainerWithItems> Items
        {
            get
            {
                return this.ACClassMethodConfig_ParentACClassMethodConfig;
            }
        }

        /// <summary>Gets the parent container.</summary>
        /// <value>The parent container.</value>
        [NotMapped]
        public IACContainerWithItems ParentContainer
        {
            get
            {
                return ACClassMethodConfig1_ParentACClassMethodConfig;
            }
        }

        /// <summary>Gets the root container.</summary>
        /// <value>The root container.</value>
        [NotMapped]
        public IACContainerWithItems RootContainer
        {
            get
            {
                if (ACClassMethodConfig1_ParentACClassMethodConfig == null)
                    return this;
                return ACClassMethodConfig1_ParentACClassMethodConfig.RootContainer;
            }
        }

        /// <summary>Adds the specified child-container</summary>
        /// <param name="child">The child-container</param>
        public void Add(IACContainerWithItems child)
        {
            if (child is ACClassMethodConfig)
            {
                ACClassMethodConfig ACClassMethodConfig = child as ACClassMethodConfig;
                ACClassMethodConfig.ACClassMethodConfig1_ParentACClassMethodConfig = this;
                ACClassMethodConfig_ParentACClassMethodConfig.Add(ACClassMethodConfig);
            }
        }

        /// <summary>Removes the specified child-container</summary>
        /// <param name="child">The child-container</param>
        /// <returns>true if removed</returns>
        public bool Remove(IACContainerWithItems child)
        {
            if (child is ACClassMethodConfig)
            {
                return ACClassMethodConfig_ParentACClassMethodConfig.Remove(child as ACClassMethodConfig);
            }
            return false;
        }

        [NotMapped]
        public ACClass VBACClass
        {
            get
            {
                return VBiACClass;
            }
        }

        #endregion

    }
}
