using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.datamodel.Licensing;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Manager für Umgebungseigenschaften, Icons, Bitmaps, Übersetzung und globale C#-Scripte
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Local Service Objects'}de{'Lokale Dienstobjekte'}", Global.ACKinds.TACLocalServiceObjects, Global.ACStorableTypes.Required, false, false)]
    public class LocalServiceObjects : ApplicationManager
    {
        public const string ClassName = "LocalServiceObjects";
        public const string ClassNameNetService = "Service";

        #region c´tors
        public LocalServiceObjects(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _NoManager = ACVBNoManager.ACRefToServiceInstance(this);
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_NoManager != null)
                ACVBNoManager.DetachACRefFromServiceInstance(this, _NoManager);
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        private ACRef<ACVBNoManager> _NoManager;
        public IACVBNoManager NoManager
        {
            get
            {
                if (_NoManager == null)
                    _NoManager = ACVBNoManager.ACRefToServiceInstance(this);
                return _NoManager != null ? _NoManager.ValueT : null;
            }
        }
    }
}
