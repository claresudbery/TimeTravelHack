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
        private TestClock _testClock;
        private ITimeTracker _timeTracker;
        private MoreTimeRequestContext _dbDummyContext;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _timeRequestData = new FakeTimeRequestData();
            _testClock = new TestClock();
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

        private void UserAlreadyAlerted(DateTime startTime, int requestLengthInMinutes, string userId)
        {
            var requestTime = startTime.AddMinutes(requestLengthInMinutes);
            _testClock.SetDateTime(requestTime);
            _controller.GetAlert(userId);
        }

        private void RequestAlreadyExpired(DateTime startTime, int requestLengthInMinutes, string userId)
        {
            var requestTime = startTime.AddMinutes(requestLengthInMinutes);
            _testClock.SetDateTime(requestTime);
            _controller.GetTime(userId);
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
            UserAlreadyAlerted(startTime, requestLengthInMinutes, userId);

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
            Assert.AreEqual(expectedTime.Second, timeAction.Value.NewSeconds);
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
            RequestAlreadyExpired(startTime, requestLengthInMinutes, userId);
            var currentTime = startTime.AddMinutes(requestLengthInMinutes + 10);

            // Act
            _testClock.SetDateTime(currentTime);
            ActionResult<TimeAndAlert> timeAction = _controller.GetTime(userId);

            // Assert
            var negativeRequestLength = -requestLengthInMinutes;
            var adjustedTime = currentTime.AddMinutes(negativeRequestLength);
            Assert.AreEqual(adjustedTime.Hour, timeAction.Value.NewHours);
            Assert.AreEqual(adjustedTime.Minute, timeAction.Value.NewMinutes);
            Assert.AreEqual(adjustedTime.Second, timeAction.Value.NewSeconds);
        }
    }
}
