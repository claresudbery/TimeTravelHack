using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TimeTravelApi.Models;
using System;

namespace TimeTravelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoreTimeRequestController : ControllerBase
    {
        private static int accumulatedTimeDifference = 0;
        private readonly MoreTimeRequestContext _context;

        public MoreTimeRequestController(MoreTimeRequestContext context)
        {
            _context = context;

            if (_context.MoreTimeRequests.Count() == 0)
            {
                // Create a new MoreTimeRequest if collection is empty,
                // which means you can't delete all MoreTimeRequests.
                _context.MoreTimeRequests.Add(
                    new MoreTimeRequest { 
                        RequestTimeStamp = DateTime.Now,
                        Expired = false,
                        LengthInMinutes = TimeConstants.DefaultRequestLengthInMinutes
                    });
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult<List<MoreTimeRequest>> GetAll()
        {
            return _context.MoreTimeRequests.ToList();
        }

        [HttpGet("{userId}", Name = "GetAlert")]
        public ActionResult<TimeAndAlert> GetAlert(String userId)
        {
            var alertProcessor = new AlertProcessor();
            var mostRecentTimeRequest = _context
                .MoreTimeRequests
                .Where(x => x.UserId == userId)
                .LastOrDefault();

            var alert = false;
            if (mostRecentTimeRequest != null)
            {
                alert = alertProcessor.HasTimeRequestJustExpired(mostRecentTimeRequest, accumulatedTimeDifference);
            }
            
            var newTime = DateTime.Now.AddMinutes(-accumulatedTimeDifference);

            var justExpiredTimeRequests = _context
                .MoreTimeRequests
                .ToList()
                .Where(x => alertProcessor.HasTimeRequestJustExpired(x, accumulatedTimeDifference));
            
            if (justExpiredTimeRequests.Count() > 0)
            {
                var earliestExpiredRequestStartTime = justExpiredTimeRequests
                    .Select(x => x.RequestTimeStamp)
                    .Min();
                newTime = earliestExpiredRequestStartTime;

                var timeDifference = alertProcessor
                    .GetTimeDifferenceSinceRequest(earliestExpiredRequestStartTime, accumulatedTimeDifference);

                accumulatedTimeDifference += timeDifference;

                foreach (var request in justExpiredTimeRequests) 
                {
                    request.Expired = true;
                    _context.MoreTimeRequests.Update(request);
                }
                _context.SaveChanges();
            }

            return new TimeAndAlert {Alert = alert, 
                                    NewHours = newTime.TimeOfDay.Hours,
                                    NewMinutes = newTime.TimeOfDay.Minutes,
                                    NewSeconds = newTime.TimeOfDay.Seconds
                                    };
        }

        [HttpPost]
        public IActionResult Create(MoreTimeRequest newRequest)
        {
            var newItem = new MoreTimeRequest {
                RequestTimeStamp = DateTime.Now.AddMinutes(-accumulatedTimeDifference),
                Expired = false,
                LengthInMinutes = newRequest.LengthInMinutes,
                UserId = newRequest.UserId
            };
            _context.MoreTimeRequests.Add(newItem);
            _context.SaveChanges();

            return CreatedAtRoute(
                "GetAlert", 
                new { userId = newRequest.UserId }, 
                newItem);
        }
    }
}
