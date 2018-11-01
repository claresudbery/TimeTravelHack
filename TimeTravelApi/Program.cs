using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TimeTravelApi.Tests;

namespace TimeTravelApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var doTesting = true;
            if (doTesting)
            {
                runTests();
            }
            else
            {
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        private static void runTests()
        {
            runControllerTests();
        }

        private static void runControllerTests()
        {
            var controllerTests = new MoreTimeRequestControllerTests();
            controllerTests.OneTimeSetUp();

            controllerTests.Setup();
            controllerTests.GivenRequestExistsAndTimeIsUp_WhenGetAlertCalledByRequester_ThenAlertIsReturned();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
