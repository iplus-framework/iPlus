// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;

namespace gip.core.licensing
{
    /// <summary>
    /// Two-stage grace period for license problems (V4 parity).
    /// </summary>
    /// <remarks>
    /// When a license problem is detected the clock starts. After
    /// <see cref="WarningPeriod"/> the client shows a warning message once, but the
    /// system stays fully usable. After <see cref="BlockPeriod"/> the server blocks
    /// <c>ACUrlCommand</c>, so the program stops working. This gives the end user
    /// time to resolve the license issue.
    ///
    /// The class is deliberately free of any database or UI dependency so that it
    /// can be unit tested and reused by tools.
    /// </remarks>
    public sealed class LicenseGracePeriod
    {
        /// <summary>
        /// Default period before the client starts warning (30 minutes).
        /// </summary>
        public static readonly TimeSpan DefaultWarningPeriod = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Default period before the server blocks <c>ACUrlCommand</c> (1.5 days).
        /// </summary>
        public static readonly TimeSpan DefaultBlockPeriod = TimeSpan.FromDays(1.5);

        private readonly object _Lock = new object();
        private DateTime? _StartUtc;

        /// <summary>
        /// Creates a grace period with the default durations.
        /// </summary>
        public LicenseGracePeriod()
            : this(DefaultWarningPeriod, DefaultBlockPeriod)
        {
        }

        /// <summary>
        /// Creates a grace period with explicit durations.
        /// </summary>
        public LicenseGracePeriod(TimeSpan warningPeriod, TimeSpan blockPeriod)
        {
            if (warningPeriod < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(warningPeriod));
            if (blockPeriod < warningPeriod)
                throw new ArgumentOutOfRangeException(nameof(blockPeriod), "The block period must not be shorter than the warning period.");

            WarningPeriod = warningPeriod;
            BlockPeriod = blockPeriod;
        }

        /// <summary>
        /// Period after which the client shows a warning.
        /// </summary>
        public TimeSpan WarningPeriod { get; }

        /// <summary>
        /// Period after which the server blocks <c>ACUrlCommand</c>.
        /// </summary>
        public TimeSpan BlockPeriod { get; }

        /// <summary>
        /// Point in time when the current license problem was first detected, or
        /// <c>null</c> while the installation is properly licensed.
        /// </summary>
        public DateTime? StartUtc
        {
            get
            {
                lock (_Lock)
                {
                    return _StartUtc;
                }
            }
        }

        /// <summary>
        /// <c>true</c> while a license problem is being tracked.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_Lock)
                {
                    return _StartUtc.HasValue;
                }
            }
        }

        /// <summary>
        /// <c>true</c> when the warning period has elapsed. The system is still
        /// fully usable.
        /// </summary>
        public bool IsWarningDue(DateTime? nowUtc = null)
        {
            DateTime? start;
            lock (_Lock)
            {
                start = _StartUtc;
            }
            if (!start.HasValue)
                return false;
            return (nowUtc ?? DateTime.UtcNow) - start.Value >= WarningPeriod;
        }

        /// <summary>
        /// <c>true</c> when the block period has elapsed. From this point on
        /// unlicensed packages are blocked.
        /// </summary>
        public bool IsBlockDue(DateTime? nowUtc = null)
        {
            DateTime? start;
            lock (_Lock)
            {
                start = _StartUtc;
            }
            if (!start.HasValue)
                return false;
            return (nowUtc ?? DateTime.UtcNow) - start.Value >= BlockPeriod;
        }

        /// <summary>
        /// Starts the clock. Idempotent — the first detection wins, so repeated
        /// checks do not extend the grace period.
        /// </summary>
        /// <returns><c>true</c> when the clock was started by this call.</returns>
        public bool Start(DateTime? nowUtc = null)
        {
            lock (_Lock)
            {
                if (_StartUtc.HasValue)
                    return false;
                _StartUtc = nowUtc ?? DateTime.UtcNow;
                return true;
            }
        }

        /// <summary>
        /// Clears the clock (license problem resolved).
        /// </summary>
        public void Clear()
        {
            lock (_Lock)
            {
                _StartUtc = null;
            }
        }

        /// <summary>
        /// Restores a previously persisted start time (e.g. after a restart).
        /// </summary>
        public void Restore(DateTime? startUtc)
        {
            lock (_Lock)
            {
                _StartUtc = startUtc;
            }
        }
    }
}
