// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Der Designmanager dient zum verwalten beliebiger editierbarer Designs. Hierzu 
    /// zählen derzeit Maskenlayouts, Visualisierung und Workflows. Ist um beliebige
    /// weitere Designmanager zu erweitern
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Selection Manager'}de{'Auswahl Manager'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOSelectionManager : ACComponent
    {
        #region c´tors
        public VBBSOSelectionManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            this._SelectedVBControl = null;
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Selection
        IVBContent _SelectedVBControl;
        [ACPropertyInfo(9999)]
        public virtual IVBContent SelectedVBControl
        {
            get
            {
                return _SelectedVBControl;
            }
            set
            {
                _SelectedVBControl = value;

                OnPropertyChanged("SelectedVBControl");
                OnPropertyChanged("CurrentContentACObject");
                OnPropertyChanged("SelectedACObject");
                OnPropertyChanged("ShowACObjectForSelection");
            }
        }

        private List<IVBContent> _VBControlMultiselect;
        [ACPropertyInfo(9999)]
        public virtual List<IVBContent> VBControlMultiselect
        {
            get
            {
                if (_VBControlMultiselect == null)
                    _VBControlMultiselect = new List<IVBContent>();
                return _VBControlMultiselect;
            }
        }

        /// <summary>
        /// Aktuelles ausgewähltes VB-Steuerelement
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACObject CurrentContentACObject
        {
            get
            {
                if (SelectedVBControl == null)
                    return null;
                if (SelectedVBControl.ACContentList == null || !SelectedVBControl.ACContentList.Any())
                    return null;
                return SelectedVBControl.ACContentList.First();
            }
        }

        private bool? _ShowModuleOfPWGroup;
        [ACPropertyInfo(300, "", "en{'Show referenced Processmodule'}de{'Zeige verbundenes Prozessmodul'}")]
        public bool ShowModuleOfPWGroup
        {
            get
            {
                if (!_ShowModuleOfPWGroup.HasValue)
                    return false;
                return _ShowModuleOfPWGroup.Value;
            }
            set
            {
                bool changed = _ShowModuleOfPWGroup != value;
                _ShowModuleOfPWGroup = value;
                OnPropertyChanged("ShowModuleOfPWGroup");
                if (changed)
                {
                    OnPropertyChanged("ShowACObjectForSelection");
                }
            }
        }

        public IACObject SelectedACObject
        {
            get
            {
                if (SelectedVBControl != null)
                    return SelectedVBControl.ContextACObject;
                else
                    return null;
            }
        }

        public IACObject ShowACObjectForSelection
        {
            get
            {
                return DetermineACObjectToDisplay(SelectedACObject);
            }
        }

        public IACObject DetermineACObjectToDisplay(IACObject selectedACObject)
        {
            ACComponent tempComp = selectedACObject as ACComponent;
            if (tempComp != null && tempComp.ACType != null)
            {
                if (!_ShowModuleOfPWGroup.HasValue)
                {
                    if (typeof(PWGroup).IsAssignableFrom(tempComp.ACType.ObjectType))
                    {
                        _ShowModuleOfPWGroup = true;
                        OnPropertyChanged("ShowModuleOfPWGroup");
                    }
                }
                if (_ShowModuleOfPWGroup.HasValue && _ShowModuleOfPWGroup.Value)
                {
                    ACComponent refModule = GetReferencedModuleOfPWGroup(tempComp);
                    if (refModule != null)
                        tempComp = refModule;
                }
                return tempComp;
            }
            else
            {
                _ShowModuleOfPWGroup = null;
                OnPropertyChanged("ShowModuleOfPWGroup");
                return selectedACObject;
            }
        }

        public static ACComponent GetReferencedModuleOfPWGroup(ACComponent pwGroup)
        {
            IACPointNetClientObject<ACComponent> _semaphoreProp = pwGroup.GetPointNet("TrySemaphore") as IACPointNetClientObject<ACComponent>;
            if (_semaphoreProp == null)
                return null;

            if (_semaphoreProp.ConnectionList == null || !_semaphoreProp.ConnectionList.Any())
                return null;

            string acUrlOfComponent = _semaphoreProp.ConnectionList.FirstOrDefault().ACUrl;
            ACComponent paProcessModule = pwGroup.ACUrlCommand(acUrlOfComponent) as ACComponent;
            return paProcessModule;
        }

        /// <summary>Method which ist called from VBDesign, to select a Control
        /// The VBBSOSelectionManager can have more VBDesigns connected,
        /// but only one VBControl can be active</summary>
        /// <param name="clickedVBControl"></param>
        /// <param name="isMultiSelect"></param>
        [ACMethodInfo("", "en{'OnVBControlClicked'}de{'OnVBControlClicked'}", 9999)]
        public void OnVBControlClicked(IVBContent clickedVBControl, bool isMultiSelect = false)
        {
            if (!isMultiSelect)
            {
                VBControlMultiselect.Clear();
            }
            if (!VBControlMultiselect.Contains(clickedVBControl))
                VBControlMultiselect.Add(clickedVBControl);

            SelectedVBControl = clickedVBControl;

            HighlightVBControl(clickedVBControl, isMultiSelect);
        }

        protected void HighlightVBControl(IVBContent vbControlToSelect, bool isMultiSelect)
        {
            BroadcastToVBControls(Const.CmdHighlightVBControl, null, this, vbControlToSelect, isMultiSelect);
        }


        [ACMethodInfo("", "en{'HighlightContentACObject'}de{'HighlightContentACObject'}", 9999)]
        public void HighlightContentACObject(IACObject acObjectToSelect, bool highlightParentIfNotFound = false)
        {
            BroadcastToVBControls(Const.CmdHighlightContentACObject, null, this, acObjectToSelect, highlightParentIfNotFound);
        }

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "OnVBControlClicked":
                    OnVBControlClicked(acParameter[0] as IVBContent, acParameter.Length > 1 ? (bool) acParameter[1] : false);
                    return true;
                case "HighlightContentACObject":
                    HighlightContentACObject(acParameter[0] as IACObject, acParameter.Length > 1 ? (bool)acParameter[1] : false);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        //protected override bool HandleIsEnabledExecuteACMethod(out bool result, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        //{
        //    return base.HandleIsEnabledExecuteACMethod(out result, acMethodName, acClassMethod, acParameter);
        //}
        #endregion

        #endregion
    }
}
