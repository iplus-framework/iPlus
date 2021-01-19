using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.tool.publish
{
    public enum DownloadVersionState : short
    {
        DownloadFail = 0,
        SameVersionExist = 10,
        DeseralizationFail = 20,
        Ok = 30
    }
}
