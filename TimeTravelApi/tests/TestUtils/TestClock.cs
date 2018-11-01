using System;
using TimeTravelApi.Models;

namespace TimeTravelApi.Tests.TestUtils
{
    public class TestClock : ITimeTravelClock
    {
        private DateTime _currentDateTime;

        public TestClock()
        {
            _currentDateTime = DateTime.Now;
        }

        public void SetDateTime(DateTime newDateTime)
        {
            _currentDateTime = newDateTime;
        }

        public DateTime Now
        {
            get { return _currentDateTime; }
        }
    }
}