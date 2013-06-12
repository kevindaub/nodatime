﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NodaTime.Calendars;
using NodaTime.Fields;
using NodaTime.Text;

namespace NodaTime.Test.Calendars
{
    [TestFixture]
    public class CalculatorCalendarSystemTest
    {
#pragma warning disable 0414 // Used by tests via reflection - do not remove!

        private static readonly CalendarSystem[] GregorianLikeOldCalendarSystems = 
        {
            CalendarSystem.GetGregorianCalendar(1),
            CalendarSystem.GetGregorianCalendar(2),
            CalendarSystem.GetGregorianCalendar(3),
            CalendarSystem.GetGregorianCalendar(4),
            CalendarSystem.GetGregorianCalendar(5),
            CalendarSystem.GetGregorianCalendar(6),
            CalendarSystem.GetGregorianCalendar(7),
            CalendarSystem.Iso
        };

        private static readonly CalendarSystem[] GregorianLikeNewCalendarSystems = 
        {
            new CalculatorCalendarSystem("new greg 1", "ignored", GregorianYearMonthDayCalculator.Instance, 1),
            new CalculatorCalendarSystem("new greg 2", "ignored", GregorianYearMonthDayCalculator.Instance, 2),
            new CalculatorCalendarSystem("new greg 3", "ignored", GregorianYearMonthDayCalculator.Instance, 3),
            new CalculatorCalendarSystem("new greg 4", "ignored", GregorianYearMonthDayCalculator.Instance, 4),
            new CalculatorCalendarSystem("new greg 5", "ignored", GregorianYearMonthDayCalculator.Instance, 5),
            new CalculatorCalendarSystem("new greg 6", "ignored", GregorianYearMonthDayCalculator.Instance, 6),
            new CalculatorCalendarSystem("new greg 7", "ignored", GregorianYearMonthDayCalculator.Instance, 7),
            new CalculatorCalendarSystem("new iso", "ignored", IsoYearMonthDayCalculator.Instance, 4),
        };

        private static readonly string[] GregorionLikeTestValues =
        {
            "2013-06-12T15:17:08.1234567",
            "-0500-06-12T00:52:59.1234567",
            "-1550-06-12T00:52:59.1234567",
            "1000-06-12T00:52:59.1234567",
            "1972-02-29T12:52:59.1234567",
        };

        private static readonly TestCaseData[] GregorianLikeCalendarTestData = 
            (from calendarPair in GregorianLikeOldCalendarSystems.Zip(GregorianLikeNewCalendarSystems, (Old, New) => new { Old, New })
             from property in GetSupportedProperties(calendarPair.New.Fields).Where(p => p.PropertyType == typeof(DateTimeField))
             from text in GregorionLikeTestValues
             select new TestCaseData(text, calendarPair.Old, calendarPair.New, property)
                           .SetName(text + ": " + calendarPair.Old.Id + " - " + property.Name)).ToArray();
#pragma warning restore 0414

        private static IEnumerable<PropertyInfo> GetSupportedProperties(FieldSet fields)
        {
            // This is horrible, but should return all supported fields of either type
            return typeof(FieldSet).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                   .Where(p => (bool)p.PropertyType.GetProperty("IsSupported", BindingFlags.Instance | BindingFlags.NonPublic)
                                                                    .GetValue(p.GetValue(fields, null), null));
        }

        [Test]
        [TestCaseSource("GregorianLikeCalendarTestData")]
        public void GregorianDateTimeFields(string text, CalendarSystem oldSystem, CalendarSystem newSystem, PropertyInfo property)
        {
            LocalInstant localInstant = LocalDateTimePattern.ExtendedIsoPattern.Parse(text).Value.LocalInstant;

            FieldSet originalFields = oldSystem.Fields;
            FieldSet newFields = newSystem.Fields;

            var isoField = (DateTimeField)property.GetValue(originalFields, null);
            var testField = (DateTimeField)property.GetValue(newFields, null);
            long expectedValue = isoField.GetInt64Value(localInstant);
            long actualValue = testField.GetInt64Value(localInstant);
            Assert.AreEqual(expectedValue, actualValue, "Error for property {0}", property.Name);
            if (expectedValue >= int.MinValue && expectedValue <= int.MaxValue)
            {
                Assert.AreEqual(isoField.GetInt64Value(localInstant), testField.GetInt64Value(localInstant));
            }
        }
    }

}
