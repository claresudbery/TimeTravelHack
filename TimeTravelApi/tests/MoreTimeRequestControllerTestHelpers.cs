using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TimeTravelApi.Controllers;
using TimeTravelApi.Models;
using TimeTravelApi.Tests.ModelBuilders;
using TimeTravelApi.Tests.TestUtils;

namespace TimeTravelApi.Tests
{
    [TestFixture]
    public partial class MoreTimeRequestControllerTests
    {
        // !!! This is a partial class! All the actual tests and member variables and setup methods are in a separate file,
        // for ease of navigation.

        private void CreateRequestViaController(
            int requestLengthInMinutes,
            DateTime startTime,
            String userId)
        {
            var timeRequest = new TimeRequestModelBuilder()
                .WithLengthInMinutes(requestLengthInMinutes)
                .WithUserId(userId)
                .Model();
            _testClock.SetDateTime(startTime);
            _controller.Create(timeRequest);
        }

        private void CreateRequestInternally(
            int requestLengthInMinutes,
            DateTime startTime,
            String userId,
            bool alerted)
        {
            var timeRequest = new TimeRequestModelBuilder()
                .WithLengthInMinutes(requestLengthInMinutes)
                .WithRequestTimeStamp(startTime)
                .WithUserId(userId)
                .WithAlerted(alerted)
                .Model();
            _testClock.SetDateTime(startTime);
            _timeRequestData.AddTimeRequest(_dbDummyContext, timeRequest);
        }

        private bool AlertUser(DateTime startTime, int requestLengthInMinutes, string userId)
        {
            var requestTime = startTime.AddMinutes(requestLengthInMinutes);
            _testClock.SetDateTime(requestTime);
            ActionResult<TimeAndAlert> alertAction = _controller.GetAlert(userId);
            return alertAction.Value.Alert;
        }

        private DateTime ExpireRequest(DateTime startTime, int requestLengthInMinutes, string userId)
        {
            var requestTime = startTime.AddMinutes(requestLengthInMinutes);
            _testClock.SetDateTime(requestTime);
            ActionResult<TimeAndAlert> timeAction = _controller.GetTime(userId);
            return new DateTime(
                2018, 10, 31,
                timeAction.Value.NewHours,
                timeAction.Value.NewMinutes,
                timeAction.Value.NewSeconds);
        }

        private DateTime GetExpectedTime(
            TimeType expectedTimeType,
            DateTime startTime,
            DateTime requestTime)
        {
            DateTime expectedTime = DateTime.Now;
            var negativeTimeDifference = _timeTracker.AccumulatedTimeDifference * -1;
            switch (expectedTimeType)
            {
                case TimeType.CurrentTime: expectedTime = requestTime; break;
                case TimeType.RequestStartTime: expectedTime = startTime; break;
                case TimeType.CurrentTimeMinusAccumulatedDifference:
                    expectedTime = requestTime.AddMinutes(negativeTimeDifference); break;
            }
            return expectedTime;
        }

        private DateTime GetDateTime(string requestTimeAsString)
        {
            int hour = Convert.ToInt32(requestTimeAsString.Substring(0, 2));
            int minute = Convert.ToInt32(requestTimeAsString.Substring(3, 2));
            return new DateTime(2018, 10, 31, hour, minute, 0);
        }

        private void CreateAndCheckTwoOverlappingTimeRequests(
            String overlappingRequestStart,
            int overlappingRequestLength,
            String myRequestStart,
            int myRequestLength,
            int expectedTimeAdjustment,
            bool iAskedFirst)
        {
            // Arrange
            // Start by creating the "overlapping" request
            DateTime overlappingStartTime = GetDateTime(overlappingRequestStart);
            var overlappingUserId = "User01";
            var myUserId = "User02";
            CreateRequestViaController(overlappingRequestLength, overlappingStartTime, overlappingUserId);
            // Now add "our" request
            var myStartTime = GetDateTime(myRequestStart);
            CreateRequestViaController(myRequestLength, myStartTime, myUserId);

            // Act
            // Get each user to ask for the time.
            DateTime newTime;
            if (iAskedFirst)
            {
                newTime = ExpireRequest(myStartTime, myRequestLength, myUserId);
                ExpireRequest(overlappingStartTime, overlappingRequestLength, overlappingUserId);
            }
            else
            {
                ExpireRequest(overlappingStartTime, overlappingRequestLength, overlappingUserId);
                newTime = ExpireRequest(myStartTime, myRequestLength, myUserId);
            }

            // Assert
            var negativeTimeAdjustment = expectedTimeAdjustment * -1;
            var expectedTime = _testClock.Now.AddMinutes(negativeTimeAdjustment);
            Assert.AreEqual(expectedTime.Hour, expectedTime.Hour);
            Assert.AreEqual(expectedTime.Minute, expectedTime.Minute);
        }
    }
}
