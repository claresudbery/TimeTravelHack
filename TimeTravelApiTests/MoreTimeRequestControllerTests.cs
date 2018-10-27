using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeTravelApi.Controllers;

namespace TimeTravelApiTests
{
    [TestClass]
    public class MoreTimeRequestControllerTests
    {
        [TestMethod]
        public void GivenMoreTimeRequestedWhen20MinutesElapsedThenAlertHappens()
        {
            var controller = new MoreTimeRequestController();

        }
    }
}
