using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
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
        [ACPropertyInfo(3, "", "en{'Facility No.'}de{'Lagerplatz Nr.'}")]
        public string FacilityNo { get; set; }

        [DataMember]
        [ACPropertyInfo(4, "", "en{'Machine ACUrl'}de{'Machine ACUrl'}")]
        public string MachineACUrl { get; set; }

        [DataMember]
        [ACPropertyInfo(5, "", "en{'Default'}de{'Standard'}")]
        public bool IsDefault { get; set; }
    }
}
