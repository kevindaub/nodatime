﻿using System.Globalization;
using NUnit.Framework;

namespace NodaTime.Demo
{
    [TestFixture]
    public class LocalTimeDemo
    {
        [Test]
        public void Construction()
        {
            LocalTime time = new LocalTime(16, 20, 0);
            Assert.AreEqual("16:20:00", time.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
