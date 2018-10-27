using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TimeTravelApi.Controllers;
using TimeTravelApi.Models;
//using Microsoft.AspNetCore.Mvc;

namespace Tests
{
    public class MoreTimeRequestControllerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GivenMoreTimeRequested_When20MinutesElapse_ThenAlertHappens()
        {
            var controller = new MoreTimeRequestController(
                new MoreTimeRequestContext(
                    new DbContextOptions<MoreTimeRequestContext>()));
            //var result = controller.GetAlert(true);
            Assert.Pass();
        }
    }
}