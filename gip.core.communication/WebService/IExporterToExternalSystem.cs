using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.communication
{
    public interface IExporterToExternalSystem : IACComponent
    {
        string ExportDir
        {
            get;
        }

        bool ExportOff
        {
            get;
        }

        bool ReSendObject(ERPFileItem fileItem);
    }
}
