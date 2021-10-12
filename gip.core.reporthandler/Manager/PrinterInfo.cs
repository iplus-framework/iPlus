using gip.core.datamodel;
using System;
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
        [ACPropertyInfo(4, "", "en{'Facility ID'}de{'Lagerplatz ID'}")]
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

        public Guid? ACClassConfigID { get; set; }

    }
}
