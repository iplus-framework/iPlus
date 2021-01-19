using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PATimeInfo'}de{'PATimeInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PATimeInfo : INotifyPropertyChanged
    {
        #region c'tors
        public PATimeInfo() : this(new ActivityTimeInfo(), new ActivityTimeInfo())
        {
            InitLock();
        }

        public PATimeInfo(ActivityTimeInfo actualTimes, ActivityTimeInfo plannedTimes)
        {
            InitLock();
            lock (Lock)
            {
                _ActualTimes = actualTimes;
                _ActualTimes.PropertyChanged += new PropertyChangedEventHandler(Times_PropertyChanged);
                _PlannedTimes = plannedTimes;
                _PlannedTimes.PropertyChanged += new PropertyChangedEventHandler(Times_PropertyChanged);
            }
        }

        public PATimeInfo(ACProgramLog acProgramLog)
        {
            InitLock();
            ReadFromProgramLog(acProgramLog);
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            InitLock();
        }

        private void InitLock()
        {
            if (_Lock == null)
                _Lock = new object();
        }
        #endregion

        #region Properties
        private object _Lock = new object();
        private object Lock
        {
            get
            {
                if (_Lock == null)
                    InitLock();
                return _Lock;
            }
        }
        [DataMember]
        private ActivityTimeInfo _ActualTimes;

        [IgnoreDataMember]
        public ActivityTimeInfo ActualTimes
        {
            get
            {
                lock (Lock)
                {
                    return _ActualTimes;
                }
            }
            private set
            {
                lock (Lock)
                {
                    _ActualTimes = value;
                }
            }
        }


        [DataMember]
        private ActivityTimeInfo _PlannedTimes;

        [IgnoreDataMember]
        public ActivityTimeInfo PlannedTimes
        {
            get
            {
                lock (Lock)
                {
                    return _PlannedTimes;
                }
            }
            private set
            {
                lock (Lock)
                {
                    _PlannedTimes = value;
                }
            }
        }

        [IgnoreDataMember]
        public bool IsNull
        {
            get
            {
                return ActualTimes.IsNull && PlannedTimes.IsNull;
            }
        }

        #endregion

        #region Methods
        public PATimeInfo Clone()
        {
            return new PATimeInfo(ActualTimes.Clone(), PlannedTimes.Clone());
        }

        public void Reset()
        {
            var actualTimes = ActualTimes;
            var plannedTimes = PlannedTimes;

            actualTimes.PropertyChanged -= Times_PropertyChanged;
            actualTimes.Reset();
            actualTimes.PropertyChanged += Times_PropertyChanged;

            plannedTimes.PropertyChanged -= Times_PropertyChanged;
            plannedTimes.Reset();
            plannedTimes.PropertyChanged += Times_PropertyChanged;

            OnPropertyChanged("ActualTimes");
            OnPropertyChanged("PlannedTimes");
        }

        public void ReadFromProgramLog(ACProgramLog acProgramLog)
        {
            var actualTimes = ActualTimes;
            var plannedTimes = PlannedTimes;
            if (actualTimes == null)
            {
                actualTimes = new ActivityTimeInfo(acProgramLog.StartDate, acProgramLog.Duration, acProgramLog.EndDate);
                actualTimes.PropertyChanged += new PropertyChangedEventHandler(Times_PropertyChanged);
                ActualTimes = actualTimes;
            }
            else
                actualTimes.ChangeTime(acProgramLog.StartDate, acProgramLog.Duration, acProgramLog.EndDate);

            if (plannedTimes == null)
            {
                plannedTimes = new ActivityTimeInfo(acProgramLog.StartDatePlan, acProgramLog.DurationPlan, acProgramLog.EndDatePlan);
                plannedTimes.PropertyChanged += new PropertyChangedEventHandler(Times_PropertyChanged);
                PlannedTimes = plannedTimes;
            }
            else
                plannedTimes.ChangeTime(acProgramLog.StartDatePlan, acProgramLog.DurationPlan, acProgramLog.EndDatePlan);
        }

        public void StoreToProgramLog(ACProgramLog acProgramLog)
        {
            acProgramLog.StartDate = GetValidDateTime(ActualTimes.StartTimeValue);
            acProgramLog.Duration = ActualTimes.Duration;
            acProgramLog.EndDate = GetValidDateTime(ActualTimes.EndTimeValue);
            acProgramLog.StartDatePlan = GetValidDateTime(PlannedTimes.StartTime);
            acProgramLog.DurationPlan = PlannedTimes.Duration;
            acProgramLog.EndDatePlan = GetValidDateTime(PlannedTimes.EndTime);
        }

        public DateTime GetValidDateTime(DateTime? dateTimeValue)
        {
            if (!dateTimeValue.HasValue)
                return DateTime.Now;
            return GetValidDateTime(dateTimeValue.Value);
        }

        public DateTime GetValidDateTime(DateTime dateTimeValue)
        {
            if (dateTimeValue.Year < 2000)
                return DateTime.Now;
            return dateTimeValue;
        }

        void Times_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == _PlannedTimes)
                OnPropertyChanged("PlannedTimes");
            else if (sender == _ActualTimes)
                OnPropertyChanged("ActualTimes");
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
