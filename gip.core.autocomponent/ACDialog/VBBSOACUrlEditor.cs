// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACUrl-Editor'}de{'ACUrl-Editor'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBBSOACUrlEditor : ACBSO
    {
        #region c´tors
        public VBBSOACUrlEditor(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            Result = false;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            //this._CurrentACComponentACClass = null;
            //this._CurrentACSignature = null;
            //this._CurrentParameterValue = null;
            //this._CurrentResultValue = null;
            return base.ACDeInit(deleteACClassTask);
        }

        /// <summary>
        /// true = Änderungen werden übernommen
        /// false = Änderungen werden nicht übernommen
        /// </summary>
        bool Result
        {
            get;
            set;
        }
        #endregion

        #region BSO->ACProperty
        ACClass _CurrentACComponentACClass;
        [ACPropertyCurrent(9999, "ACComponentACClass", "en{'Component'}de{'Component'}")]
        public ACClass CurrentACComponentACClass
        {
            get
            {
                return _CurrentACComponentACClass;
            }
            set
            {
                _CurrentACComponentACClass = value;
                OnPropertyChanged("CurrentACComponentACClass");
            }
        }

        [ACPropertyCurrent(9999, Const.ACUrlPrefix, "en{'Command'}de{'Anweisung'}")]
        public string CurrentACUrl
        {
            get
            {
                return CurrentACSignature.ACIdentifier;
            }
            set
            {
                if (CurrentACSignature.ACIdentifier != value )
                {
                    CurrentACSignature.ACIdentifier = value;
                    ACMethod acMethod = GetACValueList(CurrentACSignature.ACIdentifier);
                    if (acMethod == null)
                    {
                        CurrentACSignature.ParameterValueList = null;
                        CurrentACSignature.ResultValueList = null;
                        IsValidACUrl = false;
                    }
                    else
                    {
                        CurrentACSignature.ParameterValueList = acMethod.ParameterValueList;
                        CurrentACSignature.ResultValueList = acMethod.ResultValueList;
                        IsValidACUrl = true;
                    }
                    OnPropertyChanged("CurrentACUrl");
                    OnPropertyChanged("ParameterValueList");
                    OnPropertyChanged("ResultValueList");
                }
            }
        }

        ACMethod _CurrentACSignature;
        [ACPropertyCurrent(9999, "ACSignature", "en{'Signature'}de{'Signatur'}")]
        public ACMethod CurrentACSignature
        {
            get
            {
                return _CurrentACSignature;
            }
            set
            {
                _CurrentACSignature = value;
            }
            //set
            //{
            //    if (_CurrentACSignature != value)
            //    {
            //        _CurrentACSignature = value;

            //        OnPropertyChanged("ParameterValueList");
            //        OnPropertyChanged("ResultValueList");

            //        if (_CurrentACSignature != null)
            //        {
            //            if (_CurrentACSignature.ResultValueList.Any())
            //            {
            //                CurrentResultValue = _CurrentACSignature.ResultValueList.First();
            //            }
            //            if (_CurrentACSignature.ParameterValueList.Any())
            //            {
            //                CurrentParameterValue = _CurrentACSignature.ParameterValueList.First();
            //            }
            //        }
            //        else
            //        {
            //            CurrentACSignature = null;
            //            CurrentResultValue = null;
            //        }
            //        OnPropertyChanged("CurrentACSignature");
            //    }
            //}
        }

        ACValue _CurrentParameterValue;
        [ACPropertyCurrent(9999, "ACValueList", "en{'Parameter'}de{'Parameter'}")]
        public ACValue CurrentParameterValue
        {
            get
            {
                return _CurrentParameterValue;
            }
            set
            {
                _CurrentParameterValue = value;
                OnPropertyChanged("CurrentParameterValue");
            }
        }

        [ACPropertyList(9999, "ACValueList", "en{'ACUrl'}de{'ACUrl'}")]
        public IEnumerable<ACValue> ParameterValueList
        {
            get
            {
                if (CurrentACSignature == null)
                    return null;
                return CurrentACSignature.ParameterValueList; 
            }
        }

        ACValue _CurrentResultValue;
        [ACPropertyCurrent(9999, "ResultValue", "en{'Result'}de{'Result'}")]
        public ACValue CurrentResultValue
        {
            get
            {
                return _CurrentResultValue;
            }
            set
            {
                _CurrentResultValue = value;
                OnPropertyChanged("CurrentResultValue");
            }
        }

        [ACPropertyList(9999, "ResultValue", "en{'ACUrl'}de{'ACUrl'}")]
        public IEnumerable<ACValue> ResultValueList
        {
            get
            {
                if (CurrentACSignature == null)
                    return null;
                return CurrentACSignature.ResultValueList;
            }
        }

        bool _IsValidACUrl;
        [ACPropertyInfo(9999, "ResultValue", "en{'Result'}de{'Result'}")]
        public bool IsValidACUrl
        {
            get
            {
                return _IsValidACUrl;
            }
            set
            {
                _IsValidACUrl = value;
                OnPropertyChanged("IsValidACUrl");
            }
        }
        #endregion

        #region BSO->ACMethod
        /// <summary>
        /// Zeigt den Dialog zum konfigurieren einer ACUrl mit Parametern an
        /// </summary>
        /// <returns>true wenn Dialog mit "OK" geschlossen wird</returns>
        [ACMethodCommand(Const.ACUrlPrefix, "en{'Config'}de{'Konfiguration'}", 9999)]
        public ACMethod ACUrlEditorDlg(ACClass acComponentACClass, string acUrl, ACValueList acValueList)
        {
            IsValidACUrl = false;
            CurrentACComponentACClass = acComponentACClass;

            CurrentACSignature = new ACMethod();
            CurrentACUrl = acUrl;

            CurrentACSignature = GetACValueList(acUrl);

            CurrentACSignature.ACIdentifier = acUrl;
            if (CurrentACSignature != null)
                CurrentACSignature.ParameterValueList.UpdateValues(acValueList);

            ShowDialog(this, "ACUrlEditorDlg");
            if (Result)
            {
                CurrentACSignature.ACIdentifier = this.CurrentACUrl;
                return CurrentACSignature;
            }
            return null;
        }

        [ACMethodCommand(Const.ACUrlPrefix, Const.Ok, (short)MISort.Okay)]
        public void OK()
        {
            Result = true;
            CloseTopDialog();
            this.ParentACComponent.StopComponent(this);
        }

        [ACMethodCommand(Const.ACUrlPrefix, Const.Cancel, (short)MISort.Cancel)]
        public void Cancel()
        {
            Result = false;
            CloseTopDialog();
            this.ParentACComponent.StopComponent(this);
        }
        #endregion

        #region Hilfsmethoden
        ACMethod GetACValueList(string acUrl)
        {
            return CurrentACComponentACClass.ACUrlACTypeSignature(acUrl); 
        }
        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "OK":
                    OK();
                    return true;
                case "Cancel":
                    Cancel();
                    return true;
                case "ACUrlEditorDlg":
                    result = ACUrlEditorDlg(acParameter[0] as ACClass, acParameter[1] as string, acParameter[2] as ACValueList);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        //protected override bool HandleIsEnabledExecuteACMethod(out bool result, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        //{
        //    return base.HandleIsEnabledExecuteACMethod(out result, acMethodName, acClassMethod, acParameter);
        //}
        #endregion

    }
}
