// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;
using System.IO;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Diagnostic dialog'}de{'Diagnose-dialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSODiagnosticDialog : VBBSOSelectionDependentDialog
    {
        public VBBSODiagnosticDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            if (SelectionManager != null)
                SelectionManager.ShowModuleOfPWGroup = false;
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._SelectedItem = null;
            this._Update = null;
            return base.ACDeInit(deleteACClassTask);
        }

        #region Properties
        string _Update = "";
        public override string CurrentLayout
        {
            get
            {
                if (CurrentSelection == null)
                    return null;

                ACClassDesign acClassDesign = CurrentSelection.ACType.GetDesign(CurrentSelection, Global.ACUsages.DUDiagnostic, Global.ACKinds.DSDesignLayout);
                string layoutXAML = null;
                if (acClassDesign == null)
                {
                    layoutXAML = ACType.GetDesign("Unknown").XAMLDesign + _Update;
                }
                else
                {
                    layoutXAML = acClassDesign.XAMLDesign + _Update;
                }

                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update = _Update == "" ? " " : "";
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
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(SelectedValueText));
                    OnPropertyChanged(nameof(SelectedValueXMLDoc));
                    OnPropertyChanged(nameof(SelectedCmdMethod));
                    OnPropertyChanged(nameof(CmdMethodList));
                }
            }
        }

        [ACPropertyInfo(9999)]
        public string SelectedValueText
        {
            get
            {
                if (SelectedItem == null)
                    return "";
                if (SelectedItem is XmlText)
                {
                    XmlText xmlText = SelectedItem as XmlText;
                    return xmlText.Value;
                }
                return "";
            }
        }

        [ACPropertyInfo(9999)]
        public XmlDocument SelectedValueXMLDoc
        {
            get
            {
                if (String.IsNullOrEmpty(SelectedValueText))
                    return null;
                if (SelectedValueText.StartsWith("<") && SelectedValueText.EndsWith(">"))
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        using (StringReader stringReader = new StringReader(SelectedValueText))
                        using (XmlReader xmlReader = new XmlTextReader(stringReader))
                        {
                            xmlReader.Read();
                            doc.Load(xmlReader);
                        }
                        return doc;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("VBBSODiagnosticDialog", "SelectedValueXMLDoc", msg);
                    }
                }
                return null;
            }
        }


        private string _ACUrlCommandText;
        [ACPropertyInfo(300, "ACUrlCmd", "en{'ACUrlCommand-Shell:'}de{'ACUrlCommand-Shell:'}")]
        public string ACUrlCommandText
        {
            get
            {
                return _ACUrlCommandText;
            }
            set
            {
                if (_ACUrlCommandText != value)
                {
                    _ACUrlCommandText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _ACUrlCommandResult;
        [ACPropertyInfo(301, "ACUrlCmd", "en{'Result value:'}de{'Rückgabewert:'}")]
        public string ACUrlCommandResult
        {
            get
            {
                return _ACUrlCommandResult;
            }
        }

        private ACClassMethod _SelectedCmdMethod;
        [ACPropertySelected(310, "Methods", "en{'Method'}de{'Methode'}")]
        public ACClassMethod SelectedCmdMethod
        {
            get
            {
                return _SelectedCmdMethod;
            }
            set
            {
                bool changed = _SelectedCmdMethod != value;
                _SelectedCmdMethod = value;
                if (changed)
                {
                    if (_SelectedCmdMethod != null)
                        ACUrlCommandText = ACUrlHelper.Delimiter_InvokeMethod + _SelectedCmdMethod.ACIdentifier;
                    else
                        ACUrlCommandText = null;
                }
                OnPropertyChanged();
            }
        }

        [ACPropertyList(311, "Methods", "en{'Methods'}de{'Methoden'}")]
        public IEnumerable<ACClassMethod> CmdMethodList
        {
            get
            {
                ACComponent component = CurrentSelection as ACComponent;
                if (component == null)
                    return null;
                return component.ACClassMethods.Where(c => c.IsCommand || c.IsInteraction).OrderBy(c => c.ACIdentifier).ToList();
            }
        }
        #endregion

        #region Methods
        [ACMethodInfo("ACUrlCmd", "en{'Execute Command'}de{'Befehl ausführen'}", 300)]
        public void CallACUrlCommand()
        {
            if (!IsEnabledCallACUrlCommand()) 
                return;
            ACComponent component = CurrentSelection as ACComponent;
            object result = component.ACUrlCommand(ACUrlCommandText);
            if (result == null)
                _ACUrlCommandResult = "NULL";
            else
            {
                try
                {
                    _ACUrlCommandResult = ACConvert.ObjectToXML(_ACUrlCommandResult, false);
                }
                catch (Exception e)
                {
                    _ACUrlCommandResult = e.Message;
                }
            }
            OnPropertyChanged(nameof(ACUrlCommandResult));
        }

        public bool IsEnabledCallACUrlCommand()
        {
            return !String.IsNullOrEmpty(ACUrlCommandText) && CurrentSelection is ACComponent;
        }

        public override void OnSelectionChanged()
        {
            SelectedItem = CurrentSelection;
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(CallACUrlCommand):
                    CallACUrlCommand();
                    return true;
                case nameof(IsEnabledCallACUrlCommand):
                    result = IsEnabledCallACUrlCommand();
                    return true;
                default:
                    break;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
