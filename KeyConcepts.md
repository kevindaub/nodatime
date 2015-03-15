# Introduction #

This page presents the main concepts used in the Noda Time project. Understanding these concept is mandatory for everyone :)

# Concepts #

Time is a tricky topic. It's important to understand what _isn't_ covered by Noda Time as well as what is.

## Stuff we don't do ##

Firstly, Noda Time is non-relativistic. Once you bring relativity into the equation, only maths/physics graduates can understand anything, and even then only after liberal amounts of coffee.

Secondly, Noda Time doesn't support [leap seconds](http://en.wikipedia.org/wiki/Leap_second). They complicate the model significantly, and aren't needed for the vast majority of applications.

## The time line ##

Instead, we treat time as a steady linear progression of ticks. (There are 10,000 ticks in a millisecond. This level of granularity was chosen for consistency with the .NET time-related types; Joda Time uses milliseconds.) The time line doesn't have the concept of months, centuries, dates and so forth - it's just a line.

There's one important event on the line - the Unix epoch. That's the "zero point" for Noda Time. Conceptually it's just a point on the line, but it so happens that the point is also known as midnight, January 1st 1970 UTC. It's an arbitrary choice - the important thing is that we've got to have **some** point on the time line to call 0.

## Humans make time messy ##

Leaving aside the tricky bits of astronomy and relativity, mankind has still made time hard to negotiate. If we all used ticks from the Unix epoch to talk about time, there wouldn't be a need for a library like Noda Time.

But no, we like to talk in years, months, days, weeks - and for some reason we like 12pm (which confusingly comes before 1pm) to be roughly the time at which the sun is highest... so we have _time zones_.

Not only that, but we don't all agree on how many months there are. Different civilizations have come up with different ways of splitting up the year, and different numbers for the years to start with. These are _calendar systems_.

# Types in Noda Time #

Noda Time takes these concepts and puts them into code as best it can.

## Instant ##

Represents an instant on the timeline, measured in ticks from the Unix epoch. It is a computer view of the time, represented by an ever increasing number.

## Interval ##

Represents an interval of time between a start `Instant` (inclusive) and an end Instant (exclusive).

## Duration ##

A length of time in ticks; a scientific amount of time. This does _not_ translate well into concepts such as "months" or even "days" - as both a month and a day can vary in length. So for example, it makes sense to talk about the duration of an Interval, but not of a `Period`.

## Offset ##

This specifies the difference between UTC and the local time in a particular time zone. This is always in the range (-24 hours, +24 hours), and has a granularity of milliseconds.

## CalendarSystem ##

A calendar system such as "the Gregorian calendar" or "the Coptic calendar." This does not inherently have a time zone associated with it. It is a way of breaking up time into months, weeks, days etc. Most applications will use the ISO 8601 calendar system - also just called the ISO calendar elsewhere for simplicity. This is the calendar system used by most Western countries, and is the default in Noda Time for any methods which have some overloads accepting a calendar and some not.

Noda Time doesn't currently support user-defined calendar systems, as that would require exposing more external details than we're comfortable with. If there is demand for a particular calendar system, we can implement it for everyone - and if this is a frequent requirement, we may consider opening it up further.

## LocalDateTime ##

A date and time in a particular calendar system. This does not represent an instant on the timeline, as it has not an associated time zone. For example, many countries held a 2 minute silence on November 11th 2009 at 11:00:00 (Gregorian/ISO calendar), but that meant different instants in different time zones.

## DateTimeZone ##

A [time zone](http://en.wikipedia.org/wiki/Time_zone) which can determine the offset at any instant between local time and UTC ([co-ordinated universal time](http://en.wikipedia.org/wiki/Coordinated_Universal_Time)), including the rules about if/when that offset changes due to [daylight saving time](http://en.wikipedia.org/wiki/Daylight_saving_time).

Noda Time uses time zones from the [Olson database](http://en.wikipedia.org/wiki/Zoneinfo) (also known as the `zoneinfo` of `tz` database). Time zone identifiers are names such as "Europe/London" or "America/Los\_Angeles". Noda Time will provide a mechanism for converting to the nearest known Windows time zone name, for use with .NET 3.5's [TimeZoneInfo](http://msdn.microsoft.com/en-us/library/system.timezoneinfo.aspx) class.

Unlike Joda Time and the built-in .NET date/time API, Noda Time does _not_ use the system default time zone unless you do so explicitly. You can access this using the `DateTimeZone.SystemDefault` property.

## ZonedDateTime ##

A `LocalDateTime` and a `DateTimeZone` - such as "November 11th 2009 at 11:00:00 in the Europe/London time zone".

It's possible to come up with ambiguous or impossible `ZonedDateTime` values. For example, October 25th 2009 01:30:00 in Europe/London occurred twice - because at 2am, the clocks went back as daylight saving time ended. Likewise March 28th 2010 01:30:00 in Europe/London won't exist at all - because we'll move straight from 00:59:59.999 to 02:00:00.000 as we put clocks forward.

Any `ZonedDateTime` represents a specific instant in time - so even in the ambiguous case, once you've successfully constructed a value you can distinguish between the two. This is important if you do further arithmetic, such as advancing the time by a day.

At the point where you construct a new `ZonedDateTime` or convert a `LocalDateTime` to a `ZonedDateTime`, you can specify a `TransitionResolver` which determines how gaps and ambiguous times are converted. Ambiguities are simple - the time can be rejected, treated as the earlier option, or treated as the later option. Gaps are more complicated as there is _no_ `ZonedDateTime` which will map to the requested `LocalDateTime`. The different strategies available allow you to pick from various "close" instants.

Currently it's not possible for users to define their own strategies for gap/ambiguity resolution; if you need a particular strategy which is currently unsupported, please ask.

## Period ##

A period is a length of time between local or zoned dates/times. These may vary depending on when it's applied. For example, "1 month" is longer when it's added to a date in June than when it's added to a date in February.

Periods are only relevant with respect to local dates and times; without reference to a particular calendar system, the concept of a month is meaningless.

# Links #

  * [Joda Time Quick Start](http://joda-time.sourceforge.net/quickstart.html)
  * https://jsr-310.dev.java.net/servlets/ProjectDocumentList