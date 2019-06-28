﻿using System;

namespace FTServer.Monitor
{
    public static class GaugeExtensions
    {
        // Math copypasted from DateTimeOffset.cs in .NET Framework.

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
        private const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162
        private const long UnixEpochTicks = TimeSpan.TicksPerDay * DaysTo1970; // 621,355,968,000,000,000
        private const long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800

        /// <summary>
        /// Sets the value of the gauge to the current UTC time as a Unix timestamp in seconds.
        /// Value does not include any elapsed leap seconds because Unix timestamps do not include leap seconds.
        /// </summary>
        public static void SetToCurrentTimeUtc(this IGauge gauge)
        {
            // This gets us sub-millisecond precision, which is better than ToUnixTimeMilliseconds().
            var ticksSinceUnixEpoch = DateTimeOffset.UtcNow.Ticks - UnixEpochSeconds * TimeSpan.TicksPerSecond;
            var secondsSinceUnixEpoch = ticksSinceUnixEpoch / (double)TimeSpan.TicksPerSecond;
            gauge.Set(secondsSinceUnixEpoch);
        }

        private sealed class InProgressTracker : IDisposable
        {
            public InProgressTracker(IGauge gauge)
            {
                _gauge = gauge;
            }

            public void Dispose()
            {
                _gauge.Dec();
            }

            private readonly IGauge _gauge;
        }

        /// <summary>
        /// Tracks the number of in-progress operations taking place.
        /// 
        /// Calling this increments the gauge. Disposing of the returned instance decrements it again.
        /// </summary>
        /// <remarks>
        /// It is safe to track the sum of multiple concurrent in-progress operations with the same gauge.
        /// </remarks>
        public static IDisposable TrackInProgress(this IGauge gauge)
        {
            if (gauge == null)
                throw new ArgumentNullException(nameof(gauge));

            gauge.Inc();

            return new InProgressTracker(gauge);
        }
    }
}
