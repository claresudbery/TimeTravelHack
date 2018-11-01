using System.Collections.Generic;
using System.Linq;

namespace TimeTravelApi.Models
{
    public class TimeRequestData : ITimeRequestData
    {
        public void SaveChanges(MoreTimeRequestContext context)
        {
            context.SaveChanges();
        }

        public int NumTimeRequests(MoreTimeRequestContext context)
        {
            return context.MoreTimeRequests.Count();
        }

        public void AddTimeRequest(MoreTimeRequestContext context, MoreTimeRequest moreTimeRequest)
        {
            context.MoreTimeRequests.Add(moreTimeRequest);
        }

        public List<MoreTimeRequest> AllTimeRequests(MoreTimeRequestContext context)
        {
            return context.MoreTimeRequests.ToList();
        }

        public void UpdateTimeRequest(MoreTimeRequestContext context, MoreTimeRequest request)
        {
            context.MoreTimeRequests.Update(request);
        }
    }
}