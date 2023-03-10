using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System;

namespace gip.core.wpfservices
{
    public class WPFServices : IWPFServices
    {
        VBDesignerService _VBDesignerService = new VBDesignerService();
        public IVBDesignerService DesignerService { get { return _VBDesignerService; } }

        VBFlowDocService _VBFlowdocService = new VBFlowDocService();
        public IVBFlowDocService FlowDocService { get { return _VBFlowdocService; } }
    }
}
