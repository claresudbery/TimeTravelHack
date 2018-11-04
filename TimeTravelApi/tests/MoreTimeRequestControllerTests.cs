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
    public class MoreTimeRequestControllerTests
    {
        private FakeTimeRequestData _timeRequestData;
        private MoreTimeRequestController _controller;
        private FakeClock _testClock;
        private ITimeTracker _timeTracker;
        private MoreTimeRequestContext _dbDummyContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _timeRequestData = new FakeTimeRequestData();
            _testClock = new FakeClock();
            _timeTracker = new TimeTracker();
            _dbDummyContext = new MoreTimeRequestContext(new DbContextOptions<MoreTimeRequestContext>());

            _controller = new MoreTimeRequestController(
                _dbDummyContext,
                _timeRequestData,
                _testClock,
                _timeTracker);
        }

        [SetUp]
        public void Setup()
        {
            _timeRequestData.RemoveAllTimeRequests();
            _timeTracker.AccumulatedTimeDifference = 0;
        }

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

        [TestCase(true, true, true, TestName = "TimeIsUp_CalledByRequester_AlertIsTrue")]
        [TestCase(false, true, false, TestName = "TimeIsNotUp_CalledByRequester_AlertIsFalse")]
        [TestCase(false, false, false, TestName = "TimeIsNotUp_CalledByOtherUser_AlertIsFalse")]
        [TestCase(true, false, false, TestName = "TimeIsUp_CalledByOtherUser_AlertIsFalse")]
        [Parallelizable(ParallelScope.None)]
        public void GivenRequestExists_WhenGetAlertCalled_ThenAlertIsOnlyReturnedWhenAppropriate(
            bool timeIsUp,
            bool calledByRequester,
            bool expectedAlertValue)
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
            var requestTimeDifference = timeIsUp ? requestLengthInMinutes : requestLengthInMinutes - 10;
            var requestTime = startTime.AddMinutes(requestTimeDifference);

            // Act
            _testClock.SetDateTime(requestTime);
            ActionResult<TimeAndAlert> alertAction = _controller.GetAlert(calledByRequester ? userId : "Some other user");

            // Assert
            Assert.AreEqual(expectedAlertValue, alertAction.Value.Alert);
        }

        [TestCase(true, TestName = "UserAlreadyAlerted_CalledByRequester_AlertIsFalse")]
        [TestCase(false, TestName = "UserAlreadyAlerted_CalledByOtherUser_AlertIsFalse")]
        [Parallelizable(ParallelScope.None)]
        public void GivenRequestExistsAndUserAlreadyAlerted_WhenGetAlertCalled_ThenAlertIsFalse(
            bool calledByRequester)
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
            AlertUser(startTime, requestLengthInMinutes, userId);

            // Act
            ActionResult<TimeAndAlert> alertAction = _controller.GetAlert(calledByRequester ? userId : "Some other user");

            // Assert
            Assert.AreEqual(false, alertAction.Value.Alert);
        }

        [TestCase(0, TimeType.RequestStartTime, TestName = "TimeIsUp_TimeRequested_TimeIsRequestStartTime")]
        [TestCase(-1, TimeType.CurrentTime, TestName = "TimeIsNotUp_TimeRequested_TimeIsCurrentTime")]
        [TestCase(10, TimeType.CurrentTimeMinusAccumulatedDifference, TestName = "TimeWasUpAWhileAgo_TimeRequested_TimeIsRequestStartTime")]
        [Parallelizable(ParallelScope.None)]
        public void GivenRequestExists_WhenTimeRequested_ThenCorrectTimeIsReturned(
            int endTimeOffsetInMinutes,
            TimeType expectedTimeType)
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
            var requestTime = startTime.AddMinutes(requestLengthInMinutes + endTimeOffsetInMinutes);

            // Act
            _testClock.SetDateTime(requestTime);
            ActionResult<TimeAndAlert> timeAction = _controller.GetTime(userId);

            // Assert
            DateTime expectedTime = GetExpectedTime(expectedTimeType, startTime, requestTime);
            Assert.AreEqual(expectedTime.Hour, timeAction.Value.NewHours);
            Assert.AreEqual(expectedTime.Minute, timeAction.Value.NewMinutes);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void GivenRequestHasAlreadyExpired_WhenTimeRequested_ThenCurrentTimeMinusRequestLengthReturned()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime, userId);
            ExpireRequest(startTime, requestLengthInMinutes, userId);
            var currentTime = startTime.AddMinutes(requestLengthInMinutes + 10);

            // Act
            _testClock.SetDateTime(currentTime);
            ActionResult<TimeAndAlert> timeAction = _controller.GetTime(userId);

            // Assert
            var negativeRequestLength = requestLengthInMinutes * -1;
            var adjustedTime = currentTime.AddMinutes(negativeRequestLength);
            Assert.AreEqual(adjustedTime.Hour, timeAction.Value.NewHours);
            Assert.AreEqual(adjustedTime.Minute, timeAction.Value.NewMinutes);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void GivenPreviousRequestsHaveNotExpired_WhenNewRequestMade_ThenPreviousUnexpiredRequestsWillNotResetTheClock()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime1 = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime1, userId);
            // Add a second request
            var startTime2 = startTime1.AddMinutes(1);
            CreateRequestViaController(requestLengthInMinutes, startTime2, userId);
            // Add a third request - this one should override the other two.
            var startTime3 = startTime1.AddMinutes(2);
            var shorterRequestLength = requestLengthInMinutes - 10;
            CreateRequestViaController(shorterRequestLength, startTime3, userId);
            // Get the third request to reset the clock.
            ExpireRequest(startTime3, shorterRequestLength, userId);

            // Act
            // Make a time request at the time each request's time is up, 
            // and add on extra time for the other two requests in case the 
            // accumulated time difference is messing things up.
            var firstExpirationTime = startTime1.AddMinutes(requestLengthInMinutes + requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(firstExpirationTime);
            ActionResult<TimeAndAlert> firstExpirationResult = _controller.GetTime(userId);
            // Second request
            var secondExpirationTime = startTime2.AddMinutes(requestLengthInMinutes + requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(secondExpirationTime);
            ActionResult<TimeAndAlert> secondExpirationResult = _controller.GetTime(userId);

            // Assert
            var negativeTimeAdjustment = shorterRequestLength * -1;
            // First time should only be adjusted by the length of the third request.
            var expectedFirstResult = firstExpirationTime.AddMinutes(negativeTimeAdjustment);
            Assert.AreEqual(expectedFirstResult.Hour, firstExpirationResult.Value.NewHours);
            Assert.AreEqual(expectedFirstResult.Minute, firstExpirationResult.Value.NewMinutes);
            // Second time should only be adjusted by the length of the third request.
            var expectedSecondResult = secondExpirationTime.AddMinutes(negativeTimeAdjustment);
            Assert.AreEqual(expectedSecondResult.Hour, secondExpirationResult.Value.NewHours);
            Assert.AreEqual(expectedSecondResult.Minute, secondExpirationResult.Value.NewMinutes);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void GivenPreviousRequestsHaveNotExpired_WhenNewRequestMade_ThenPreviousUnexpiredRequestsWillNotAlert()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime1 = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequestViaController(requestLengthInMinutes, startTime1, userId);
            // Add a second request
            var startTime2 = startTime1.AddMinutes(1);
            CreateRequestViaController(requestLengthInMinutes, startTime2, userId);
            // Add a third request - this one should override the other two.
            var startTime3 = startTime1.AddMinutes(2);
            var shorterRequestLength = requestLengthInMinutes - 10;
            CreateRequestViaController(shorterRequestLength, startTime3, userId);
            // Get the third request to alert the user.
            AlertUser(startTime3, shorterRequestLength, userId);

            // Act
            // Make a time request at the time each request's time is up, 
            // and add on extra time for the other two requests in case the 
            // accumulated time difference is messing things up.
            var firstExpirationTime = startTime1.AddMinutes(requestLengthInMinutes + requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(firstExpirationTime);
            ActionResult<TimeAndAlert> firstAlertResult = _controller.GetAlert(userId);
            // Second request
            var secondExpirationTime = startTime2.AddMinutes(requestLengthInMinutes + requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(secondExpirationTime);
            ActionResult<TimeAndAlert> secondAlertResult = _controller.GetAlert(userId);

            // Assert
            Assert.AreEqual(false, firstAlertResult.Value.Alert);
            Assert.AreEqual(false, secondAlertResult.Value.Alert);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void GivenOtherUserRequestHasNotExpired_WhenNewOverlappingRequestExpires_ThenPreviousRequestCanStillResetClockLater()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime1 = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId01 = "User01";
            var userId02 = "User02";
            CreateRequestViaController(requestLengthInMinutes, startTime1, userId01);
            // Add a second request from a different user
            var startTime2 = startTime1.AddMinutes(1);
            var shorterRequestLength = requestLengthInMinutes - 10;
            CreateRequestViaController(shorterRequestLength, startTime2, userId02);
            // Get the second request to reset the clock.
            ExpireRequest(startTime2, shorterRequestLength, userId02);

            // Act
            // Make a time request at the time the first request's time is up, 
            // and add on extra time for the other request in case the 
            // accumulated time difference is messing things up.
            var expirationTime = startTime1.AddMinutes(requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(expirationTime);
            ActionResult<TimeAndAlert> expirationResult = _controller.GetTime(userId01);

            // Assert
            var negativeTimeAdjustment = (requestLengthInMinutes) * -1;
            // Time should be adjusted by the length of the second request 
            // - shouldn't be affected by the other request.
            var expectedResult = expirationTime.AddMinutes(negativeTimeAdjustment);
            Assert.AreEqual(expectedResult.Hour, expirationResult.Value.NewHours);
            Assert.AreEqual(expectedResult.Minute, expirationResult.Value.NewMinutes);
        }

        [Test]
        [Parallelizable(ParallelScope.None)]
        public void GivenOtherUserRequestHasNotExpired_WhenNewUserMakesRequest_ThenOtherUserRequestWillStillAlert()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime1 = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId01 = "User01";
            var userId02 = "User02";
            CreateRequestViaController(requestLengthInMinutes, startTime1, userId01);
            // Add a second request from a different user
            var startTime2 = startTime1.AddMinutes(1);
            var shorterRequestLength = requestLengthInMinutes - 10;
            CreateRequestViaController(shorterRequestLength, startTime2, userId02);
            // Get the second request to alert.
            AlertUser(startTime2, shorterRequestLength, userId02);

            // Act
            // Make a time request at the time the first request's time is up, 
            // and add on extra time for the other request in case the 
            // accumulated time difference is messing things up.
            var expirationTime = startTime1.AddMinutes(requestLengthInMinutes + shorterRequestLength);
            _testClock.SetDateTime(expirationTime);
            ActionResult<TimeAndAlert> alertResult = _controller.GetAlert(userId01);;

            // Assert
            Assert.AreEqual(true, alertResult.Value.Alert);
        }

        [TestCase("10:00", 40, "10:20", 30, 40, false,
            "OverlappingRequestExpiredAndLongerAndStartedAndEndedBeforeMyStartAndEnd_TimeAdjustmentIsOverlappingRequestLength")]
        [TestCase("10:00", 40, "10:10", 30, 40, true,
            "OverlappingRequestExpiredAndLongerAndEndedAtMyEndAndIAskedFirst_TimeAdjustmentIsOverlappingRequestLength")]
        [TestCase("10:00", 40, "10:10", 30, 40, false,
            "OverlappingRequestExpiredAndLongerAndEndedAtMyEndAndTheyAskedFirst_TimeAdjustmentIsOverlappingRequestLength")]
        [Parallelizable(ParallelScope.None)]
        public void GivenOverlappingRequestIsExpiredAndLonger_WhenMyRequestExpires_ThenTimeAdjustmentIsOverlappingRequestLength(
            String overlappingRequestStart,
            int overlappingRequestLength,
            String myRequestStart,
            int myRequestLength,
            int expectedTimeAdjustment,
            bool iAskedFirst)
        {
            // Arrange
            // Start with the overlapping request
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

        [TestCase("10:00", 30, "10:20", 30, 30, false, "OverlappingRequestExpiredAndSameLengthAndStartedBeforeMyStart_TimeAdjustmentIsMyLength")]
        [TestCase("10:00", 30, "10:00", 30, 30, true, "OverlappingRequestExpiredAndSameLengthAndStartedAtMyStartAndIAskedFirst_TimeAdjustmentIsMyLength")]
        [TestCase("10:00", 30, "10:00", 30, 30, false, "OverlappingRequestExpiredAndSameLengthAndStartedAtMyStartAndTheyAskedFirst_TimeAdjustmentIsMyLength")]
        [Parallelizable(ParallelScope.None)]
        public void GivenOverlappingRequestIsExpiredAndSameLength_WhenMyRequestExpires_ThenTimeAdjustmentIsMyLength
        (String overlappingRequestStart,
            int overlappingRequestLength,
            String myRequestStart,
            int myRequestLength,
            int expectedTimeAdjustment,
            bool iAskedFirst)
        {
            // Arrange
            // Start with the overlapping request
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
