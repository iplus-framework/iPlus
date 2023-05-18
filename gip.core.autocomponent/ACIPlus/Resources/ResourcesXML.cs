using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    public class ResourcesXML : Resources
    {
        #region ctor's

        public ResourcesXML()
            : base()
        {
        }

        #endregion

        #region Dir

        public override ACFSItem Dir(IACEntityObjectContext db, ACFSItemContainer container, string path, bool recursive, bool withFiles = false)
        {

            int pos = path.LastIndexOf("\\");
            string taskName = string.Format(@"ResourcesXML.Dir(""{0}"")", path.Substring(pos + 1));
            if (VBProgress != null)
                VBProgress.AddSubTask(taskName, 0, 1);
            ACFSItem rootACObjectItem = new ACFSItem(this, container, null, path.Substring(pos + 1), ResourceTypeEnum.XML, "\\XML\\" + path);

            ACEntitySerializer serializer = new ACEntitySerializer();
            // serializer.VBProgress = this.VBProgress;
            try
            {
                serializer.DeserializeXMLData(this, rootACObjectItem, path);
                if (MsgObserver != null && serializer.MsgList.Any())
                {
                    serializer.MsgList.ForEach(x => MsgObserver.SendMessage(x));
                }    
            }
            catch (Exception ec)
            {

                if (MsgObserver != null)
                {
                    MsgObserver.SendMessage(new Msg()
                    {
                        MessageLevel = eMsgLevel.Error,
                        Message = string.Format(@"ResourcesXML({0}): Unable to deserialize XML! Exception: {1}", path, ec.Message)
                    });
                }
            }
            if (VBProgress != null)
                VBProgress.ReportProgress(taskName, 1, "XML extract finished!");

            SetupProperties(rootACObjectItem);
            return rootACObjectItem;
        }

        #endregion

    }
}
