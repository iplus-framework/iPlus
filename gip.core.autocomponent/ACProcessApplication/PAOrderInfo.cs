using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Data;

namespace gip.core.autocomponent
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAOrderInfo'}de{'PAOrderInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAOrderInfo
    {
        #region c'tors
        public PAOrderInfo()
        {
        }
        #endregion

        #region Properties


        [DataMember]
        private List<PAOrderInfoEntry> _Entities;

        [IgnoreDataMember]
        public List<PAOrderInfoEntry> Entities
        {
            get
            {
                if (_Entities == null)
                    _Entities = new List<PAOrderInfoEntry>();

                return _Entities;
            }
        }

        [IgnoreDataMember]
        public VBDialogResult DialogResult { get; set; }

        [IgnoreDataMember]
        public short DialogSelectInfo { get; set; }

        #endregion

        #region Methods

        public void Add(string entityName, Guid entityID)
        {
            if (!this.Entities.Where(c => c.EntityID == entityID).Any())
            {
                this.Entities.Add(new PAOrderInfoEntry(entityName, entityID));
            }
        }

        public void Append(PAOrderInfo info)
        {
            if (info == null)
                return;
            foreach (var key in info.Entities)
            {
                if (!this.Entities.Where(c => c.EntityID == key.EntityID).Any())
                {
                    this.Entities.Add(key);
                }
            }
        }

        public PAOrderInfo Clone()
        {
            PAOrderInfo orderInfo = new PAOrderInfo();
            foreach (PAOrderInfoEntry entry in Entities)
            {
                orderInfo.Entities.Add(entry.Clone());
            }
            return orderInfo;
        }

        #endregion

        public override string ToString()
        {
            if (Entities == null || !Entities.Any()) return "";
            return string.Format(@"[{0}]", string.Join(",", Entities.Select(c => c.EntityName)));
        }
    }

    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAOrderInfoEntry'}de{'PAOrderInfoEntry'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PAOrderInfoEntry
    {
        public PAOrderInfoEntry()
        {
        }

        public PAOrderInfoEntry(string entityName, Guid entityID)
        {
            EntityName = entityName;
            EntityID = entityID;
        }

        [DataMember]
        public String EntityName { get; set; }

        [DataMember]
        public Guid EntityID { get; set; }

        public override string ToString()
        {
            return string.Format(@"{0} [{1}]", EntityName, EntityID);
        }

        public ACClass EntityACType
        {
            get
            {
                return Database.GlobalDatabase.GetACType(EntityName);
            }
        }

        public PAOrderInfoEntry Clone()
        {
            return new PAOrderInfoEntry(this.EntityName, this.EntityID);
        }
    }
}
