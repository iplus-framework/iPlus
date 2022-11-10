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

#if !EFCR
        public ACConfigStoreInfo(EntityKey configStoreEntity, decimal priority)
        {
            _ConfigStoreEntity = configStoreEntity;
            _Priority = priority;
        }
#endif

#if !EFCR
        [DataMember(Name="CSE")]
        private EntityKey _ConfigStoreEntity;
#endif

#if !EFCR
        [IgnoreDataMember]
        [ACPropertyInfo(1, "", "en{'ConfigStoreEntity'}de{'ConfigStoreEntity'}")]
        public EntityKey ConfigStoreEntity
        {
            get
            {
                return _ConfigStoreEntity;
            }
        }
#endif

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

        public override string ToString()
        {
#if !EFCR
            if (ConfigStoreEntity != null)
            {
                string keyValueString = "";
                var firstMember = ConfigStoreEntity.EntityKeyValues.FirstOrDefault();
                if (firstMember != null && firstMember.Value != null)
                    keyValueString = String.Format("{0}:{1}", firstMember.Key, firstMember.Value.ToString());
                return String.Format("CSE:{0},P:{1},V:{2}", ConfigStoreEntity.EntitySetName, Priority, keyValueString);
            }
#endif
            return base.ToString();
        }
    }
}