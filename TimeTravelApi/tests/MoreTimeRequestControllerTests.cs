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

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _timeRequestData = new FakeTimeRequestData();
            _testClock = new TestClock();
            _controller = new MoreTimeRequestController(
                new MoreTimeRequestContext(
                    new DbContextOptions<MoreTimeRequestContext>()),
                _timeRequestData,
                _testClock);
        }

        [SetUp]
        public void Setup()
        {
            _timeRequestData.RemoveAllTimeRequests();
        }

        private void CreateRequest(
            int requestLengthInMinutes,
            DateTime startTime,
            String userId)
        {
            var timeRequest = new TimeRequestModelBuilder()
                .WithLengthInMinutes(requestLengthInMinutes)
                .WithRequestTimeStamp(startTime)
                .WithUserId(userId)
                .Model();
            _testClock.SetDateTime(startTime);
            _controller.Create(timeRequest);
        }

        [Test]
        public void GivenRequestExistsAndTimeIsUp_WhenGetAlertCalledByRequester_ThenAlertIsReturned()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var startTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var userId = "User01";
            CreateRequest(requestLengthInMinutes, startTime, userId);
            var alertTime = startTime.AddMinutes(requestLengthInMinutes);

            // Act
            _testClock.SetDateTime(alertTime);
            ActionResult<TimeAndAlert> alertAction = _controller.GetAlert(userId);

            // Assert
            Assert.True(alertAction.Value.Alert);
        }
    }
}
