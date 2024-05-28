// ***********************************************************************
// Assembly         : gip.bso.iplus
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-15-2013
// ***********************************************************************
// <copyright file="BSOProgram.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;

namespace gip.bso.iplus
{
    /// <summary>
    /// Class BSOProgram
    /// </summary>
    [ACClassConstructorInfo(
        new object[] 
        { 
            new object[] {"AutoFilter", Global.ParamOption.Optional, typeof(String) },
            new object[] {"PWMethodType", Global.ParamOption.Optional, typeof(String) }
        }
    )]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Programmanagement'}de{'Programmverwaltung'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACProgram.ClassName)]
    public class BSOProgram : ACBSONav
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="BSOProgram"/> class.
        /// </summary>
        /// <param name="acType">Type of the ac.</param>
        /// <param name="content">The content.</param>
        /// <param name="parentACObject">The parent AC object.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        public BSOProgram(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        /// <summary>
        /// ACs the init.
        /// </summary>
        /// <param name="startChildMode">The start child mode.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            string pwMethodType = ParameterValue("PWMethodType") as string;
            if (!string.IsNullOrEmpty(pwMethodType))
            {
                _CurrentWorkflowACClass = WorkflowACClassList.Where(c => c.ACObject is ACClass && c.ACObject.ACIdentifier == pwMethodType).First();
                ControlModeFilter = Global.ControlModes.Collapsed;
            }
            else
            {
                _CurrentWorkflowACClass = WorkflowACClassList.First();
            }
            AccessPrimary.NavSearch(Database.ContextIPlus);
            if (ACProgramList.Any())
            {
                CurrentACProgram = ACProgramList.First();
            }

            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            this._CurrentNewACProgram = null;
            this._CurrentWorkflowACClass = null;
            this._WorkflowACClassList = null;
            var b = base.ACDeInit(deleteACClassTask);
            if (_AccessPrimary != null)
            {
                _AccessPrimary.ACDeInit(false);
                _AccessPrimary = null;
            }
            return b;
        }

        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);
            switch (vbControl.VBContent)
            {
                case "ControlModeFilter":
                    return ControlModeFilter;
            }
            return base.OnGetControlModes(vbControl);
        }

        /// <summary>
        /// The _ control mode filter
        /// </summary>
        Global.ControlModes _ControlModeFilter = Global.ControlModes.Enabled;
        /// <summary>
        /// Gets or sets the control mode filter.
        /// </summary>
        /// <value>The control mode filter.</value>
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlModeFilter
        {
            get
            {
                return _ControlModeFilter;
            }
            set
            {
                _ControlModeFilter = value;
                OnPropertyChanged("ControlModeFilter");
            }
        }
        #endregion

        #region BSO->ACProperty
        #region ACProgram
        public override IAccessNav AccessNav { get { return AccessPrimary; } }
        /// <summary>
        /// The _ access primary
        /// </summary>
        ACAccessNav<ACProgram> _AccessPrimary;
        /// <summary>
        /// Gets the access primary.
        /// </summary>
        /// <value>The access primary.</value>
        [ACPropertyAccessPrimary(590, "ACProgram")]
        public ACAccessNav<ACProgram> AccessPrimary
        {
            get
            {
                if (_AccessPrimary == null && ACType != null)
                {
                    ACQueryDefinition navACQueryDefinition = Root.Queries.CreateQueryByClass(null, PrimaryNavigationquery(), ACType.ACIdentifier);
                    _AccessPrimary = navACQueryDefinition.NewAccessNav<ACProgram>(ACProgram.ClassName, this);
                }
                return _AccessPrimary;
            }
        }

        /// <summary>
        /// Gets or sets the selected AC program.
        /// </summary>
        /// <value>The selected AC program.</value>
        [ACPropertySelected(501, "ACProgram")]
        public ACProgram SelectedACProgram
        {
            get
            {
                return AccessPrimary.Selected;
            }
            set
            {
                AccessPrimary.Selected = value;
                OnPropertyChanged("SelectedACProgram");
            }
        }

        /// <summary>
        /// Gets or sets the current AC program.
        /// </summary>
        /// <value>The current AC program.</value>
        [ACPropertyCurrent(502, "ACProgram")]
        public ACProgram CurrentACProgram
        {
            get
            {
                return AccessPrimary.Current;
            }
            set
            {
                if (AccessPrimary.Current != value)
                {
                    AccessPrimary.Current = value;

                    VBPresenterMethod vbPresenterMethod = this.ACUrlCommand("VBPresenterMethod(CurrentDesign)") as VBPresenterMethod;
                    if (vbPresenterMethod == null)
                    {
                        Messages.Error(this, "This user has no rights for viewing workflows. Assign rights for VBPresenterMethod in the group management!", true);
                        return;
                    }
                    vbPresenterMethod.Load(value);

                    OnPropertyChanged("CurrentACProgram");
                }
            }
        }

        /// <summary>
        /// Gets the AC program list.
        /// </summary>
        /// <value>The AC program list.</value>
        [ACPropertyList(503, "ACProgram")]
        public IEnumerable<ACProgram> ACProgramList
        {
            get
            {
                if (_CurrentWorkflowACClass == null)
                    return null;
                if (_CurrentWorkflowACClass.ACObject is ACClass)
                {
                    ACClass pwACClass = _CurrentWorkflowACClass.ACObject as ACClass;
                    return AccessPrimary.NavList.Where(c => c.WorkflowTypeACClass.ACClassID == pwACClass.ACClassID);
                }
                else
                {
                    return AccessPrimary.NavList;
                }
            }
        }

        /// <summary>
        /// The _ current new AC program
        /// </summary>
        ACProgram _CurrentNewACProgram;
        /// <summary>
        /// Gets or sets the current new AC program.
        /// </summary>
        /// <value>The current new AC program.</value>
        [ACPropertyCurrent(504, "NewACProgram")]
        public ACProgram CurrentNewACProgram
        {
            get
            {
                return _CurrentNewACProgram;
            }
            set
            {
                _CurrentNewACProgram = value;
                OnPropertyChanged("CurrentNewACProgram");
            }
        }
        #endregion

        #region Filter
        /// <summary>
        /// The _ current workflow AC class
        /// </summary>
        ACObjectItem _CurrentWorkflowACClass;
        /// <summary>
        /// Gets or sets the current workflow AC class.
        /// </summary>
        /// <value>The current workflow AC class.</value>
        [ACPropertyCurrent(505, "WorkflowACClass", "en{'Programtype'}de{'Programmart'}")]
        public ACObjectItem CurrentWorkflowACClass
        {
            get
            {
                return _CurrentWorkflowACClass;
            }
            set
            {
                _CurrentWorkflowACClass = value;
                OnPropertyChanged("CurrentWorkflowACClass");
                Search();
                if (ACProgramList != null && ACProgramList.Any())
                {
                    SelectedACProgram = ACProgramList.First();
                    CurrentACProgram = SelectedACProgram;
                }
                else
                {
                    SelectedACProgram = null;
                    CurrentACProgram = null;
                }
            }
        }

        /// <summary>
        /// The _ workflow AC class list
        /// </summary>
        List<ACObjectItem> _WorkflowACClassList;
        /// <summary>
        /// Gets the workflow AC class list.
        /// </summary>
        /// <value>The workflow AC class list.</value>
        [ACPropertyList(506, "WorkflowACClass")]
        public IEnumerable<ACObjectItem> WorkflowACClassList
        {
            get
            {
                if (_WorkflowACClassList == null)
                {
                    _WorkflowACClassList = new List<ACObjectItem>();
                    _WorkflowACClassList.Add(new ACObjectItem(Root.Environment.TranslateText(this, "All Methods")));

                    var query = Database.ContextIPlus.WorkflowTypeMethodACClassList.OrderBy(c => c.SortIndex);
                    foreach (var pwMethod in query)
                    {
                        _WorkflowACClassList.Add(new ACObjectItem(pwMethod, pwMethod.ACCaption));
                    }
                }
                return _WorkflowACClassList;
            }
        }
        #endregion
        #endregion

        #region BSO->ACMethod
        #region ACProgram
        /// <summary>
        /// Saves this instance.
        /// </summary>
        [ACMethodCommand("ACProgram", "en{'Save'}de{'Speichern'}", (short)MISort.Save, false, Global.ACKinds.MSMethodPrePost)]
        public void Save()
        {
            OnSave();
        }

        /// <summary>
        /// Determines whether [is enabled save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledSave()
        {
            return OnIsEnabledSave();
        }

        /// <summary>
        /// Undoes the save.
        /// </summary>
        [ACMethodCommand("ACProgram", "en{'Undo'}de{'Nicht speichern'}", (short)MISort.UndoSave, false, Global.ACKinds.MSMethodPrePost)]
        public void UndoSave()
        {
            OnUndoSave();
        }

        /// <summary>
        /// Determines whether [is enabled undo save].
        /// </summary>
        /// <returns><c>true</c> if [is enabled undo save]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledUndoSave()
        {
            return OnIsEnabledUndoSave();
        }

        /// <summary>
        /// News this instance.
        /// </summary>
        [ACMethodInteraction("ACProgram", "en{'New'}de{'Neu'}", (short)MISort.New, true, "SelectedACProgram", Global.ACKinds.MSMethodPrePost)]
        public void New()
        {
            if (!PreExecute("New")) return;

            //            ACUrlCommand("VBBSOReportDialog!ReportPrintDlg");
            ACUrlCommand("BSOProgramWizard!ShowProgramWizard", null);
            //            CloseTopDialog();

            ACComponent a = ACUrlCommand("BSOProgramWizard") as ACComponent;
            this.StopComponent(a);
            return;

            //CurrentNewACProgram = ACProgram.NewACObject(Database.ContextIPlus, null);
            //if (CurrentWorkflowACClass.ACObject == null)
            //{
            //    foreach (var WorkflowACClass in WorkflowACClassList)
            //    {
            //        if (WorkflowACClass.ACObject != null)
            //        {
            //            CurrentNewACProgram.WorkflowTypeACClass = WorkflowACClass.ACObject as ACClass;
            //            break;
            //        }
            //    }
            //}
            //else
            //{
            //    CurrentNewACProgram.WorkflowTypeACClass = CurrentWorkflowACClass.ACObject as ACClass;
            //}

            //if (ControlModeFilter == Global.ControlModes.Collapsed)
            //{
            //    InitNewACProgram();
            //}
            //else
            //{
            //    ShowDialog(this, "ACProgramNew");
            //}

            //PostExecute("New");
        }

        /// <summary>
        /// Determines whether [is enabled new].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNew()
        {
            return !Database.ContextIPlus.IsChanged;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [ACMethodInteraction("ACProgram", "en{'Delete'}de{'Löschen'}", (short)MISort.Delete, true, "CurrentACProgram", Global.ACKinds.MSMethodPrePost)]
        public void Delete()
        {
            if (Messages.Question(this, "Question00002", Global.MsgResult.Yes, false, CurrentACProgram.ToString()) == Global.MsgResult.Yes)
            {
                if (!PreExecute("Delete")) return;
                Msg msg = CurrentACProgram.DeleteACObject(Database.ContextIPlus, true);
                if (msg != null)
                {
                    Messages.Msg(msg);
                    return;
                }
                ACSaveChanges();

                PostExecute("Delete");
            }
        }

        /// <summary>
        /// Determines whether [is enabled delete].
        /// </summary>
        /// <returns><c>true</c> if [is enabled delete]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledDelete()
        {
            return CurrentACProgram != null;
        }

        /// <summary>
        /// Searches this instance.
        /// </summary>
        [ACMethodCommand("ACProgram", "en{'Search'}de{'Suchen'}", (short)MISort.Search)]
        public void Search()
        {
            AccessPrimary.NavSearch(Database.ContextIPlus);
            OnPropertyChanged("ACProgramList");
        }

        /// <summary>
        /// News the AC program OK.
        /// </summary>
        [ACMethodCommand("NewACProgram", Const.Ok, (short)MISort.Okay)]
        public void NewACProgramOK()
        {
            CloseTopDialog();
            InitNewACProgram();
        }

        /// <summary>
        /// Determines whether [is enabled new AC program OK].
        /// </summary>
        /// <returns><c>true</c> if [is enabled new AC program OK]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledNewACProgramOK()
        {

            if (CurrentNewACProgram == null || string.IsNullOrEmpty(CurrentNewACProgram.ProgramNo) || string.IsNullOrEmpty(CurrentNewACProgram.ProgramName)
                || CurrentNewACProgram.WorkflowTypeACClass == null)
                return false;
            return true;
        }

        /// <summary>
        /// Inits the new AC program.
        /// </summary>
        private void InitNewACProgram()
        {
            Database.ContextIPlus.ACProgram.AddObject(CurrentNewACProgram);

            OnPropertyChanged("ACProgramList");

            CurrentACProgram = CurrentNewACProgram;
        }

        /// <summary>
        /// News the AC program cancel.
        /// </summary>
        [ACMethodCommand("NewACProgram", Const.Cancel, (short)MISort.Cancel)]
        public void NewACProgramCancel()
        {
            CloseTopDialog();
            CurrentNewACProgram.DeleteACObject(Database.ContextIPlus, false);
            CurrentNewACProgram = null;
        }
        #endregion
        #endregion



        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case"Save":
                    Save();
                    return true;
                case"IsEnabledSave":
                    result = IsEnabledSave();
                    return true;
                case"UndoSave":
                    UndoSave();
                    return true;
                case"IsEnabledUndoSave":
                    result = IsEnabledUndoSave();
                    return true;
                case"New":
                    New();
                    return true;
                case"IsEnabledNew":
                    result = IsEnabledNew();
                    return true;
                case"Delete":
                    Delete();
                    return true;
                case"IsEnabledDelete":
                    result = IsEnabledDelete();
                    return true;
                case"Search":
                    Search();
                    return true;
                case"NewACProgramOK":
                    NewACProgramOK();
                    return true;
                case"IsEnabledNewACProgramOK":
                    result = IsEnabledNewACProgramOK();
                    return true;
                case"NewACProgramCancel":
                    NewACProgramCancel();
                    return true;
            }
                return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion


    }
}
