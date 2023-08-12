using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassConstructorInfo(
    new object[] 
        { 
            new object[] {ACProgram.ClassName, Global.ParamOption.Required, typeof(Guid)},
            new object[] {ACProgramLog.ClassName, Global.ParamOption.Optional, typeof(Guid)},
            new object[] {PWProcessFunction.C_InvocationCount, Global.ParamOption.Optional, typeof(int)}
        }
    )]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Program'}de{'Programm'}", Global.ACKinds.TPWMethod, Global.ACStorableTypes.Optional, true, true, "", "PWBSOMethod/ACProgram", 1000)]
    public class PWMethod : PWProcessFunction 
    {
        new public const string PWClassName = Const.ACClassIdentifierOfPWMethod;

        #region c´tors
        public PWMethod(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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
            if (!base.ACDeInit(deleteACClassTask))
                return false;

            return true;
        }
        #endregion

        #region Methods
        #region Planning and Testing
        protected override TimeSpan GetPlannedDuration()
        {
            return base.GetPlannedDuration();
        }

        protected override DateTime GetPlannedStartTime()
        {
            return base.GetPlannedStartTime();
        }
        #endregion
        #endregion
    }
}
