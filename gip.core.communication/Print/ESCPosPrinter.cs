using gip.core.autocomponent;
using gip.core.datamodel;
using System;

namespace gip.core.communication.Print
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ESCPosPrinter'}de{'ESCPosPrinter'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class ESCPosPrinter : ACPrintServerBase
    {

        #region ctor's
        public ESCPosPrinter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
           : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Implementation

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 9999, true, Global.ACKinds.MSMethodPrePost)]
        public override void Print(ACClassDesign acClassDesign, ReportData data, int copies)
        {
            Console.WriteLine("Print");
        }
        #endregion

    }
}
