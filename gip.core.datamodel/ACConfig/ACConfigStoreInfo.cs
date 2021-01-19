// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACConfigStoreInfo.cs" company="gip mbh, Oftersheim, Germany">
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
using System.Data.Objects.DataClasses;
using System.Transactions;
using System.Runtime.Serialization;

namespace gip.core.datamodel
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACConfigStoreInfo}de{'ACConfigStoreInfo'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACConfigStoreInfo
    {
        public ACConfigStoreInfo()
        {
        }

        public ACConfigStoreInfo(EntityKey configStoreEntity, decimal priority)
        {
            _ConfigStoreEntity = configStoreEntity;
            _Priority = priority;
        }

        [DataMember(Name="CSE")]
        private EntityKey _ConfigStoreEntity;

        [IgnoreDataMember]
        [ACPropertyInfo(1, "", "en{'ConfigStoreEntity'}de{'ConfigStoreEntity'}")]
        public EntityKey ConfigStoreEntity
        {
            get
            {
                return _ConfigStoreEntity;
            }
        }

        [DataMember(Name = "P")]
        private decimal _Priority;

        [IgnoreDataMember]
        [ACPropertyInfo(2, "", "en{'Priority'}de{'Priority'}")]
        public decimal Priority
        {
            get
            {
                return _Priority;
            }
        }
    }
}