// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using System.Xml;
using System.IO;
using gip.core.autocomponent;
using gip.core.manager;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Workflow dialog'}de{'Workflow-dialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOWorkflowDialog : VBBSOSelectionDependentDialog
    {
        public VBBSOWorkflowDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _CurrentPWRootNode = null;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentPWRootNode = null;
            this._SelectedItem = null;
            return await base.ACDeInit(deleteACClassTask);
        }

        public override string CurrentLayout
        {
            get
            {
                if (CurrentSelection == null)
                    return null;

                string layoutXAML = "";
                if (_CurrentPWRootNode != null)
                {
                    layoutXAML += "<vb:VBDockPanel>";
                    layoutXAML += "<vb:VBDynamic VBContent=\"CurrentLayout\">";
                    layoutXAML += "<vb:VBInstanceInfo Key=\"VBPresenter\" ACIdentifier=\"VBPresenterTask(CurrentDesign)\"  SetAsDataContext=\"True\" SetAsBSOACComponet=\"True\" AutoStart=\"True\"/>";
                    layoutXAML += "</vb:VBDynamic>";
                    layoutXAML += "</vb:VBDockPanel>";
                }
                else
                {
                    layoutXAML += "<vb:VBDockPanel>";
                    layoutXAML += "</vb:VBDockPanel>";
                }
                return layoutXAML;
            }
        }

        private object _SelectedItem;
        [ACPropertyInfo(9999)]
        public object SelectedItem
        {
            get
            {
                return _SelectedItem;
            }

            set
            {
                if (value != null)
                {
                    _SelectedItem = value;
                    OnPropertyChanged("SelectedItem");
                    //OnPropertyChanged("SelectedValueText");
                    //OnPropertyChanged("SelectedValueXMLDoc");
                }
            }
        }

        private IACComponentPWNode _CurrentPWRootNode;

        //[ACPropertyInfo(9999)]
        //public string SelectedValueText
        //{
        //    get
        //    {
        //        if (SelectedItem == null)
        //            return "";
        //        if (SelectedItem is XmlText)
        //        {
        //            XmlText xmlText = SelectedItem as XmlText;
        //            return xmlText.Value;
        //        }
        //        return "";
        //    }
        //}

        //[ACPropertyInfo(9999)]
        //public XmlDocument SelectedValueXMLDoc
        //{
        //    get
        //    {
        //        if (String.IsNullOrEmpty(SelectedValueText))
        //            return null;
        //        if (SelectedValueText.StartsWith("<") && SelectedValueText.EndsWith(">"))
        //        {
        //            try
        //            {
        //                XmlDocument doc = new XmlDocument();
        //                StringReader stringReader = new StringReader(SelectedValueText);
        //                using (XmlReader xmlReader = new XmlTextReader(stringReader))
        //                {
        //                    xmlReader.Read();
        //                    doc.Load(xmlReader);
        //                }
        //                return doc;
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //        return null;
        //    }
        //}

        public override void OnSelectionChanged()
        {
            SelectedItem = CurrentSelection;
            _CurrentPWRootNode = null;
            ACComponent component = CurrentSelection as ACComponent;
            if (component != null)
            {
                if (typeof(PAProcessModule).IsAssignableFrom(component.ACType.ObjectType))
                {
                    string acUrlOfPWNode = null;
                    if (component.IsProxy)
                    {
                        string[] accessArr = (string[]) component.ACUrlCommand("!SemaphoreAccessedFrom");
                        if (accessArr != null && accessArr.Any())
                            acUrlOfPWNode = accessArr[0];
                    }
                    else
                    {
                        IACPointNetServiceObject<ACComponent> semaphore = component.GetPoint("Semaphore") as IACPointNetServiceObject<ACComponent>;
                        if (semaphore != null)
                        {
                            ACPointNetWrapObject<ACComponent> mappedEntry = semaphore.ConnectionList.FirstOrDefault();
                            if (mappedEntry != null)
                                acUrlOfPWNode = mappedEntry.ACUrl;
                        }
                    }
                    if (!String.IsNullOrEmpty(acUrlOfPWNode))
                    {
                        IACComponentPWNode pwNode = component.ACUrlCommand(acUrlOfPWNode) as IACComponentPWNode;
                        if (pwNode != null)
                        {
                            if (pwNode.ParentRootWFNode != null)
                            {
                                VBPresenterTask vbPresenterTask = this.ACUrlCommand("VBPresenterTask(CurrentDesign)") as VBPresenterTask;
                                if (vbPresenterTask != null)
                                {
                                    vbPresenterTask.Load(pwNode.ParentRootWFNode);
                                    _CurrentPWRootNode = pwNode.ParentRootWFNode;
                                }
                            }
                        }
                    }
                }
            }
            OnPropertyChanged("CurrentLayout");
        }
    }
}
