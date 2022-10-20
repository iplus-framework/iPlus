using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.manager
{
    //[ACClassInfo("VarioSystem", "en{'PWObjectRoot'}de{'PWObjectRoot'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    //public abstract class PWObjectRoot : ACGenericObject, IACObjectDesign //, IACObjectDesignWFManager
    //{
    //    #region c´tors
    //    public PWObjectRoot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
    //        base(acType, content, parentACObject, parameter, acIdentifier)
    //    {
    //    }
    //    #endregion

    //    #region BSO->ACProperty
    //    public abstract IACObjectDesignWF WorkflowModify { get; }

    //    public PWObjectRoot ParentPWObjectRoot
    //    {
    //        get
    //        {
    //            var parentACObject = this.ParentACObject;
    //            while (parentACObject != null)
    //            {
    //                if (parentACObject is PWObjectRoot)
    //                    return parentACObject as PWObjectRoot;
    //                parentACObject = parentACObject.ParentACObject;
    //            }
                 
    //            return null;
    //        }
    //    }

    //    [ACPropertyCurrent(9999, "PWRoot", "en{'Caller'}de{'Aufrufer'}")]
    //    public string PWRootACUrl
    //    {
    //        get
    //        {
    //            if (ParentACObject is PWObjectNode)
    //                return ParentACObject.GetACUrl();
    //            return "";
    //        }
    //    }
    //    #endregion


    //    #region IACObjectDesign
    //    public abstract string GetDesignXAML(IACComponent acComponent, string vbContentDesign);

    //    public abstract string XMLDesign
    //    {
    //        get;
    //        set;
    //    }

    //    public abstract IACComponentDesignManager GetDesignManager(IACComponent acComponent, string vbContentDesign);
    //    #endregion
    //}
}
