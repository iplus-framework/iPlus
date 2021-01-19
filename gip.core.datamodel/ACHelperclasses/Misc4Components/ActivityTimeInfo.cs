// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ActivityTimeInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Threading;
using System.Data.Objects.DataClasses;
using System.Data;
using System.Data.Objects;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ActivityTimeInfo
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ActivityTimeInfo'}de{'ActivityTimeInfo'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ActivityTimeInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Enum Property
        /// </summary>
        public enum Property
        {
            /// <summary>
            /// The none
            /// </summary>
            None = 0,
            /// <summary>
            /// The start time
            /// </summary>
            StartTime = 1,
            /// <summary>
            /// The duration
            /// </summary>
            Duration = 2,
            /// <summary>
            /// The end time
            /// </summary>
            EndTime = 3
        }

        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        public ActivityTimeInfo()
        {
            InitLock();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="duration">The duration.</param>
        public ActivityTimeInfo(DateTime startTime, TimeSpan duration)
        {
            InitLock();
            ChangeTime(startTime, duration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        public ActivityTimeInfo(DateTime startTime, DateTime endTime)
        {
            ChangeTime(startTime, endTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="endTime">The end time.</param>
        public ActivityTimeInfo(TimeSpan duration, DateTime endTime)
        {
            InitLock();
            ChangeTime(duration, endTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="endTime">The end time.</param>
        public ActivityTimeInfo(Nullable<DateTime> startTime, Nullable<TimeSpan> duration, Nullable<DateTime> endTime)
        {
            InitLock();
            ChangeTime(startTime, duration, endTime);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTimeInfo"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="endTime">The end time.</param>
        /// <exception cref="System.ArgumentException">Time-Value-Triple is not equal</exception>
        public ActivityTimeInfo(DateTime startTime, TimeSpan duration, DateTime endTime)
        {
            InitLock();
            //if (startTime + duration != endTime)
            //throw new ArgumentException("Time-Value-Triple is not equal");
            _StartTimeValue = startTime;
            _DurationValue = duration;
            _EndTimeValue = endTime;
            OnPropertyChangedAll();
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

        #region Stored Properties
        /// <summary>
        /// The _ last manipulated properties
        /// </summary>
        private ActivityTimeInfo.Property[] _LastManipulatedProperties = new Property[] { Property.None, Property.None };

        /// <summary>
        /// Gets the last manipulated property.
        /// </summary>
        /// <value>The last manipulated property.</value>
        public ActivityTimeInfo.Property LastManipulatedProperty
        {
            get
            {
                lock (Lock)
                {
                    return _LastManipulatedProperties[0];
                }
            }
            private set
            {
                lock (Lock)
                {
                    if (_LastManipulatedProperties[0] != value)
                    {
                        _LastManipulatedProperties[1] = _LastManipulatedProperties[0];
                        _LastManipulatedProperties[0] = value;
                    }
                }
            }
        }

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

        /// <summary>
        /// The _ start time value
        /// </summary>
        [DataMember]
        private Nullable<DateTime> _StartTimeValue;

        /// <summary>
        /// Gets the start time value.
        /// </summary>
        /// <value>The start time value.</value>
        [IgnoreDataMember]
        public Nullable<DateTime> StartTimeValue
        {
            get
            {
                lock (Lock)
                {
                    return _StartTimeValue;
                }
            }
        }


        /// <summary>
        /// The _ duration value
        /// </summary>
        [DataMember]
        private Nullable<TimeSpan> _DurationValue;

        /// <summary>
        /// Gets the duration value.
        /// </summary>
        /// <value>The duration value.</value>
        [IgnoreDataMember]
        public Nullable<TimeSpan> DurationValue
        {
            get
            {
                lock (Lock)
                {
                    return _DurationValue;
                }
            }
        }

        /// <summary>
        /// The _ end time value
        /// </summary>
        [DataMember]
        private Nullable<DateTime> _EndTimeValue;

        /// <summary>
        /// Gets the end time value.
        /// </summary>
        /// <value>The end time value.</value>
        [IgnoreDataMember]
        public Nullable<DateTime> EndTimeValue
        {
            get
            {
                lock (Lock)
                {
                    return _EndTimeValue;
                }
            }
        }
        #endregion

        #region Calculated Properties
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>The start time.</value>
        [IgnoreDataMember]
        public DateTime StartTime
        {
            get
            {
                if (StartTimeValue.HasValue)
                    return StartTimeValue.Value;
                else if (DurationValue.HasValue && EndTimeValue.HasValue)
                    return EndTimeValue.Value - DurationValue.Value;
                return DateTime.MinValue;
            }
            set
            {
                lock (Lock)
                {
                    _StartTimeValue = value;
                }
                OnPropertyChanged("StartTimeValue");
                OnPropertyChanged("StartTime");
                if (DurationValue.HasValue && !EndTimeValue.HasValue)
                {
                    lock (Lock)
                    {
                        _EndTimeValue = StartTimeValue + DurationValue;
                    }
                    OnPropertyChanged("EndTimeValue");
                    OnPropertyChanged("EndTime");
                }
                else if (EndTimeValue.HasValue && !DurationValue.HasValue)
                {
                    lock (Lock)
                    {
                        _DurationValue = EndTimeValue - StartTimeValue;
                    }
                    OnPropertyChanged("DurationValue");
                    OnPropertyChanged("Duration");
                }
                else if (EndTimeValue.HasValue && DurationValue.HasValue)
                {
                    ActivityTimeInfo.Property property0;
                    ActivityTimeInfo.Property property1;
                    lock (Lock)
                    {
                        property0 = _LastManipulatedProperties[0];
                        property1 = _LastManipulatedProperties[1];
                    }

                    if (property0 == Property.EndTime
                        || (property0 != Property.Duration
                            && property1 == Property.EndTime))
                    {
                        lock (Lock)
                        {
                            _DurationValue = EndTimeValue - StartTimeValue;
                        }
                        OnPropertyChanged("DurationValue");
                        OnPropertyChanged("Duration");
                    }
                    else
                    {
                        lock (Lock)
                        {
                            _EndTimeValue = StartTimeValue + DurationValue;
                        }
                        OnPropertyChanged("EndTimeValue");
                        OnPropertyChanged("EndTime");
                    }
                }
                LastManipulatedProperty = Property.StartTime;
            }
        }


        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>The duration.</value>
        [IgnoreDataMember]
        public TimeSpan Duration
        {
            get
            {
                if (DurationValue.HasValue)
                    return DurationValue.Value;
                else if (StartTimeValue.HasValue && EndTimeValue.HasValue)
                    return EndTimeValue.Value - StartTimeValue.Value;
                return TimeSpan.Zero;
            }
            set
            {
                lock (Lock)
                {
                    _DurationValue = value;
                }
                OnPropertyChanged("DurationValue");
                OnPropertyChanged("Duration");
                if (StartTimeValue.HasValue && !EndTimeValue.HasValue)
                {
                    lock (Lock)
                    {
                        _EndTimeValue = StartTimeValue + DurationValue;
                    }
                    OnPropertyChanged("EndTimeValue");
                    OnPropertyChanged("EndTime");
                }
                else if (EndTimeValue.HasValue && !StartTimeValue.HasValue)
                {
                    lock (Lock)
                    {
                        _StartTimeValue = EndTimeValue - DurationValue;
                    }
                    OnPropertyChanged("StartTimeValue");
                    OnPropertyChanged("StartTime");
                }
                else if (StartTimeValue.HasValue && EndTimeValue.HasValue)
                {
                    ActivityTimeInfo.Property property0;
                    ActivityTimeInfo.Property property1;
                    lock (Lock)
                    {
                        property0 = _LastManipulatedProperties[0];
                        property1 = _LastManipulatedProperties[1];
                    }

                    if (property0 == Property.EndTime
                        || (property0 != Property.StartTime
                            && property1 == Property.EndTime))
                    {
                        lock (Lock)
                        {
                            _StartTimeValue = EndTimeValue - DurationValue;
                        }
                        OnPropertyChanged("StartTimeValue");
                        OnPropertyChanged("StartTime");
                    }
                    else
                    {
                        lock (Lock)
                        {
                            _EndTimeValue = StartTimeValue + DurationValue;
                        }
                        OnPropertyChanged("EndTimeValue");
                        OnPropertyChanged("EndTime");
                    }
                }
                LastManipulatedProperty = Property.Duration;
            }
        }


        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>The end time.</value>
        [IgnoreDataMember]
        public DateTime EndTime
        {
            get
            {
                if (EndTimeValue.HasValue)
                    return EndTimeValue.Value;
                else if (StartTimeValue.HasValue && DurationValue.HasValue)
                    return StartTimeValue.Value + DurationValue.Value;
                return DateTime.MaxValue;
            }
            set
            {
                lock (Lock)
                {
                    _EndTimeValue = value;
                }
                OnPropertyChanged("EndTimeValue");
                OnPropertyChanged("EndTime");
                if (DurationValue.HasValue && !StartTimeValue.HasValue)
                {
                    lock (Lock)
                    {
                        _StartTimeValue = EndTimeValue - DurationValue;
                    }
                    OnPropertyChanged("StartTimeValue");
                    OnPropertyChanged("StartTime");
                }
                else if (StartTimeValue.HasValue && !DurationValue.HasValue)
                {
                    lock (Lock)
                    {
                        _DurationValue = EndTimeValue - StartTimeValue;
                    }
                    OnPropertyChanged("DurationValue");
                    OnPropertyChanged("Duration");
                }
                else if (StartTimeValue.HasValue && DurationValue.HasValue)
                {
                    ActivityTimeInfo.Property property0;
                    ActivityTimeInfo.Property property1;
                    lock (Lock)
                    {
                        property0 = _LastManipulatedProperties[0];
                        property1 = _LastManipulatedProperties[1];
                    }

                    if (property0 == Property.StartTime
                        || (property0 != Property.Duration
                            && property1 == Property.StartTime))
                    {
                        lock (Lock)
                        {
                            _DurationValue = EndTimeValue - StartTimeValue;
                        }
                        OnPropertyChanged("DurationValue");
                        OnPropertyChanged("Duration");
                    }
                    else
                    {
                        lock (Lock)
                        {
                            _StartTimeValue = EndTimeValue - DurationValue;
                        }
                        OnPropertyChanged("StartTimeValue");
                        OnPropertyChanged("StartTime");
                    }
                }
                LastManipulatedProperty = Property.EndTime;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
        [IgnoreDataMember]
        public bool IsNull
        {
            get
            {
                lock (Lock)
                {
                    return !_StartTimeValue.HasValue && !_DurationValue.HasValue && !_EndTimeValue.HasValue;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>ActivityTimeInfo.</returns>
        public ActivityTimeInfo Clone()
        {
            return new ActivityTimeInfo(_StartTimeValue, _DurationValue, _EndTimeValue);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            lock (Lock)
            {
                _StartTimeValue = null;
                _DurationValue = null;
                _EndTimeValue = null;
                _LastManipulatedProperties[0] = Property.None;
                _LastManipulatedProperties[1] = Property.None;
            }
            OnPropertyChangedAll();
        }

        /// <summary>
        /// Setzt StartTime und Duration und berechnet EndTime durch Addition
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="duration">The duration.</param>
        public void ChangeTime(DateTime startTime, TimeSpan duration)
        {
            lock (Lock)
            {
                _StartTimeValue = startTime;
                _DurationValue = duration;
                _EndTimeValue = startTime + duration;
            }
            OnPropertyChangedAll();
        }

        /// <summary>
        /// Setzt StartTime und EndTime und berechnet Duration durch Differenzbildung
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        public void ChangeTime(DateTime startTime, DateTime endTime)
        {
            lock (Lock)
            {
                _StartTimeValue = startTime;
                _EndTimeValue = endTime;
                _DurationValue = endTime - startTime;
            }
            OnPropertyChangedAll();
        }

        /// <summary>
        /// Setzt EndTime und Duration und berechnet StartTime durch Differenzbildung
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <param name="endTime">The end time.</param>
        public void ChangeTime(TimeSpan duration, DateTime endTime)
        {
            lock (Lock)
            {
                _EndTimeValue = endTime;
                _DurationValue = duration;
                _StartTimeValue = endTime - duration;
            }
            OnPropertyChangedAll();
        }

        /// <summary>
        /// Changes the time.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="endTime">The end time.</param>
        /// <exception cref="System.ArgumentException">Time-Value-Triple is not equal</exception>
        public void ChangeTime(Nullable<DateTime> startTime, Nullable<TimeSpan> duration, Nullable<DateTime> endTime)
        {
            if (startTime.HasValue && duration.HasValue && endTime.HasValue)
            {
                //if (startTime.Value + duration.Value != endTime.Value)
                //throw new ArgumentException("Time-Value-Triple is not equal");
                lock (Lock)
                {
                    _StartTimeValue = startTime;
                    _DurationValue = duration;
                    _EndTimeValue = endTime;
                }
                OnPropertyChangedAll();
            }
            else if (startTime.HasValue && duration.HasValue)
            {
                ChangeTime(startTime.Value, duration.Value);
            }
            else if (duration.HasValue && endTime.HasValue)
            {
                ChangeTime(duration.Value, endTime.Value);
            }
            else if (startTime.HasValue && endTime.HasValue)
            {
                ChangeTime(startTime.Value, endTime.Value);
            }
            else
            {
                if (startTime.HasValue)
                {
                    lock (Lock)
                    {
                        _StartTimeValue = startTime;
                    }
                    OnPropertyChanged("StartTimeValue");
                    OnPropertyChanged("StartTime");
                }
                else if (duration.HasValue)
                {
                    lock (Lock)
                    {
                        _DurationValue = duration;
                    }
                    OnPropertyChanged("DurationValue");
                    OnPropertyChanged("Duration");
                }
                else if (endTime.HasValue)
                {
                    lock (Lock)
                    {
                        _EndTimeValue = endTime;
                    }
                    OnPropertyChanged("EndTimeValue");
                    OnPropertyChanged("EndTime");
                }
            }
        }

        /// <summary>
        /// Called when [property changed all].
        /// </summary>
        protected void OnPropertyChangedAll()
        {
            OnPropertyChanged("StartTimeValue");
            OnPropertyChanged("StartTime");
            OnPropertyChanged("DurationValue");
            OnPropertyChanged("Duration");
            OnPropertyChanged("EndTimeValue");
            OnPropertyChanged("EndTime");
        }
        #endregion

        #region events
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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
