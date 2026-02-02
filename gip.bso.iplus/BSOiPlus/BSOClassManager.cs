// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.bso.iplus.VarioBatch
{
    [ACClassInfo(Const.PackName_VarioDevelopment, "en{'Class Manager'}de{'Class manager'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true, Const.QueryPrefix + ACPackage.ClassName)]
    public class BSOClassManager : ACBSO
    {
        #region DI and properties
        #endregion

        #region c'tors

        public BSOClassManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }


        #endregion

        #region Class Clean

        #region Class Clean -> Methods

        [ACMethodInfo("Report", "en{'Report'}de{'Report'}", 401)]
        public void Report()
        {
            using (Database db = new core.datamodel.Database())
            {
                ACClassCleanManager aCClassCleanManager = new ACClassCleanManager(db);
                aCClassCleanManager.CollectData();
                ReportShowData(aCClassCleanManager);
            }

        }

        private void ReportShowData(ACClassCleanManager aCClassCleanManager)
        {
            _MissingClassList = aCClassCleanManager.MissingClasses;
            if (_MissingClassList != null && _MissingClassList.Any())
                SelectedMissingClass = _MissingClassList.FirstOrDefault();
            OnPropertyChanged("MissingClassList");
            if (MakeJsonExport)
            {
                ACCleanManagerJsonReport aCCleanManagerJsonReport = new ACCleanManagerJsonReport(Root.Environment.Datapath);
                aCCleanManagerJsonReport.WriteACClassCleanManagerJsonData(aCClassCleanManager);
            }
        }

        [ACMethodInfo("Clean", "en{'Clean'}de{'Reinemachen'}", 402)]
        public void Clean()
        {
            using (Database db = new core.datamodel.Database())
            {
                ACClassCleanManager aCClassCleanManager = new ACClassCleanManager(db);
                aCClassCleanManager.CollectData();
                ReportShowData(aCClassCleanManager);
                MsgWithDetails msgWithDetails = aCClassCleanManager.RemoveAssembiles(true);
                CleanShowData(aCClassCleanManager);
            }
        }

        private void CleanShowData(ACClassCleanManager aCClassCleanManager)
        {
            _RemoveReportList = aCClassCleanManager.RemoveClasses();
            OnPropertyChanged("RemoveReportList");
            if (MakeJsonExport)
            {
                ACCleanManagerJsonReport aCCleanManagerJsonReport = new ACCleanManagerJsonReport(Root.Environment.Datapath);
                aCCleanManagerJsonReport.WriteRemoveClassesReport(_RemoveReportList);
            }
        }



        #endregion

        #region Class Clean -> Properties

        #region Class Clean -> Properties -> RemoveReport
        private RemoveClassReport _SelectedRemoveReport;
        /// <summary>
        /// Selected property for RemoveClassReport
        /// </summary>
        /// <value>The selected RemoveReport</value>
        [ACPropertySelected(401, "RemoveReport", "en{'TODO: RemoveReport'}de{'TODO: RemoveReport'}")]
        public RemoveClassReport SelectedRemoveReport
        {
            get
            {
                return _SelectedRemoveReport;
            }
            set
            {
                if (_SelectedRemoveReport != value)
                {
                    _SelectedRemoveReport = value;
                    OnPropertyChanged("SelectedRemoveReport");
                }
            }
        }


        private List<RemoveClassReport> _RemoveReportList;
        /// <summary>
        /// List property for RemoveClassReport
        /// </summary>
        /// <value>The RemoveReport list</value>
        [ACPropertyList(402, "RemoveReport")]
        public List<RemoveClassReport> RemoveReportList
        {
            get
            {
                return _RemoveReportList;
            }
        }

        private bool _MakeJsonExport;
        [ACPropertyInfo(403, "MakeJsonExport", "en{'Export json'}de{'Json exportieren'}")]
        public bool MakeJsonExport
        {
            get
            {
                return _MakeJsonExport;
            }
            set
            {
                if (_MakeJsonExport != value)
                {
                    _MakeJsonExport = value;
                    OnPropertyChanged("MakeJsonExport");
                }
            }
        }

        #endregion

        #region MissingClasses
        private ACCleanAssembly _SelectedMissingClass;
        /// <summary>
        /// Selected property for ACCleanAssembly
        /// </summary>
        /// <value>The selected MissingClasses</value>
        [ACPropertySelected(404, "MissingClass", "en{'TODO: MissingClasses'}de{'TODO: MissingClasses'}")]
        public ACCleanAssembly SelectedMissingClass
        {
            get
            {
                return _SelectedMissingClass;
            }
            set
            {
                if (_SelectedMissingClass != value)
                {
                    _SelectedMissingClass = value;
                    if (_SelectedMissingClass == null)
                        _CleanItemList = null;
                    else
                        _CleanItemList = _SelectedMissingClass.CleanItemList;
                    SelectedCleanItem = _CleanItemList != null ? _CleanItemList.FirstOrDefault() : null;
                    OnPropertyChanged("CleanItemList");
                    OnPropertyChanged("SelectedMissingClass");
                }
            }
        }


        private List<ACCleanAssembly> _MissingClassList;
        /// <summary>
        /// List property for ACCleanAssembly
        /// </summary>
        /// <value>The MissingClasses list</value>
        [ACPropertyList(405, "MissingClass")]
        public List<ACCleanAssembly> MissingClassList
        {
            get
            {
                return _MissingClassList;
            }
        }

        #endregion

        #region  MissingClasses -> CleanItem
        private ACCleanItem _SelectedCleanItem;
        /// <summary>
        /// Selected property for ACCleanItem
        /// </summary>
        /// <value>The selected CleanItem</value>
        [ACPropertySelected(406, "CleanItem", "en{'TODO: CleanItem'}de{'TODO: CleanItem'}")]
        public ACCleanItem SelectedCleanItem
        {
            get
            {
                return _SelectedCleanItem;
            }
            set
            {
                if (_SelectedCleanItem != value)
                {
                    _SelectedCleanItem = value;
                    OnPropertyChanged("SelectedCleanItem");
                }
            }
        }


        private List<ACCleanItem> _CleanItemList;
        /// <summary>
        /// List property for ACCleanItem
        /// </summary>
        /// <value>The CleanItem list</value>
        [ACPropertyList(407, "CleanItem")]
        public List<ACCleanItem> CleanItemList
        {
            get
            {
                return _CleanItemList;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Properties -> Messages

        public void AddPAResultObjectToMsgList(object result)
        {
            MsgWithDetails msgWithDetails = result as MsgWithDetails;
            if (msgWithDetails != null)
            {
                MsgList.Add(msgWithDetails);
                OnPropertyChanged("MsgList");
                CurrentMsg = msgWithDetails;
            }
        }

        /// <summary>
        /// The _ current MSG
        /// </summary>
        Msg _CurrentMsg;
        /// <summary>
        /// Gets or sets the current MSG.
        /// </summary>
        /// <value>The current MSG.</value>
        [ACPropertyCurrent(410, "Message", "en{'Message'}de{'Meldung'}")]
        public Msg CurrentMsg
        {
            get
            {
                return _CurrentMsg;
            }
            set
            {
                _CurrentMsg = value;
                OnPropertyChanged("CurrentMsg");
            }
        }

        private ObservableCollection<Msg> msgList;
        /// <summary>
        /// Gets the MSG list.
        /// </summary>
        /// <value>The MSG list.</value>
        [ACPropertyList(411, "Message", "en{'Messagelist'}de{'Meldungsliste'}")]
        public ObservableCollection<Msg> MsgList
        {
            get
            {
                if (msgList == null)
                    msgList = new ObservableCollection<Msg>();
                return msgList;
            }
        }

        #endregion

    }
}
