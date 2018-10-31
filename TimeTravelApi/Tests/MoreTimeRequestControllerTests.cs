using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using TimeTravelApi.Controllers;
using TimeTravelApi.Models;
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
            _controller = new MoreTimeRequestController(_timeRequestData, _testClock);
        }

        [SetUp]
        public void Setup()
        {
            _timeRequestData.RemoveAllTimeRequests();
        }

        [Test]
        public void GivenRequestExistsAndTimeIsUp_WhenGetAlertCalledByRequester_ThenAlertIsReturned()
        {
            // Arrange
            var requestLengthInMinutes = 30;
            var testTime = new DateTime(2018, 10, 31, 12, 0, 0);
            var alertTime = testTime.AddMinutes(requestLengthInMinutes);
            var userId = "User01";
            var timeRequest = new MoreTimeRequest()
            {
                Alerted = false,
                Expired = false,
                LengthInMinutes = requestLengthInMinutes,
                RequestTimeStamp = testTime,
                UserId = userId
            };
            _controller.Create(timeRequest);

            // Act
            _testClock.SetDateTime(alertTime);
            ActionResult<TimeAndAlert> alertAction = _controller.GetAlert(userId);

            // Assert
            Assert.True(alertAction.Value.Alert);
        }
    }
}
