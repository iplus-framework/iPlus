// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-17-2013
// ***********************************************************************
// <copyright file="Translator.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Returns the local time without daylight saving time (Wintertime)
        /// </summary>
        public static DateTime NowDST
        {
            get
            {
                DateTime dt = DateTime.UtcNow;
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                dt = dt.Add(localZone.BaseUtcOffset);
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                return dt;
            }
        }

        /// <summary>
        /// Returns the local time with offest to utc without daylight saving time
        /// </summary>
        public static DateTimeOffset NowOffDST
        {
            get
            {
                return new DateTimeOffset(NowDST);
            }
        }

        public static DateTime GetWinterTime(this DateTime dateTime)
        {
            if (dateTime.IsDaylightSavingTime())
                return dateTime.AddHours(-1);
            return dateTime;
        }

        /// <summary>
        /// This Method is for correcting Times, that are stored in the database without daylight savings (e.g. ProgramLog) and should be
        /// displayed on the screen with the daylight svaing offset.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeDSTCorrected(this DateTime dateTime)
        {
            // IsDaylightSavingTime => Summertime (One hour forward, than normal)
            return dateTime.IsDaylightSavingTime() ? dateTime.AddHours(1) : dateTime;
        }

        public static DateTime AddDurationDSTCorrected(this DateTime startTime, TimeSpan duration)
        {
            // Wintertime -> Summertime (Switch from 2h => 3h)
            //26.03.2023 01:59:00 => IsDaylightSavingTime(): False
            //26.03.2023 02:00:01 => IsDaylightSavingTime(): False
            //26.03.2023 03:00:01 => IsDaylightSavingTime(): True

            // Summertime -> Wintertime (Switch from 3h => 2h)
            //29.10.2023 01:59:00 => IsDaylightSavingTime(): True
            //29.10.2023 02:00:01 => IsDaylightSavingTime(): False
            //29.10.2023 03:00:01 => IsDaylightSavingTime(): False

            DateTime endTime = startTime.Add(duration);
            DateTime endTimeTestSwitchForeward = endTime.AddHours(1);
            DateTime endTimeTestSwitchBackward = endTime.AddHours(-1);
            // If entime in Summer (+ 1h)
            if (endTime.IsDaylightSavingTime())
            {
                // Test if in the meantime has happend a switch from winter to summer
                if (startTime.IsDaylightSavingTime() != endTime.IsDaylightSavingTime())
                    return endTimeTestSwitchForeward;
            }
            // If endtime in Winter
            else
            {
                // If endtime falled in the period where time is switching from 2 -> 3 hour (summer -> wintertime), then return 1h more
                if (endTimeTestSwitchForeward.IsDaylightSavingTime())
                    return endTimeTestSwitchForeward;
                // Test if in the meantime has happend a switch from summer to winter
                else if (startTime.IsDaylightSavingTime() != endTime.IsDaylightSavingTime()
                        && !endTimeTestSwitchBackward.IsDaylightSavingTime())
                    return endTimeTestSwitchBackward;
            }
            return endTime;
        }

        public static DateTime SubtractDurationDSTCorrected(this DateTime endTime, TimeSpan duration)
        {
            throw new NotImplementedException();
            // Wintertime -> Summertime (Switch from 2h => 3h)
            //26.03.2023 01:59:00 => IsDaylightSavingTime(): False
            //26.03.2023 02:00:01 => IsDaylightSavingTime(): False
            //26.03.2023 03:00:01 => IsDaylightSavingTime(): True

            // Summertime -> Wintertime (Switch from 3h => 2h)
            //29.10.2023 01:59:00 => IsDaylightSavingTime(): True
            //29.10.2023 02:00:01 => IsDaylightSavingTime(): False
            //29.10.2023 03:00:01 => IsDaylightSavingTime(): False

            //DateTime startTime = endTime.Subtract(duration);
            //DateTime startTimeTestSwitchForeward = startTime.AddHours(1);
            //DateTime startTimeTestSwitchBackward = startTime.AddHours(-1);
            //// TODO:!!!
            //return startTime;
        }
    }
}
