// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using System;
using gip.core.licensing;
using Xunit;

namespace gip.core.licensing.unit.test
{
    public class LicenseGracePeriodTests
    {
        private static readonly DateTime T0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void NewGracePeriodIsNotRunning()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            Assert.False(grace.IsRunning);
            Assert.Null(grace.StartUtc);
            Assert.False(grace.IsWarningDue(T0));
            Assert.False(grace.IsBlockDue(T0));
        }

        [Fact]
        public void DefaultsMatchV4Behaviour()
        {
            Assert.Equal(TimeSpan.FromMinutes(30), LicenseGracePeriod.DefaultWarningPeriod);
            Assert.Equal(TimeSpan.FromDays(1.5), LicenseGracePeriod.DefaultBlockPeriod);

            LicenseGracePeriod grace = new LicenseGracePeriod();
            Assert.Equal(LicenseGracePeriod.DefaultWarningPeriod, grace.WarningPeriod);
            Assert.Equal(LicenseGracePeriod.DefaultBlockPeriod, grace.BlockPeriod);
        }

        [Fact]
        public void StartIsIdempotent()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            Assert.True(grace.Start(T0));
            Assert.False(grace.Start(T0.AddHours(1)));
            Assert.Equal(T0, grace.StartUtc);
        }

        [Fact]
        public void WarningIsDueAfterWarningPeriodOnly()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);

            Assert.False(grace.IsWarningDue(T0.AddMinutes(29)));
            Assert.True(grace.IsWarningDue(T0.AddMinutes(30)));
            Assert.True(grace.IsWarningDue(T0.AddMinutes(31)));
        }

        [Fact]
        public void BlockIsDueAfterBlockPeriodOnly()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);

            Assert.False(grace.IsBlockDue(T0.AddHours(35)));
            Assert.True(grace.IsBlockDue(T0.AddHours(36)));
            Assert.True(grace.IsBlockDue(T0.AddDays(2)));
        }

        [Fact]
        public void WarningComesBeforeBlock()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);

            DateTime warningPoint = T0.AddMinutes(30);
            Assert.True(grace.IsWarningDue(warningPoint));
            Assert.False(grace.IsBlockDue(warningPoint));
        }

        [Fact]
        public void ClearStopsTheClock()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);
            grace.Clear();

            Assert.False(grace.IsRunning);
            Assert.Null(grace.StartUtc);
            Assert.False(grace.IsWarningDue(T0.AddDays(5)));
            Assert.False(grace.IsBlockDue(T0.AddDays(5)));
        }

        [Fact]
        public void ClearAllowsANewStart()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);
            grace.Clear();
            Assert.True(grace.Start(T0.AddDays(1)));
            Assert.Equal(T0.AddDays(1), grace.StartUtc);
        }

        [Fact]
        public void RestoreKeepsAPersistedStart()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Restore(T0);

            Assert.True(grace.IsRunning);
            Assert.Equal(T0, grace.StartUtc);
            Assert.True(grace.IsBlockDue(T0.AddDays(2)));
        }

        [Fact]
        public void RestoreWithNullClearsTheClock()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            grace.Start(T0);
            grace.Restore(null);
            Assert.False(grace.IsRunning);
        }

        [Fact]
        public void CustomPeriodsAreHonoured()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod(TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
            grace.Start(T0);

            Assert.False(grace.IsWarningDue(T0.AddMinutes(4)));
            Assert.True(grace.IsWarningDue(T0.AddMinutes(5)));
            Assert.False(grace.IsBlockDue(T0.AddMinutes(59)));
            Assert.True(grace.IsBlockDue(T0.AddHours(1)));
        }

        [Fact]
        public void BlockPeriodShorterThanWarningPeriodIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LicenseGracePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(1)));
        }

        [Fact]
        public void NegativeWarningPeriodIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new LicenseGracePeriod(TimeSpan.FromMinutes(-1), TimeSpan.FromHours(1)));
        }

        [Fact]
        public void ConcurrentStartsOnlyOneWins()
        {
            LicenseGracePeriod grace = new LicenseGracePeriod();
            int winners = 0;
            System.Threading.Tasks.Parallel.For(0, 64, _ =>
            {
                if (grace.Start(T0))
                    System.Threading.Interlocked.Increment(ref winners);
            });

            Assert.Equal(1, winners);
            Assert.Equal(T0, grace.StartUtc);
        }
    }
}
