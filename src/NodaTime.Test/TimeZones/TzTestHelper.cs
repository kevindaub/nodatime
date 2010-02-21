﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace NodaTime.Test.TimeZones
{
    /// <summary>
    /// Extension methods to help with time zone testing, and other helper methods.
    /// </summary>
    internal static class TzTestHelper
    {
        /// <summary>
        /// Returns the uncached version of the given zone. If the zone isn't
        /// an instance of CachedDateTimeZone, the same reference is returned back.
        /// </summary>
        internal static IDateTimeZone Uncached(this IDateTimeZone zone)
        {
            CachedDateTimeZone cached = zone as CachedDateTimeZone;
            return cached == null ? zone : cached.TimeZone;
        }

        internal static Transition ValidateNextTransition(this IDateTimeZone zone, Instant instant)
        {
            Transition? transition = zone.NextTransition(instant);
            return transition.Validate(zone);
        }

        internal static Transition ValidatePreviousTransition(this IDateTimeZone zone, Instant instant)
        {
            Transition? transition = zone.PreviousTransition(instant);
            return transition.Validate(zone);
        }

        /// <summary>
        /// Convenience method which puts a transition through its paces. Apply liberally
        /// to any transition you don't expect to be null.
        /// </summary>
        internal static Transition Validate(this Transition? nullableTransition, IDateTimeZone zone)
        {
            Assert.IsNotNull(nullableTransition);
            Transition transition = nullableTransition.Value;
            Assert.AreEqual(transition.NewOffset, zone.GetOffsetFromUtc(transition.Instant));
            Assert.AreEqual(transition.OldOffset, zone.GetOffsetFromUtc(transition.Instant - Duration.One));

            Instant instant = transition.Instant;

            if (instant == Instant.MinValue)
            {
                Assert.IsNull(zone.PreviousTransition(instant));
            }
            else
            {
                Assert.AreEqual(transition, zone.NextTransition(instant - Duration.One));
                Assert.AreEqual(zone.PreviousTransition(instant),
                    zone.PreviousTransition(instant - Duration.One));
            }

            if (instant == Instant.MaxValue)
            {
                Assert.IsNull(zone.NextTransition(instant));
            }
            else
            {
                Assert.AreEqual(transition, zone.PreviousTransition(instant + Duration.One));
                Assert.AreEqual(zone.NextTransition(instant),
                    zone.NextTransition(instant + Duration.One));
            }

            return transition;
        }

    }
}
