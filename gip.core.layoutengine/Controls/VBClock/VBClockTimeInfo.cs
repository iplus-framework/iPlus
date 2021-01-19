using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a VBClock time info
    /// </summary>
	public sealed class VBClockTimeInfo
	{
        /// <summary>
        /// Creates a new VBClockTimeInfo
        /// </summary>
        /// <param name="tz">The time zone.</param>
        public VBClockTimeInfo(TimeZoneInfo tz)
		{
			_timezone = tz;
		}

        /// <summary>
        /// Copies the VBClock time info.
        /// </summary>
        /// <param name="ti">Time info to copy.</param>
        /// <returns>Returns copied time info.</returns>
        public static VBClockTimeInfo Copy(VBClockTimeInfo ti)
		{
            VBClockTimeInfo copy = new VBClockTimeInfo(ti.TimeZoneInfo);

			if( ti.IsDisplayNameOverridden )
			{
				copy.DisplayName = ti.DisplayName;
			}

			return copy;
		}

        /// <summary>
        /// Gets is display name overridden.
        /// </summary>
        public bool IsDisplayNameOverridden
		{
			get
			{
				return _displayName != null;
			}
		}

        /// <summary>
        /// Gets the offset name.
        /// </summary>
		public string OffsetName
		{
			get
			{
				if( _timezone.BaseUtcOffset == TimeSpan.Zero )
				{
					return "GMT";
				}
				else if( _timezone.BaseUtcOffset.Hours < 0 )
				{
					return string.Format( "GMT-{0}:{1:00}", -_timezone.BaseUtcOffset.Hours, -_timezone.BaseUtcOffset.Minutes );
				}
				else
				{
					return string.Format( "GMT+{0}:{1:00}", _timezone.BaseUtcOffset.Hours, _timezone.BaseUtcOffset.Minutes );
				}
			}
		}

        /// <summary>
        /// Gets or sets the Display name.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Anzeigenamen.
        /// </summary>
        [Category("VBControl")]
        public string DisplayName
		{
			get
			{
				return _displayName ?? _timezone.DisplayName;
			}
			set
			{
				if( value == _timezone.DisplayName )
				{
					_displayName = null;
				}
				else
				{
					_displayName = value;
				}
			}
		}

        /// <summary>
        /// Gets the time zone info.
        /// </summary>
		public TimeZoneInfo TimeZoneInfo
		{
			get
			{
				return _timezone;
			}
		}

        /// <summary>
        /// Get the adjusted date time.
        /// </summary>
        /// <param name="start">The start date time.</param>
        /// <returns>Returns adjusted date time.</returns>
		public DateTime GetAdjusted( DateTime start )
		{
			return start.Add( _timezone.GetUtcOffset( start ) );
		}

		private TimeZoneInfo _timezone;
		private string _displayName;
	}
}
