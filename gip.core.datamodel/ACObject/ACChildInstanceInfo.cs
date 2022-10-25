// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACChildInstanceInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Data;
using System.Transactions;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACChildInstanceInfo
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Child-InstanceInfo'}de{'Child-InstanceInfo'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACChildInstanceInfo", "en{'ACChildInstanceInfo'}de{'ACChildInstanceInfo'}", typeof(ACChildInstanceInfo), "ACChildInstanceInfo", Const.ACIdentifierPrefix, Const.ACIdentifierPrefix)]
    public class ACChildInstanceInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACChildInstanceInfo"/> class.
        /// </summary>
        public ACChildInstanceInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACChildInstanceInfo"/> class.
        /// </summary>
        /// <param name="component">The component.</param>
        public ACChildInstanceInfo(IACComponent component)
        {
            //_ACNameInstance = component.ACNameInstance;
            if (component.ParentACComponent != null)
                _ACUrlParent = component.ParentACComponent.GetACUrl();
            _ACIdentifier = component.ACIdentifier;
#if !EFCR
            _ACType = new ACRef<ACClass>(component.ComponentClass, true);
            _Content = new ACRef<IACObject>(component.Content, true);
#endif
        }

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="contextOfACType">Type of the context of AC.</param>
        /// <param name="contextOfContent">Content of the context of.</param>
        public void AttachTo(Database contextOfACType, Database contextOfContent)
        {
#if !EFCR
            if (_ACType != null)
                _ACType.AttachTo(contextOfACType);
            if (_Content != null)
                _Content.AttachTo(contextOfContent);
#endif
            if (Childs.Count > 0)
            {
                foreach (ACChildInstanceInfo child in Childs)
                {
                    child.AttachTo(contextOfACType, contextOfContent);
                }
            }
        }

        /// <summary>
        /// The _ AC identifier
        /// </summary>
        [DataMember]
        private String _ACIdentifier;

        /// <summary>
        /// Gets the AC identifier.
        /// </summary>
        /// <value>The AC identifier.</value>
        [IgnoreDataMember]
        [ACPropertyInfo(1, "", "en{'ACIdentifier'}de{'ACIdentifier'}")]
        public String ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
        }

        /// <summary>
        /// The _ AC URL parent
        /// </summary>
        [DataMember]
        private String _ACUrlParent;

        /// <summary>
        /// Gets the AC URL parent.
        /// </summary>
        /// <value>The AC URL parent.</value>
        [IgnoreDataMember]
        [ACPropertyInfo(2, "", "en{'ACUrlParent'}de{'ACUrlParent'}")]
        public String ACUrlParent
        {
            get
            {
                return _ACUrlParent;
            }
        }


        /// <summary>
        /// The _ AC type
        /// </summary>
#if !EFCR
        [DataMember]
        private ACRef<ACClass> _ACType;

        /// <summary>
        /// Gets the type of the AC.
        /// </summary>
        /// <value>The type of the AC.</value>
        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public ACRef<ACClass> ACType
        {
            get
            {
                return _ACType;
            }
        }
        /// <summary>
        /// The _ content
        /// </summary>
        [DataMember]
        private ACRef<IACObject> _Content;
        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        [IgnoreDataMember]

        [ACPropertyInfo(9999)]
        public ACRef<IACObject> Content
        {
            get
            {
                return _Content;
            }
        }
#endif
        /// <summary>
        /// The _ childs
        /// </summary>
        [DataMember]
        private List<ACChildInstanceInfo> _Childs;

        /// <summary>
        /// Gets the childs.
        /// </summary>
        /// <value>The childs.</value>
        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public List<ACChildInstanceInfo> Childs
        {
            get
            {
                if (_Childs == null)
                    _Childs = new List<ACChildInstanceInfo>();
                return _Childs;
            }
        }

        [DataMember]
        private ACValueWithCaptionList _MValues;

        [IgnoreDataMember]
        [ACPropertyInfo(9999)]
        public ACValueWithCaptionList MemberValues
        {
            get
            {
                return _MValues;
            }
            set
            {
                _MValues = value;
            }
        }

        [ACPropertyInfo(9999, "", "en{'Type'}de{'Type'}")]
        public string ACTypeName
        {
            get
            {
#if !EFCR
                if(ACType != null && ACType.Value != null && ACType.Value is ACClass)
                    return ((ACClass)ACType.Value).ACCaption;
#endif
                return ACIdentifier;
            }
        }
    }


    [DataContract]
    public class ChildInstanceInfoSearchParam
    {
        [DataMember]
        public bool OnlyWorkflows { get; set; }

#if !EFCR
        [DataMember]
        public ACRef<ACClass> TypeOfRoots { get; set; }
#endif

        [DataMember]
        public Guid[] ACRequestIDs { get; set; }

        [DataMember]
        public string ACIdentifier { get; set; }

        [DataMember]
        public Guid[] ACProgramIDs { get; set; }

        [DataMember]
        public string ContainsPropertyName { get; set; }
        
        [DataMember]
        public bool ReturnAsFlatList { get; set; }

        [DataMember]
        public bool BottomUpSearch { get; set; }

        [DataMember]
        public bool WithWorkflows { get; set; }

        [DataMember]
        public bool ReturnLocalProperties { get; set; }
    }
}