using gip.core.datamodel;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace gip.core.reporthandler
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PrinterInfo'}de{'PrinterInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PrinterInfo
    {
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Printer ACUrl'}de{'Printer ACUrl'}")]
        public string PrinterACUrl { get; set; }

        [DataMember]
        [ACPropertyInfo(2, "", "en{'Printer name'}de{'Printer Name'}")]
        public string PrinterName { get; set; }

        [DataMember]
        [ACPropertyInfo(3, "", "en{'Facility ID'}de{'Lagerplatz ID'}")]
        public Guid FacilityID { get; set; }

        [DataMember]
        [ACPropertyInfo(4, "", "en{'Facility No.'}de{'Lagerplatz Nr'}")]
        public string FacilityNo { get; set; }

        [DataMember]
        [ACPropertyInfo(5, "", "en{'Machine ACUrl'}de{'Machine ACUrl'}")]
        public string MachineACUrl { get; set; }

        [DataMember]
        [ACPropertyInfo(6, "", "en{'Default'}de{'Standard'}")]
        public bool IsDefault { get; set; }

        [DataMember]
        [ACPropertyInfo(7, "", "en{'Name'}de{'Name'}")]
        public string Name { get; set; }

        [DataMember]
        [ACPropertyInfo(8, "", "en{'VBUserID'}de{'VBUserID'}")]
        public Guid VBUserID { get; set; }

        [IgnoreDataMember]
        private VBUser _User;
        [IgnoreDataMember]
        [ACPropertyInfo(9, "", "en{'VBUserID'}de{'VBUserID'}")]
        public VBUser User
        {
            get => _User;
            set
            {
                _User = value;
            }
        }

        [IgnoreDataMember]
        public Guid? ACClassConfigID { get; set; }

        public override string ToString()
        {
            return PrinterACUrl ?? "" + PrinterName ?? "";
        }

        public void Attach(Database db)
        {
            if (db != null && VBUserID != Guid.Empty)
            {
                User = db.VBUser.FirstOrDefault(c => c.VBUserID == VBUserID);
            }
        }

    }
}
