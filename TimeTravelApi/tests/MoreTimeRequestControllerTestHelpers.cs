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

        private void CreateAndExpireTwoOverlappingTimeRequests(
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
            CreateRequestViaController(overlappingRequestLength, overlappingStartTime, overlappingUserId);
            // Now add "our" request
            var myUserId = "User02";
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

        private void CreateAndCheckTwoOverlappingTimeRequestsButOnlyExpireOneOfThem(
            String overlappingRequestStart,
            int overlappingRequestLength,
            String myRequestStart,
            int myRequestLength,
            int expectedTimeAdjustment)
        {
            // Arrange
            // Start by creating the "overlapping" request
            DateTime overlappingStartTime = GetDateTime(overlappingRequestStart);
            var overlappingUserId = "User01";
            CreateRequestViaController(overlappingRequestLength, overlappingStartTime, overlappingUserId);
            // Now add "our" request
            var myUserId = "User02";
            var myStartTime = GetDateTime(myRequestStart);
            CreateRequestViaController(myRequestLength, myStartTime, myUserId);
            // Expire the overlapping request
            ExpireRequest(overlappingStartTime, overlappingRequestLength, overlappingUserId);

            // Act
            // Now ask for the time before our request expires, but after the overlapping request expires.
            var requestTime = myStartTime.AddMinutes(myRequestLength - 1);
            _testClock.SetDateTime(requestTime);
            ActionResult<TimeAndAlert> returnedTimeAction = _controller.GetTime(myUserId);

            // Assert
            var negativeTimeAdjustment = expectedTimeAdjustment * -1;
            var expectedTime = _testClock.Now.AddMinutes(negativeTimeAdjustment);
            Assert.AreEqual(expectedTime.Hour, returnedTimeAction.Value.NewHours);
            Assert.AreEqual(expectedTime.Minute, returnedTimeAction.Value.NewMinutes);
        }

        private void CreateAndExpireRequest(
            String startTimeAsString,
            int requestLengthInMinutes,
            String userId)
        {
            DateTime startTime = GetDateTime(startTimeAsString);
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
            ExpireRequest(startTime, requestLengthInMinutes, userId);
        }

        private void CreateRequestViaController(
            String startTimeAsString,
            int requestLengthInMinutes,
            String userId)
        {
            DateTime startTime = GetDateTime(startTimeAsString);
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
        }

        // returns expected adjusted time adjustment at the current time based on what has expired so far.
        // see diagram "MultipleOverlappingTestRequests.jpg" in images folder for how these overlap.
        private int CreateMultipleExpiredAndUnexpiredOverlappingAndNonOverlappingRequests()
        {
            CreateRequestViaController("13:50", 120, "userId01");
            CreateAndExpireRequest("14:10", 70, "userId02");
            CreateAndExpireRequest("14:20", 40, "userId03");
            CreateAndExpireRequest("13:10", 80, "userId04");
            CreateAndExpireRequest("13:20", 50, "userId05");
            CreateAndExpireRequest("13:50", 20, "userId06");
            CreateAndExpireRequest("13:30", 10, "userId07");
            CreateAndExpireRequest("13:00", 60, "userId08");
            CreateAndExpireRequest("13:50", 50, "userId09");
            CreateAndExpireRequest("14:10", 20, "userId10");
            CreateRequestViaController("14:50", 50, "userId11");
            CreateRequestViaController("15:10", 30, "userId12");
            CreateAndExpireRequest("15:00", 10, "userId13");
            CreateRequestViaController("15:10", 20, "userId14");
            CreateRequestViaController("15:10", 40, "userId15");
            CreateAndExpireRequest("14:50", 20, "userId16");
            CreateAndExpireRequest("15:00", 20, "userId17");
            CreateAndExpireRequest("14:50", 30, "userId18");
            CreateRequestViaController("14:50", 30, "userId19");

            return 140;
        }
    }
}
