---
layout: userguide
title: Migrating from 1.x to 2.0
category: core
weight: 1050
---

Noda Time 2.0 contains a number of breaking changes. If you have a project which uses Noda Time
1.x and are considering upgrading to 2.0, please read the following migration guide carefully.
In particular, there are some changes which are changes to execution-time behaviour, and won't show up as compile-time errors.

Obsolete members
====

A few members in 1.x were already marked as obsolete, and they have now been removed. Code using 
these members will no longer compile. Two of these were simple typos in the name - fixing code 
using these is simply a matter of using the correct name instead:

- `Era.AnnoMartyrm` should be `Era.AnnoMartyrum`
- `Period.FromMillseconds` should be `Period.FromMilliseconds`

In addition, `DateTimeZoneProviders.Default` has been removed. It wasn't the default in any Noda 
Time code, and it's clearer to use the `DateTimeZoneProviders.Tzdb` member, which the `Default`
member was equivalent to anyway.

Support for the resource-based time zone database format was removed in Noda Time 2.0. In terms
of the public API, this just meant removing three `TzdbDateTimeZoneSource` constructors, and
removing some documented edge cases where the legacy resource format didn't include as much
information as the more recent "nzd" format. If you were previously using the resource format,
just move to the "nzd" format, using the static factory members of `TzdbDateTimeZoneSource`.

Removed (or now private) members
====

The `Instant(long)` constructor is now private; use `Instant.FromTicksSinceUnixEpoch` instead.
As the resolution of 2.0 is nanoseconds, a constructor taking a number of *ticks* since the
Unix epoch is confusing. The static method is self-describing, and this allows the constructor
to be rewritten for use within Noda Time itself.

The `LocalTime.LocalDateTime` property has been removed. It was rarely a good idea to
arbitrarily pick the Unix epoch as the date, and usually indicates a broken design. If you
still need this behaviour, you can easily construct a `LocalDate` for the Unix epoch and use
the addition operator instead.

`CalendarSystem.GetMaxMonth` has been renamed to `GetMonthsInYear`, to match the equivalent
method in `System.Globalization.Calendar`.

`CenturyOfEra` and `YearOfCentury` have both been removed. We considered it unlikely that they
were being used, and the subtle differences between the Gregorian and ISO calendar systems were
almost certainly not helpful. Users who wish to compute the century and year of century in a
particular form can do so reasonably easily in their own code. With this change in place, the
distinction between the ISO calendar system and Gregorian-4 is only maintained for simplicity,
compatibility and consistency; the two calendars behave identically.

Period
====

The `Years`, `Months`, `Weeks` and `Days` properties of `Period` (and `PeriodBuilder`) are
now `int` rather than `long` properties. Any property for those units outside the range of `int` 
could never be added to a date anyway, as it would immediately go out of range. This change just
makes that clearer, and embraces the new "`int` for dates, `long` for times" approach which 
applies throughout Noda Time 2.0. The indexer for `PeriodBuilder` is still of type `long`, but 
it will throw an `ArgumentOutOfRangeException` for values outside the range of `int` when 
setting date-based units.

Normalization of a period which has time units which add up to a "days" range outside the range
of `int` will similarly fail.

Offset
====

In Noda Time 1.x, `Offset` was implemented as a number-of-milliseconds.
Sub-second time zone offsets aren't used in practice (modulo a _very_ small
number of historical cases), and in any case aren't supported by the TZDB or
BCL source data that we are able to use, so we supported more precision than
was useful.

In Noda Time 2.0, `Offset` is implemented as a number-of-seconds. This should
be mostly transparent, though `Offset.FromMilliseconds()` will now effectively
truncate to the whole number of seconds.  (Similarly, `Offset.FromTicks()` will
now truncate to the whole number of seconds rather than a whole number of
milliseconds.) The range of `Offset` is also reduced from +/- 23:59:59.999 to
+/- 18:00:00. The range reduction should have no practical consequence for real
situations, but test code which tried to use offsets between 18 and 24 hours
ahead of or behind UTC will need to be adjusted.

As a consequence of this change, offset formatting and parsing patterns no
longer support the `f` or `F` custom patterns, nor the `f` (full) standard
pattern.  Attempting to use these will generate an error, and attempting to
parse `Offset` (or `OffsetDateTime`) values containing fractional second
offsets will fail (though as mentioned above, these values do not tend to exist
in practice).

Note that binary serialization for `Offset` _is_ compatible with 1.x, other
than the value being truncated to a whole number of seconds: the serialized
form is still based on milliseconds. (Serialized data which stored values which
are now outside the range of valid values cannot be deserialized, however.)

Serialization
====

TBD (this will be awkward). To note so far:

- Periods with year/month/week/day values outside the range of `int`
- Offset truncation to seconds (see above)

Default values
====

The default values of some structs have changed, from returning the Unix epoch to returning January 1st 1AD (at midnight where applicable):

- `LocalDate`
- `LocalDateTime`
- `ZonedDateTime` (in UTC, as in 1.x)
- `OffsetDateTime` (with an offset of 0, as in 1.x)

(The fate of `Instant` remains unknown at this point...)

We recommend that you avoid relying on the default values at all - partly for the sake of clarity.

Text handling
====

The numeric standard patterns for `Instant` and `Offset` have been removed, with no direct equivalent.
These were not known to be useful, felt "alien" in various ways, and cause issues within the 
implementation. If you need these features - possibly in a specialized way - please contact the
mailing list and we may be able to suggest alternative implementations to meet your specific 
requirements.

Patterns no longer allow ASCII letters (a-z, A-Z) to act as literals when they're not escaped or quoted.
Quoting make the intention more explicit, and avoids unintended use of a literal when a specifier was
expected (e.g. a date pattern of "yyyy-mm-dd"). One exception here is 'T', which is allowed for date/time
patterns only - so a date/time pattern of "yyyy-MM-ddTHH:mm:ss" is still acceptable for ISO-8601, for example.
If this change breaks your code, simply escape or quote the literals within the pattern.
