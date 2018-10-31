using System.Collections.Generic;
using System.Linq;

namespace TimeTravelApi.Models
{
    public class TimeRequestData : ITimeRequestData
    {
        private readonly MoreTimeRequestContext _context;

        public TimeRequestData(MoreTimeRequestContext context)
        {
            _context = context;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public int NumTimeRequests()
        {
            return _context.MoreTimeRequests.Count();
        }

        public void AddTimeRequest(MoreTimeRequest moreTimeRequest)
        {
            _context.MoreTimeRequests.Add(moreTimeRequest);
        }

        public List<MoreTimeRequest> AllTimeRequests()
        {
            return _context.MoreTimeRequests.ToList();
        }

        public void UpdateTimeRequest(MoreTimeRequest request)
        {
            _context.MoreTimeRequests.Update(request);
        }
    }
}