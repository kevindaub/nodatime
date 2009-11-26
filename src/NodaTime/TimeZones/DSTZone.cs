﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NodaTime.TimeZones
{
    internal class DSTZone
        : IDateTimeZone
    {
        private readonly string id;

        internal Offset StandardOffset { get; set; }
        internal ZoneRecurrence StartRecurrence { get; set; }
        internal ZoneRecurrence EndRecurrence { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DSTZone"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="standardOffset">The standard offset.</param>
        /// <param name="startRecurrence">The start recurrence.</param>
        /// <param name="endRecurrence">The end recurrence.</param>
        internal DSTZone(String id, Offset standardOffset, ZoneRecurrence startRecurrence, ZoneRecurrence endRecurrence)
        {
            this.id = id;
            StandardOffset = standardOffset;
            StartRecurrence = startRecurrence;
            EndRecurrence = endRecurrence;

            if (startRecurrence.Name == endRecurrence.Name)
            {
                if (startRecurrence.Savings > Offset.Zero)
                {
                    startRecurrence = startRecurrence.RenameAppend("-Summer");
                }
                else
                {
                    endRecurrence = endRecurrence.RenameAppend("-Summer");
                }
            }

        }

        public bool equals(Object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is DSTZone)
            {
                DSTZone other = (DSTZone)obj;
                return
                    Id == other.Id &&
                    StandardOffset == other.StandardOffset &&
                    StartRecurrence.Equals(other.StartRecurrence) &&
                    EndRecurrence.Equals(other.EndRecurrence);
            }
            return false;
        }

        private ZoneRecurrence findMatchingRecurrence(Instant instant)
        {
            Instant start = StartRecurrence.Next(instant, StandardOffset, EndRecurrence.Savings);
            Instant end = EndRecurrence.Next(instant, StandardOffset, StartRecurrence.Savings);
            return (start > end) ? StartRecurrence : EndRecurrence;
        }


        #region IDateTimeZone Members

        public Instant? NextTransition(Instant instant)
        {
            Instant start = StartRecurrence.Next(instant, StandardOffset, EndRecurrence.Savings);
            Instant end = EndRecurrence.Next(instant, StandardOffset, StartRecurrence.Savings);
            return (start > end) ? end : start;
        }

        public Instant? PreviousTransition(Instant instant)
        {
            // Increment in order to handle the case where instant is exactly at
            // a transition.
            instant = instant + Duration.One;
            Instant start = StartRecurrence.Previous(instant, StandardOffset, EndRecurrence.Savings);
            Instant end = EndRecurrence.Previous(instant, StandardOffset, StartRecurrence.Savings);
            Instant result = (start > end) ? start : end;
            if (result != Instant.MinValue) {
                result = result - Duration.One;
            }
            return result;
        }

        public Offset GetOffsetFromUtc(Instant instant)
        {
            throw new NotSupportedException();
        }

        public Offset GetOffsetFromLocal(LocalInstant instant)
        {
            throw new NotSupportedException();
        }

        public string Name(Instant instant)
        {
            return findMatchingRecurrence(instant).Name;
        }

        public string Id
        {
            get { return this.id; }
        }

        public bool IsFixed
        {
            get { return false; }
        }

        #endregion
    }

}
