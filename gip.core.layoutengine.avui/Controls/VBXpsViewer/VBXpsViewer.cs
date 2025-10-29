using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.IO;
using System.Transactions;
using System.ComponentModel;
using Avalonia.Controls;


namespace gip.core.layoutengine.avui
{

    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBXpsViewer'}de{'VBXpsViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBXpsViewer : VBWebBrowser
    {

        public VBXpsViewer()
        {
        }

        protected bool _Initialized = false;
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.VBContentIsXML = false;
            _Initialized = true;
        }

        public override void LoadFile()
        {
            if (!string.IsNullOrEmpty(ContentFile) && ContentFile.ToUpper().EndsWith(".XPS") && File.Exists(ContentFile))
            {
                base.LoadFile();
                // TODO AVALONIA: XPS ist not supported in Avalonia an windows any more
                // migrate to PDF
                //XpsDocument xpsDoc = new XpsDocument(ContentFile, FileAccess.Read);
                //this.Document = xpsDoc.GetFixedDocumentSequence();
                //xpsDoc.Close();
            }
        }
    }
}
