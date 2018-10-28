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
                alert = alertProcessor.HasTimeRequestJustExpired(mostRecentTimeRequest);

                if (alert)
                {
                    mostRecentTimeRequest.Expired = true;
                    _context.MoreTimeRequests.Update(mostRecentTimeRequest);
                    _context.SaveChanges();
                }
            }
            
            var newTime = DateTime.Now;
            var justExpiredTimeRequests = _context
                .MoreTimeRequests
                .ToList()
                .Where(x => alertProcessor.HasTimeRequestJustExpired(x));
            
            if (justExpiredTimeRequests.Count() > 0)
            {
                var earliestExpiredRequestStartTime = justExpiredTimeRequests
                    .Select(x => x.RequestTimeStamp)
                    .Min();
                newTime = earliestExpiredRequestStartTime;
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
                RequestTimeStamp = DateTime.Now,
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
