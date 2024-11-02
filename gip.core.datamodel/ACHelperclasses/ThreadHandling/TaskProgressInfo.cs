// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TaskProgressInfo'}de{'TaskProgressInfo'}", Global.ACKinds.TACClass)]
    public class TaskProgressInfo : INotifyPropertyChanged
    {

        #region ctor's

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressInfo"/> class.
        /// </summary>
        public TaskProgressInfo()
        {
            _ProgressText = "";
            StartTime = DateTime.Now;
            //_progressInfo = progressInfo;
            // _IsCancelled = false;
        }

        #endregion

        #region Identity


        private int taskID;
        [ACPropertyInfo(999, "TaskID", "en{'ID'}de{'ID'}")]
        public int TaskID
        {
            get
            {
                return taskID;
            }
            set
            {
                if(taskID != value)
                {
                    taskID = value;
                    OnPropertyChanged("TaskID");
                }
            }
        }

        private string taskName;
        [ACPropertyInfo(999, "TaskName", "en{'Name'}de{'Name'}")]
        public string TaskName
        {
            get
            {
                return taskName;
            }
            set
            {
                if (taskName != value)
                {
                    taskName = value;
                    OnPropertyChanged("TaskName");
                }
            }
        }

        private DateTime startTime;
        [ACPropertyInfo(999, "StartTime", "en{'Start time'}de{'Startzeit'}")]
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                if (startTime != value)
                {
                    startTime = value;
                    OnPropertyChanged("StartTime");
                }
            }
        }


        private DateTime? endTime;
        [ACPropertyInfo(999, "EndTime", "en{'End time'}de{'Endezeit'}")]
        public DateTime? EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                if (endTime != value)
                {
                    endTime = value;
                    OnPropertyChanged("EndTime");
                }
            }
        }

        #endregion

        #region Progress

        /// <summary>
        /// The _ progress range from
        /// </summary>
        private int _ProgressRangeFrom;
        /// <summary>
        /// From-Value of Range for Progress.
        /// Value can only be positive
        /// </summary>
        /// <value>The progress range from.</value>
        [ACPropertyInfo(999, "ProgressRangeFrom", "en{'Range from'}de{'Von'}")]
        public int ProgressRangeFrom
        {
            get
            {
                return _ProgressRangeFrom;
            }
            set
            {
                if (_ProgressRangeFrom != value)
                {
                    _ProgressRangeFrom = value;
                    OnPropertyChanged("ProgressRangeFrom");
                    OnPropertyChanged("ProgressPercent");
                }
            }
        }

        /// <summary>
        /// The _ Progress range to
        /// </summary>
        private int _ProgressRangeTo;
        /// <summary>
        /// To-Value of Range for Progress.
        /// Value can only be positive and greater than Range-From-Value
        /// </summary>
        /// <value>The progress range to.</value>
        [ACPropertyInfo(999, "ProgressRangeTo", "en{'Range To'}de{'bis'}")]
        public int ProgressRangeTo
        {
            get
            {
                return _ProgressRangeTo;
            }
            set
            {
                if (_ProgressRangeTo != value)
                {
                    _ProgressRangeTo = value;
                    OnPropertyChanged("ProgressRangeTo");
                    OnPropertyChanged("ProgressPercent");
                }
            }
        }

        /// <summary>
        /// The _ progress current
        /// </summary>
        private int _ProgressCurrent;
        /// <summary>
        /// Current value of  Progress.
        /// Value can only be positive
        /// and equal or greater than Range-From-Value
        /// and equal or less than Range-To-Value
        /// </summary>
        /// <value>The progress current.</value>
        [ACPropertyInfo(999, "ProgressCurrent", "en{'Current progress'}de{'laufender Fortschritt'}")]
        public int ProgressCurrent
        {
            get
            {
                return _ProgressCurrent;
            }
            set
            {
                if (_ProgressCurrent != value)
                {
                    _ProgressCurrent = value;
                    OnPropertyChanged("ProgressCurrent");
                    OnPropertyChanged("ProgressPercent");
                }
            }
        }

        /// <summary>
        /// Current value of Progress in percent
        /// </summary>
        /// <value>The progress percent.</value>
        [ACPropertyInfo(999, "ProgressCurrent", "en{'Current progress (%)'}de{'laufender Fortschritt (%)'}")]
        public int ProgressPercent
        {
            get
            {
                int nCurrentFromZero = ProgressCurrent - ProgressRangeFrom;
                int nMaxFromZero = ProgressRangeTo - ProgressRangeFrom;
                double nPercent = (nCurrentFromZero * 100) / (nMaxFromZero == 0 ? 1 : nMaxFromZero);
                return (int)nPercent;
            }
        }

        /// <summary>
        /// The progress text
        /// </summary>
        private string _ProgressText;
        /// <summary>
        /// Current Text for Reporting/decribing the progress state
        /// </summary>
        /// <value>The progress text.</value>
        [ACPropertyInfo(999, "ProgressText", "en{'Message'}de{'Meldung'}")]
        public string ProgressText
        {
            get
            {
                return _ProgressText;
            }
            set
            {
                if (_ProgressText != value)
                {
                    _ProgressText = value;
                    OnPropertyChanged("ProgressText");
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Others

        public void CloneProgressInfo(TaskProgressInfo source)
        {
            if (source == null)
                return;
            ProgressRangeFrom = source._ProgressRangeFrom;
            ProgressRangeTo = source._ProgressRangeTo;

            ProgressCurrent = source._ProgressCurrent;
            ProgressText = source._ProgressText;

            TaskID = source.TaskID;
            TaskName = source.TaskName;
        }

        #endregion

    }
}
