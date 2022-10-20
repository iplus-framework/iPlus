using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;


namespace gip.core.communication
{
    /// <summary>
    /// XML-Documents Importer Base
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'XML Importer Script'}de{'XML Importer Script'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAXMLDocImporterScript : PAXMLDocImporterBase
    {
        #region c´tors
        public PAXMLDocImporterScript(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region overrides
        public override Type TypeOfDeserialization
        {
            get
            {
                try
                {
                    return (Type)ACUrlCommand("!GetTypeOfDeserialization");
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("PAXMLDocImporterScript", "TypeOfDeserialization", msg);

                    return null;
                }
            }
        }

        public override bool IsImporterForXMLDocType(ACEventArgs fileInfoArgs, XmlReader reader)
        {
            try
            {
                return (bool)ACUrlCommand("!IsImporterForXMLDocTypeScript", fileInfoArgs, reader);
            }
            catch (Exception ec)
            {
                string msgEc = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msgEc += " Inner:" + ec.InnerException.Message;

                Messages.LogException("PAXMLDocImporterScript", "IsImporterForXMLDocType", msgEc);

                return false;
            }
        }

        public override bool ProcessObject(object xmlObj, object xmlParseObj)
        {
            try
            {
                return (bool)ACUrlCommand("!ProcessObjectScript", xmlObj, xmlParseObj);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("PAXMLDocImporterScript", "ProcessObject", msg);

                return false;
            }
        }
        #endregion
    }
}