// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="PropertyLogListInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;

using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class PropertyLogListInfo
    /// </summary>
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PropertyLogListInfo'}de{'PropertyLogListInfo'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class PropertyLogListInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Enum PropertyLogState
        /// </summary>
        public enum PropertyLogState : short
        {
            /// <summary>
            /// The stopped
            /// </summary>
            Stopped = 0,
            /// <summary>
            /// The paused
            /// </summary>
            Paused = 1,
            /// <summary>
            /// The log active
            /// </summary>
            LogActive = 2
        }

        /// <summary>
        /// Constructor for ArchiveLog
        /// </summary>
        /// <param name="refreshRateOfItems">The refresh rate of items.</param>
        /// <param name="archiveLogList">The archive log list.</param>
        public PropertyLogListInfo(Global.MaxRefreshRates refreshRateOfItems, IList<PropertyLogItem> archiveLogList, Global.InterpolationMethod iPolMethod = Global.InterpolationMethod.None, double? range = null, double? decay = null)
        {
            _ArchiveLogList = archiveLogList;
            _IsLiveLog = false;
            _LiveLogList = null;
        }

        /// <summary>
        /// Constructor for LiveLog
        /// </summary>
        /// <param name="refreshRateOfItems">The refresh rate of items.</param>
        public PropertyLogListInfo(Global.MaxRefreshRates refreshRateOfItems, Global.InterpolationMethod iPolMethod = Global.InterpolationMethod.None, int? range = null, double? decay = null, int logBufferSize = 0)
        {
            _RefreshRate = refreshRateOfItems;
            _IsLiveLog = true;
            _LiveLogList = logBufferSize > 0 ? new PropertyLogRing(logBufferSize) : new PropertyLogRing();
            RunLiveLogging();
        }

        /// <summary>
        /// Gets or sets the AC caption.
        /// </summary>
        /// <value>The AC caption.</value>
        [ACPropertyInfo(9999)]
        [DataMember]
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// The _ refresh rate
        /// </summary>
        [DataMember]
        private Global.MaxRefreshRates _RefreshRate;
        /// <summary>
        /// Gets the refresh rate.
        /// </summary>
        /// <value>The refresh rate.</value>
        [ACPropertyInfo(9999)]
        public Global.MaxRefreshRates RefreshRate
        {
            get
            {
                return _RefreshRate;
            }
        }

        /// <summary>
        /// The _ live log list
        /// </summary>
        [DataMember]
        private PropertyLogRing _LiveLogList;

        [IgnoreDataMember]
        public PropertyLogRing LiveLogList
        {
            get
            {
                return _LiveLogList;
            }
        }

        private IList<PropertyLogItem> _InterpolLiveLogList;

        /// <summary>
        /// The _ archive log list
        /// </summary>
        [DataMember]
        private IList<PropertyLogItem> _ArchiveLogList;

        private IList<PropertyLogItem> _InterpolArchiveLogList;

        /// <summary>
        /// Gets the property log list.
        /// </summary>
        /// <value>The property log list.</value>
        [ACPropertyInfo(9999)]
        public IList<PropertyLogItem> PropertyLogList
        {
            get
            {
                if (_LiveLogList != null)
                {
                    if (InterpolationMethod != Global.InterpolationMethod.None)
                    {
                        if (_InterpolLiveLogList == null)
                            Interpolate();
                        return _InterpolLiveLogList;
                    }
                    return _LiveLogList;
                }
                else if (_ArchiveLogList != null)
                {
                    if (InterpolationMethod != Global.InterpolationMethod.None)
                    {
                        if (_InterpolArchiveLogList == null)
                            Interpolate();
                        return _InterpolArchiveLogList;
                    }
                    return _ArchiveLogList;
                }
                return null;
            }
        }

        /// <summary>
        /// The _ is live log
        /// </summary>
        [DataMember]
        private bool _IsLiveLog;
        /// <summary>
        /// Gets a value indicating whether this instance is live log.
        /// </summary>
        /// <value><c>true</c> if this instance is live log; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999)]
        public bool IsLiveLog
        {
            get
            {
                return _IsLiveLog;
            }
        }

        [DataMember]
        private short _InterpolationMethodIndex;
        /// <summary>
        /// Gets the refresh rate.
        /// </summary>
        /// <value>The refresh rate.</value>
        [ACPropertyInfo(9999)]
        public Global.InterpolationMethod InterpolationMethod
        {
            get
            {
                return (Global.InterpolationMethod) _InterpolationMethodIndex;
            }
            set
            {
                bool methodIndexChanged = (short)value != _InterpolationMethodIndex;
                _InterpolationMethodIndex = (short)value;
                if (methodIndexChanged)
                {
                    Interpolate();
                    OnPropertyChanged("InterpolationMethod");
                    OnPropertyChanged("PropertyLogList");
                }
            }
        }

        [DataMember]
        private int? _Range;

        public int Range
        {
            get
            {
                return _Range.HasValue ? _Range.Value : 5;
            }
        }

        [DataMember]
        private double? _Decay;

        public double Decay
        {
            get
            {
                return _Decay.HasValue ? _Decay.Value : 0.8;
            }
        }

        public bool IsInterpolationOn
        {
            get
            {
                return IsInterpolationPossible(InterpolationMethod, _Range);
            }
        }

        /// <summary>
        /// Stops the live logging.
        /// </summary>
        public void StopLiveLogging()
        {
            if (!IsEnabledStopLiveLogging())
                return;
            if (_LiveLogList != null)
                _LiveLogList.Clear();
            LogState = PropertyLogState.Stopped;
        }

        /// <summary>
        /// Determines whether [is enabled stop live logging].
        /// </summary>
        /// <returns><c>true</c> if [is enabled stop live logging]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledStopLiveLogging()
        {
            if (LogState == PropertyLogState.Stopped)
                return false;
            return true;
        }

        /// <summary>
        /// Pauses the live logging.
        /// </summary>
        public void PauseLiveLogging()
        {
            if (!IsEnabledPauseLiveLogging())
                return;
            LogState = PropertyLogState.Paused;
        }

        /// <summary>
        /// Determines whether [is enabled pause live logging].
        /// </summary>
        /// <returns><c>true</c> if [is enabled pause live logging]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledPauseLiveLogging()
        {
            if (LogState == PropertyLogState.LogActive)
                return true;
            return false;
        }

        /// <summary>
        /// Runs the live logging.
        /// </summary>
        public void RunLiveLogging()
        {
            if (!IsEnabledRunLiveLogging())
                return;
            LogState = PropertyLogState.LogActive;
        }

        /// <summary>
        /// Determines whether [is enabled run live logging].
        /// </summary>
        /// <returns><c>true</c> if [is enabled run live logging]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledRunLiveLogging()
        {
            if (LogState == PropertyLogState.LogActive)
                return false;
            return true;
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddValue(object value)
        {
            if (LogState != PropertyLogState.LogActive)
                return;
            if (!_IsLiveLog || _LiveLogList == null)
                return;
            _LiveLogList.AddValue(value);
        }

        /// <summary>
        /// The _ log state
        /// </summary>
        private PropertyLogState _LogState = PropertyLogState.Stopped;
        /// <summary>
        /// Gets the state of the log.
        /// </summary>
        /// <value>The state of the log.</value>
        public PropertyLogState LogState
        {
            get
            {
                return _LogState;
            }

            private set
            {
                _LogState = value;
                OnPropertyChanged("LogState");
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetInterpolationParams(Global.InterpolationMethod interpolationMethod, int? range, double? decay)
        {
            this._Range = range;
            this._Decay = decay;
            _InterpolationMethodIndex = (short)interpolationMethod;
        }

        /// <summary>
        /// Returns if PropertyLogList has enough values for Interpolation
        /// </summary>
        /// <param name="interpolationMethod"></param>
        /// <param name="range"></param>
        /// <param name="decay"></param>
        /// <returns></returns>
        public bool IsInterpolationPossible(Global.InterpolationMethod interpolationMethod, int? range)
        {
            if (interpolationMethod == Global.InterpolationMethod.None 
                || !range.HasValue 
                || range <= 0 
                /*|| (_ArchiveLogList == null && _LiveLogList == null)*/)
                return false;
            else if (interpolationMethod == Global.InterpolationMethod.MovingAverage)
            {
                if (_ArchiveLogList != null)
                    return _ArchiveLogList.Count > range;
                else if (_LiveLogList != null)
                    return _LiveLogList.Count > range;
                return false;
            }
            else if (interpolationMethod == Global.InterpolationMethod.WeightedForewardedAverage)
            {
                if (_ArchiveLogList != null)
                    return (_ArchiveLogList.Count * 2) > range;
                else if (_LiveLogList != null)
                    return (_LiveLogList.Count * 2) > range;
                return false;
            }
            else if (interpolationMethod == Global.InterpolationMethod.Median)
            {
                if (_ArchiveLogList != null)
                    return _ArchiveLogList.Count >= range;
                else if (_LiveLogList != null)
                    return _LiveLogList.Count >= range;
                return false;
            }
            return true;
        }

        public void Interpolate()
        {
            if (!IsInterpolationPossible(InterpolationMethod, Range))
                return;
            if (InterpolationMethod == Global.InterpolationMethod.MovingAverage)
            {
                if (_ArchiveLogList != null)
                    _InterpolArchiveLogList = ApplyMovingAverageFilter(_ArchiveLogList, Range);
                else if (_LiveLogList != null)
                    _InterpolLiveLogList = ApplyMovingAverageFilter(_LiveLogList, Range);
            }
            else if (InterpolationMethod == Global.InterpolationMethod.WeightedForewardedAverage)
            {
                if (_ArchiveLogList != null)
                    _InterpolArchiveLogList = ApplyFLAFilter(_ArchiveLogList, Range, Decay);
                else if (_LiveLogList != null)
                    _InterpolLiveLogList = ApplyFLAFilter(_LiveLogList, Range, Decay);
            }
            else if(InterpolationMethod == Global.InterpolationMethod.Median)
            {
                if (_ArchiveLogList != null)
                    _InterpolArchiveLogList = ApplyMedianFilter(_ArchiveLogList, Range);
                else if (_LiveLogList != null)
                    _InterpolLiveLogList = ApplyMedianFilter(_LiveLogList, Range);
            }
        }

        #region Interpolation-Methods

        #region Moving Average

        /// <summary>
        /// Moving Average
        /// </summary>
        /// <param name="noisySeries"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public IList<PropertyLogItem> ApplyMovingAverageFilter(IList<PropertyLogItem> noisySeries, int range)
        {
            // https://stackoverflow.com/questions/5166716/linq-to-calculate-a-moving-average-of-a-sortedlistdatetime-double

            var result = new List<PropertyLogItem>();
            if (noisySeries == null || !noisySeries.Any())
                return result;

            Type dataType = noisySeries[0].Value.GetType();
            for (int i = 0; i < noisySeries.Count(); i++)
            {
                if (i >= range - 1)
                {
                    double total = 0;
                    for (int x = i; x > (i - range); x--)
                        total += Convert.ToDouble(noisySeries[x].Value);
                    double average = total / range;
                    result.Add(new PropertyLogItem() { Time = noisySeries[i].Time, Value = Convert.ChangeType(average,dataType) });
                }

            }
            return result;
        }
        #endregion


        #region Foreword looking average with decay

        /// <summary>
        /// Foreword looking average with decay
        /// </summary>
        /// <param name="noisySeries"></param>
        /// <param name="range"></param>
        /// <param name="decay"></param>
        /// <returns></returns>
        static private IList<PropertyLogItem> ApplyFLAFilter(IList<PropertyLogItem> noisySeries, int range, double decay)
        {
            if (noisySeries == null)
                return null;
            PropertyLogItem[] rawData = noisySeries.ToArray();
            if (rawData.Length <= range * 2)
                return rawData;
            Type dataType = rawData[0].Value.GetType();
            double[] clean = CleanDataWithFLAFilter(rawData.Select(c => Convert.ToDouble(c.Value)).ToArray(), range, decay);
            for (int i = 0; i < rawData.Length; i++)
            {
                rawData[i].Value = Convert.ChangeType(clean[i], dataType);
            }
            return rawData;
        }


        /// <summary>
        /// http://www.robosoup.com/2014/01/cleaning-noisy-time-series-data-low-pass-filter-c.html
        /// </summary>
        /// <param name="noisy"></param>
        /// <param name="range"></param>
        /// <param name="decay"></param>
        /// <returns></returns>
        static private double[] CleanDataWithFLAFilter(double[] noisy, int range, double decay)
        {
            if (noisy.Length <= range * 2)
                return noisy;
            double[] clean = new double[noisy.Length];
            double[] coefficients = FLACoefficients(range, decay);
            
            // Calculate divisor value.
            double divisor = 0;
            for (int i = -range; i <= range; i++)
                divisor += coefficients[Math.Abs(i)];

            // Clean main data.
            for (int i = range; i < clean.Length - range; i++)
            {
                double temp = 0;
                for (int j = -range; j <= range; j++)
                    temp += noisy[i + j] * coefficients[Math.Abs(j)];
                clean[i] = temp / divisor;
            }
            // Calculate leading and trailing slopes.
            double leadSum = 0;
            double trailSum = 0;
            int leadRef = range;
            int trailRef = clean.Length - range - 1;
            for (int i = 1; i <= range; i++)
            {
                leadSum += (clean[leadRef] - clean[leadRef + i]) / i;
                trailSum += (clean[trailRef] - clean[trailRef - i]) / i;
            }
            double leadSlope = leadSum / range;
            double trailSlope = trailSum / range;
            // Clean edges.
            for (int i = 1; i <= range; i++)
            {
                clean[leadRef - i] = clean[leadRef] + leadSlope * i;
                clean[trailRef + i] = clean[trailRef] + trailSlope * i;
            }
            return clean;
        }

        static private double[] FLACoefficients(int range, double decay)
        {
            // Precalculate coefficients.
            double[] coefficients = new double[range + 1];
            for (int i = 0; i <= range; i++)
                coefficients[i] = Math.Pow(decay, i);
            return coefficients;
        }
        #endregion


        #region Median

        public IList<PropertyLogItem> ApplyMedianFilter(IList<PropertyLogItem> noisySeries, int range)
        {
            if (noisySeries == null)
                return null;

            PropertyLogItem[] rawData = noisySeries.OrderBy(c => c.Time).ToArray();
            if (rawData.Length < range)
                return rawData;

            int maxCalcItems = rawData.Length - range + 1;
            Type dataType = rawData[0].Value.GetType();
            double[] arrayData = rawData.Select(c => Convert.ToDouble(c.Value)).ToArray();

            List<PropertyLogItem> result = rawData.Take(maxCalcItems).ToList();

            for (int i=0; i < maxCalcItems; i++)
            {
                double calculatedMedian = CalculateMedian(arrayData.Skip(i).Take(range));
                result[i].Value = Convert.ChangeType(calculatedMedian, dataType);
            }

            return result;
        }

        private double CalculateMedian(IEnumerable<double> sourceData)
        {
            double result = 0;

            var sourceDataSorted = sourceData.OrderBy(c => c).ToArray();

            int sortedSetCount = sourceDataSorted.Count();

            int rest = sortedSetCount % 2;
            int middle = sortedSetCount / 2;

            if (rest > 0)
            {
                result = sourceDataSorted[middle];
            }
            else
            {
                double first = sourceDataSorted[middle - 1];
                double second = sourceDataSorted[middle];

                result = (first + second) / 2;
            }

            return result;
        }

        #endregion

        #endregion

    }
}
