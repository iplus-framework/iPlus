using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace gip.core.layoutengine.avui
{
    public delegate VBDockingContainerToolWindow GetContentFromTypeString(string type);

    interface IVBDockLayoutSerializable
    {
        void Serialize(XmlDocument doc, XmlNode parentNode);

        void Deserialize(VBDockingManagerOldWPF managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler);
    }
}
