using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>ACProgramLogPropertyLog is used to connect ProgramLog with PropretyLog.</para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACProgramLogPropertyLog'}de{'ACProgramLogPropertyLog'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "")]
    public partial class ACProgramLogPropertyLog
    {
        /// <summary>
        /// Creates a new object of the ACProgramLogPropertyLog.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="acClass">The ACClass reference.</param>
        /// <returns>The created object.</returns>
        public static ACProgramLogPropertyLog NewACObject(Database db, ACPropertyLog propertyLog)
        {
            ACProgramLogPropertyLog entity = new ACProgramLogPropertyLog();
            entity.ACProgramLogPropertyLogID = Guid.NewGuid();
            if (propertyLog != null)
                entity.ACPropertyLog = propertyLog;
            return entity;
        }
    }
}
