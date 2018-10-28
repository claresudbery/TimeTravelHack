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
                        LengthInMinutes = 20
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
            var mostRecentTimeRequest = _context.MoreTimeRequests.Last();
            var alert = alertProcessor.HasTimeRequestExpired(mostRecentTimeRequest);

            if (alert)
            {
                mostRecentTimeRequest.Expired = true;
                _context.MoreTimeRequests.Update(mostRecentTimeRequest);
                _context.SaveChanges();
            }
            return new TimeAndAlert {Alert = alert, NewTime = DateTime.Now};
        }

        [HttpPost]
        public IActionResult Create(MoreTimeRequest newRequest)
        {
            var newItem = new MoreTimeRequest {
                RequestTimeStamp = DateTime.Now,
                Expired = false,
                LengthInMinutes = 20,
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
