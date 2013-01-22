﻿using System;
using NodaTime.Benchmarks.Timing;

namespace NodaTime.Benchmarks.FrameworkComparisons
{
    internal sealed class TimeZoneInfoBenchmarks
    {
        internal static readonly TimeZoneInfo PacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        internal static readonly DateTime SummerUtc = new DateTime(1976, 6, 19, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly DateTime WinterUtc = new DateTime(2003, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime SummerUnspecified = DateTime.SpecifyKind(SummerUtc, DateTimeKind.Unspecified);
        private static readonly DateTime WinterUnspecified = DateTime.SpecifyKind(WinterUtc, DateTimeKind.Unspecified);
        private static readonly DateTimeOffset SummerOffset = new DateTimeOffset(SummerUnspecified, TimeSpan.FromHours(5));
        private static readonly DateTimeOffset WinterOffset = new DateTimeOffset(WinterUnspecified, TimeSpan.FromHours(5));

        [Benchmark]
        public void GetUtcOffset_Utc()
        {
            PacificZone.GetUtcOffset(SummerUtc);
            PacificZone.GetUtcOffset(WinterUtc);
        }

        [Benchmark]
        public void GetUtcOffset_Unspecified()
        {
            PacificZone.GetUtcOffset(SummerUnspecified);
            PacificZone.GetUtcOffset(WinterUnspecified);
        }

        [Benchmark]
        public void GetUtcOffset_DateTimeOffset()
        {
            PacificZone.GetUtcOffset(SummerOffset);
            PacificZone.GetUtcOffset(WinterOffset);
        }
    }
}
