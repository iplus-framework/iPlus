// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Selection dependent Dialog'}de{'Auswahlabhängiger Dialog'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {"SelectionManagerACName", Global.ParamOption.Optional, typeof(String)}
        }
    )]
    public abstract class VBBSOSelectionDependentDialog : ACBSO, IACSelectDependentDlg
    {
        #region c'tors
        public VBBSOSelectionDependentDialog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _SelectionManagerACName = ParameterValue("SelectionManagerACName") as string;
        }

        public override bool ACPostInit()
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
            if (_SelectionManager != null)
            {
                _SelectionManager.Detach();
                _SelectionManager.ObjectDetaching -= _SelectionManager_ObjectDetaching;
                _SelectionManager.ObjectAttached -= _SelectionManager_ObjectAttached;
                _SelectionManager = null;
            }

            if (_CurrentSelection != null)
            {
                _CurrentSelection.Detach();
                _CurrentSelection = null;
            }
            //this._Update = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Selection-Manager
        private ACRef<VBBSOSelectionManager> _SelectionManager;
        public VBBSOSelectionManager SelectionManager
        {
            get
            {
                if (_SelectionManager != null)
                    return _SelectionManager.ValueT;
                if (ParentACComponent != null)
                {
                    VBBSOSelectionManager subACComponent = ParentACComponent.GetChildComponent(SelectionManagerACName) as VBBSOSelectionManager;
                    if (subACComponent == null)
                    {
                        subACComponent = ParentACComponent.StartComponent(SelectionManagerACName, null, null) as VBBSOSelectionManager;
                    }
                    if (subACComponent != null)
                    {
                        _SelectionManager = new ACRef<VBBSOSelectionManager>(subACComponent, this);
                        _SelectionManager.ObjectDetaching += new EventHandler(_SelectionManager_ObjectDetaching);
                        _SelectionManager.ObjectAttached += new EventHandler(_SelectionManager_ObjectAttached);
                    }
                }
                if (_SelectionManager == null)
                    return null;
                return _SelectionManager.ValueT;
            }
        }

        public IACComponent VBBSOSelectionManager 
        {
            get
            {
                return SelectionManager;
            }
        }


        void _SelectionManager_ObjectAttached(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        }

        void _SelectionManager_ObjectDetaching(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
        }

        string _SelectionManagerACName = null;
        private string SelectionManagerACName
        {
            get
            {
                if (_SelectionManagerACName != null)
                    return _SelectionManagerACName;
                string acInstance = ACUrlHelper.ExtractInstanceName(this.ACIdentifier);
                if (String.IsNullOrEmpty(acInstance))
                    return "VBBSOSelectionManager";
                else
                    return "VBBSOSelectionManager(" + acInstance + ")";
            }
        }

        public void SelectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowACObjectForSelection")
            {
                CurrentSelection = SelectionManager.ShowACObjectForSelection;
            }
            else if (e.PropertyName == "ShowModuleOfPWGroup")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }
        #endregion


        #region BSO->ACProperty
        [ACPropertyInfo(300, "", "en{'Show referenced Processmodule'}de{'Zeige verbundenes Prozessmodul'}")]
        public bool ShowModuleOfPWGroup
        {
            get
            {
                if (SelectionManager != null)
                    return SelectionManager.ShowModuleOfPWGroup;
                return false;
            }
            set
            {
                if (SelectionManager != null)
                    SelectionManager.ShowModuleOfPWGroup = value;
                OnPropertyChanged("ShowModuleOfPWGroup");
            }
        }

        ACRef<IACObject> _CurrentSelection;
        [ACPropertyInfo(9999)]
        public IACObject CurrentSelection
        {
            get
            {
                if (_CurrentSelection == null)
                    return null;
                return _CurrentSelection.ValueT;
            }
            set
            {
                bool objectSwapped = true;
                if (_CurrentSelection != null)
                {
                    if (_CurrentSelection != value)
                    {
                        _CurrentSelection.Detach();
                    }
                    else
                        objectSwapped = false;
                }
                if (value == null)
                    _CurrentSelection = null;
                else
                    _CurrentSelection = new ACRef<IACObject>(value, this);

                OnPropertyChanged("CurrentSelection");
                if (objectSwapped)
                {
                    OnSelectionChanged();
                    OnPropertyChanged("CurrentLayout");
                }
            }
        }

        private List<IACObject> _VBControlMultiselect;
        [ACPropertyInfo(9999)]
        public List<IACObject> VBControlMultiselect
        {
            get
            {
                return _VBControlMultiselect;
            }
            set
            {
                _VBControlMultiselect = value;
            }
        }

        public virtual void OnSelectionChanged()
        {
        }
        #endregion

        #region BSO->ACMethod

        [ACMethodCommand("Configuration", "en{'Controldialog'}de{'Steuerungsdialog'}", 9999)]
        public virtual void ShowSelectionDialog(IACInteractiveObject acElement)
        {
            IACObject acComponent = acElement.ACContentList.FirstOrDefault() as IACObject;
            if (acComponent == null)
                return;
            ShowDialogForComponent(acComponent);
        }

        public void ShowDialogForComponent(IACObject acComponent, string acClassDesignName="")
        {
            
            if (acComponent == null)
                return;
            if (String.IsNullOrEmpty(acClassDesignName))
                acClassDesignName = "Mainlayout";
            IACObject window = FindGui("", "", "*" + acClassDesignName, this.ACIdentifier);

            if (SelectionManager != null)
            {
                CurrentSelection = SelectionManager.DetermineACObjectToDisplay(acComponent);
                VBControlMultiselect = SelectionManager.VBControlMultiselect.Select(x => x.ContextACObject as IACObject).ToList();
            }
            else
                CurrentSelection = acComponent;

            if (window == null)
            {
                Global.VBDesignContainer containerType = Global.VBDesignContainer.DockableWindow;
                if (VBControlMultiselect != null && VBControlMultiselect.Count() > 1)
                    containerType = Global.VBDesignContainer.ModalDialog;
                    //containerType = Global.VBDesignContainer.DockableWindow;

                ShowWindow(this, acClassDesignName, true,
                    containerType,
                    Global.VBDesignDockState.FloatingWindow,
                    Global.VBDesignDockPosition.Right);
            }
            

        }

        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", (short)MISort.Cancel)]
        public void CloseSelectionDependentDialog()
        {
            IVBDialog window = FindGui("", "", "*Mainlayout", this.ACIdentifier) as IVBDialog;
            if (window != null)
                window.CloseDialog();
        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "ShowSelectionDialog":
                    ShowSelectionDialog(acParameter[0] as IACInteractiveObject);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        //protected override bool HandleIsEnabledExecuteACMethod(out bool result, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        //{
        //    return base.HandleIsEnabledExecuteACMethod(out result, acMethodName, acClassMethod, acParameter);
        //}
        #endregion


        //string _Update = "";
        public abstract string CurrentLayout
        {
            get;
        }

    }
}
