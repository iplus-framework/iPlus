// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ProgressInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ProgressInfo
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ProgressInfo'}de{'ProgressInfo'}", Global.ACKinds.TACClass)]
    public class ProgressInfo : INotifyPropertyChanged, IVBProgress
    {
        #region Private properties

        BackgroundWorker backGroundWorker;

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ctor's

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressInfo"/> class.
        /// </summary>
        public ProgressInfo(BackgroundWorker bw)
        {
            TotalProgress = new TaskProgressInfo();
            _IsCancelled = false;
            backGroundWorker = bw;
        }

        #endregion

        #region Progress

        private TaskProgressInfo _TotalProgress;
        [ACPropertyInfo(999, "TotalProgress", "en{'Total Progress'}de{'Total Progress'}")]
        public TaskProgressInfo TotalProgress
        {
            get
            {
                return _TotalProgress;
            }
            set
            {
                _TotalProgress = value;
                OnPropertyChanged("TotalProgress");
            }
        }



        #endregion



        #region Properties

        /// <summary>
        /// The _ is cancelled
        /// </summary>
        private bool _IsCancelled;
        /// <summary>
        /// Is Progress Cancelled
        /// </summary>
        /// <value><c>true</c> if this instance is cancelled; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(11)]
        public bool IsCancelled
        {
            get
            {
                return _IsCancelled;
            }
            set
            {
                _IsCancelled = IsCancelled;
                OnPropertyChanged("IsCancelled");
            }
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return "";
            }
        }

        private bool _ProgressInfoIsIndeterminate;
        [ACPropertyInfo(9999)]
        public bool ProgressInfoIsIndeterminate
        {
            get
            {
                return _ProgressInfoIsIndeterminate;
            }
            set
            {
                if (_ProgressInfoIsIndeterminate != value)
                {
                    _ProgressInfoIsIndeterminate = value;
                    OnPropertyChanged("ProgressInfoIsIndeterminate");
                }
            }
        }

        [ACPropertyInfo(9999)]
        public bool OnlyTotalProgress { get; set; }

        #region SubTask
        private TaskProgressInfo _SelectedSubTask;
        /// <summary>
        /// Selected property for TaskProgressInfo
        /// </summary>
        /// <value>The selected SubTask</value>
        [ACPropertySelected(9999, "SubTask", "en{'TODO: SubTask'}de{'TODO: SubTask'}")]
        public TaskProgressInfo SelectedSubTask
        {
            get
            {
                return _SelectedSubTask;
            }
            set
            {
                if (_SelectedSubTask != value)
                {
                    _SelectedSubTask = value;
                    OnPropertyChanged("SelectedSubTask");
                }
            }
        }


        private List<TaskProgressInfo> _SubTaskList;
        /// <summary>
        /// List property for TaskProgressInfo
        /// </summary>
        /// <value>The SubTask list</value>
        [ACPropertyList(9999, "SubTask")]
        public List<TaskProgressInfo> SubTaskList
        {
            get
            {
                if (_SubTaskList == null)
                    _SubTaskList = new List<TaskProgressInfo>();
                return _SubTaskList;
            }
            set
            {
                _SubTaskList = value;
                OnPropertyChanged("SubTaskList");
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void CloneProgressInfo(ProgressInfo info)
        {
            TotalProgress.CloneProgressInfo(info.TotalProgress);
            _SubTaskList = null;
            lock (info.SubTaskList)
            {
                SubTaskList = info.SubTaskList.ToList();
            }
        }

        #endregion

        #region IVBProgress

        public void Start()
        {
        }

        /// <summary>Adds a sub task for displaying a progress bar in the datagrid.</summary>
        /// <param name="subTaskName">Name of the sub task.</param>
        /// <param name="progressRangeFrom">The progress range from.</param>
        /// <param name="progressRangeTo">The progress range to.</param>
        public void AddSubTask(string subTaskName, int progressRangeFrom, int progressRangeTo)
        {
            if (OnlyTotalProgress)
            {
                TotalProgress.ProgressRangeFrom = progressRangeFrom;
                TotalProgress.ProgressRangeTo = progressRangeTo;
            }
            else
            {
                TaskProgressInfo taskInfo = SubTaskList.FirstOrDefault(x => x.TaskName == subTaskName);
                if (taskInfo == null)
                {
                    taskInfo = new TaskProgressInfo();
                    taskInfo.TaskID = 1;
                    if (SubTaskList.Any())
                        taskInfo.TaskID = SubTaskList.Max(x => x.TaskID) + 1;
                    taskInfo.TaskName = subTaskName;
                    SubTaskList.Add(taskInfo);
                }
                taskInfo.ProgressRangeFrom = progressRangeFrom;
                taskInfo.ProgressRangeTo = progressRangeTo;
            }
        }

        /// <summary>Reports the progress.</summary>
        /// <param name="subTaskName">Name of the sub task. If null, then progress in header will be refreshed.</param>
        /// <param name="progressCurrent">Current progress. If null then current progress will not be changed.</param>
        /// <param name="newProgressText">The new progress text. If null, than current text will bot ne changed.</param>
        /// <exception cref="ArgumentNullException">progressCurrent</exception>
        public void ReportProgress(string subTaskName, int? progressCurrent, string newProgressText = null)
        {
            if (OnlyTotalProgress || String.IsNullOrEmpty(subTaskName))
            {
                if (progressCurrent.HasValue)
                    TotalProgress.ProgressCurrent = progressCurrent.Value;
                if (newProgressText != null)
                    TotalProgress.ProgressText = newProgressText;
            }
            else
            {
                TaskProgressInfo taskInfo = SubTaskList.FirstOrDefault(x => x.TaskName == subTaskName);
                if (taskInfo != null)
                {
                    taskInfo.ProgressText = newProgressText;
                    if (progressCurrent.HasValue)
                        taskInfo.ProgressCurrent = progressCurrent.Value;
                    if (taskInfo.ProgressCurrent >= taskInfo.ProgressRangeTo)
                        taskInfo.EndTime = DateTime.Now;
                }
                SelectedSubTask = taskInfo;
                OnPropertyChanged("SelectedSubTask");
                OnPropertyChanged("SelectedSubTask\\ProgressCurrent");
                CaclulateTotalProgress();
            }

            if (progressCurrent == null)
            {
                progressCurrent = 0;
                // @aagincic: For support all calls - - remove after BGWorkerRefactoring
                //throw new ArgumentNullException(nameof(progressCurrent));
            }

            if (backGroundWorker != null)
                backGroundWorker.ReportProgress(TotalProgress.ProgressPercent, this);
        }

        public void CaclulateTotalProgress()
        {
            if (OnlyTotalProgress || !SubTaskList.Any()) return;
            TotalProgress.ProgressRangeFrom = (int)SubTaskList.Average(x => x.ProgressRangeFrom);
            TotalProgress.ProgressRangeTo = SubTaskList.Sum(x => x.ProgressRangeTo);
            TotalProgress.ProgressCurrent = SubTaskList.Sum(x => x.ProgressCurrent);
        }

        public void Complete()
        {
            TotalProgress = new TaskProgressInfo();
            _IsCancelled = false;
            _SubTaskList = null;
        }

        #endregion

    }
}
