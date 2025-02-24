using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace gip.core.autocomponent
{
    [DataContract]
    public class PAUserTimeInfo : EntityBase
    {
        [DataMember]
        private DateTime? _StartDate;
        [IgnoreDataMember]
        public DateTime? StartDate
        {
            get => _StartDate;
            set
            {
                _StartDate = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private DateTime? _UserStartDate;
        [IgnoreDataMember]
        public DateTime? UserStartDate
        {
            get => _UserStartDate;
            set
            {
                _UserStartDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }

        [DataMember]
        private DateTime? _EndDate;
        [IgnoreDataMember]
        public DateTime? EndDate
        {
            get => _EndDate;
            set
            {
                _EndDate = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        private DateTime? _UserEndDate;
        [IgnoreDataMember]
        public DateTime? UserEndDate
        {
            get => _UserEndDate;
            set
            {
                _UserEndDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }

        [IgnoreDataMember]
        private TimeSpan _UserStartTime;
        [IgnoreDataMember]
        public TimeSpan UserStartTime
        {
            get
            {
                return _UserStartTime;
            }
            set
            {
                _UserStartTime = value;
                OnPropertyChanged();
                UpdateDate(false);
                OnPropertyChanged(nameof(Duration));
            }
        }

        [IgnoreDataMember]
        private TimeSpan _UserEndTime;
        [IgnoreDataMember]
        public TimeSpan UserEndTime
        {
            get
            {
                return _UserEndTime;
            }
            set
            {
                _UserEndTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Duration));
            }
        }

        [IgnoreDataMember]
        private DateTime? _UserEndDateTemp;
        [IgnoreDataMember]
        public DateTime? UserEndDateTemp
        {
            get => _UserEndDateTemp;
            set
            {
                _UserEndDateTemp = value;
                OnPropertyChanged();
                UpdateDate(true);
                OnPropertyChanged(nameof(Duration));
            }
        }

        [IgnoreDataMember]
        public string Duration
        {
            get
            {
                if (!UserStartDate.HasValue || !UserEndDateTemp.HasValue)
                    return TimeSpan.Zero.ToString(@"d\.hh\:mm");

                return (UserEndDateTemp.Value - UserStartDate.Value).ToString(@"d\.hh\:mm");
            }
        }

        public void UpdateTime()
        {
            if (_UserStartTime == TimeSpan.Zero)
            {
                if (_UserStartDate != null)
                    UserStartTime = UserStartDate.Value.TimeOfDay;

                if (StartDate != null)
                    UserStartTime = StartDate.Value.TimeOfDay;
            }

            if (_UserEndTime == TimeSpan.Zero)
            {
                if (UserEndDate != null)
                    UserEndDateTemp = UserEndDate;
                else
                    UserEndDateTemp = DateTime.Now;

                UserEndTime = UserEndDateTemp.Value.TimeOfDay;
            }
        }

        public bool UpdateDate(bool updateEndDateTime)
        {
            if (UserStartDate.HasValue)
            {
                DateTime temp = UserStartDate.Value;
                UserStartDate = new DateTime(temp.Year, temp.Month, temp.Day, UserStartTime.Hours, UserStartTime.Minutes, UserStartTime.Seconds);
            }

            if (updateEndDateTime)
            {
                DateTime temp = UserEndDateTemp.Value;
                if(UserEndTime.TotalSeconds != 0)
                {
                    UserEndDate = new DateTime(temp.Year, temp.Month, temp.Day, UserEndTime.Hours, UserEndTime.Minutes, UserEndTime.Seconds);
                }
            }

            if (UserEndDate.HasValue)
                return UserStartDate < UserEndDate;

            return UserStartDate < UserEndDateTemp;
        }
    }
}
