using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

namespace gip.core.layoutengine.avui
{
    public partial class VBDockingManagerOldWPF
    {
        private void PersistDockStateToDesignList()
        {
            PART_gridDocking.PersistStateToVBDesignContent();
            foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            {
                toolWin.PersistStateToVBDesignContent();
            }
            foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            {
                tabbedDoc.PersistStateToVBDesignContent();
            }
        }

        public string SerializeVBDesignList()
        {
            if (VBDesignList == null)
                return "";

            string xaml = "";

            KeyValuePair<string, ACxmlnsInfo> nsThis = Layoutgenerator.GetNamespaceInfo(this);
            if (nsThis.Value == null)
                return "";


            PersistDockStateToDesignList();
            string thisTypeName = this.GetType().Name;

            #region LINQ to XML
            XNamespace xNsThis = nsThis.Value.XMLNameSpace;
            XNamespace xNsX = ACxmlnsResolver.xNamespaceWPF;
            XDocument xDoc = new XDocument();
            XElement xElementRoot = new XElement(xNsThis + thisTypeName);
            foreach (KeyValuePair<string, ACxmlnsInfo> kvp in ACxmlnsResolver.NamespacesDict)
            {
                string key = kvp.Key.Trim();
                if (!String.IsNullOrEmpty(key))
                    xElementRoot.Add(new XAttribute(XNamespace.Xmlns + key, kvp.Value.XMLNameSpace));
            }
            xElementRoot.Add(new XAttribute(xNsX + "Name", this.Name));
            xDoc.Add(xElementRoot);

            foreach (Control uiElement in VBDesignList)
            {
                KeyValuePair<string, ACxmlnsInfo> nsControl = Layoutgenerator.GetNamespaceInfo(uiElement);
                if (nsControl.Value == null)
                    continue;
                XNamespace xNsUI = nsControl.Value.XMLNameSpace;
                XElement xElement = new XElement(xNsUI + uiElement.GetType().Name,
                    new XAttribute(xNsThis + thisTypeName + ".Container", VBDockingManagerOldWPF.GetContainer(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockState", VBDockingManagerOldWPF.GetDockState(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockPosition", VBDockingManagerOldWPF.GetDockPosition(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".RibbonBarVisibility", VBDockingManagerOldWPF.GetRibbonBarVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".IsCloseableBSORoot", VBDockingManagerOldWPF.GetIsCloseableBSORoot(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".CloseButtonVisibility", VBDockingManagerOldWPF.GetCloseButtonVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DisableDockingOnClick", VBDockingManagerOldWPF.GetDisableDockingOnClick(uiElement).ToString())
                    );
                Size size = VBDockingManagerOldWPF.GetWindowSize(uiElement);
                xElement.Add(new XAttribute(xNsThis + thisTypeName + ".WindowSize", String.Format("{0},{1}", size.Width, size.Height)));
                if (uiElement is Control)
                    xElement.Add(new XAttribute(xNsX + "Name", (uiElement as Control).Name));
                if (uiElement is IVBSerialize)
                    (uiElement as IVBSerialize).AddSerializableAttributes(xElement);
                xElementRoot.Add(xElement);
            }
            VBDesign parentDesign = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBDesign)) as VBDesign;
            if (parentDesign != null)
            {
                parentDesign.UpdateDesignOfCurrentUser(xElementRoot, IsBSOManager);
            }
            xaml = xDoc.ToString();
            #endregion

            return xaml;
        }


        public string GetLayoutAsXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("DockingLibrary_Layout"));
            PART_gridDocking.Serialize(doc, doc.DocumentElement);
            return doc.OuterXml;
        }

        public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            PART_gridDocking.Deserialize(this, doc.ChildNodes[0], getContentHandler);

            List<VBDockingPanelBase> addedPanes = new List<VBDockingPanelBase>();
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList)
            {
                VBDockingPanelToolWindow pane = content.VBDockingPanel as VBDockingPanelToolWindow;
                if (pane != null && !addedPanes.Contains(pane))
                {
                    if (pane.State == VBDockingPanelState.AutoHide)
                    {
                        addedPanes.Add(pane);
                        HideDockingPanelToolWindow_AsDockingButton(pane);
                    }
                }
            }

            _currentButton = null;
        }
    }
}
