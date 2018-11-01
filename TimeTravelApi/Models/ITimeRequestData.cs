using System.Collections.Generic;

namespace TimeTravelApi.Models
{
    public interface ITimeRequestData
    {
        void SaveChanges(MoreTimeRequestContext context);
        int NumTimeRequests(MoreTimeRequestContext context);
        void AddTimeRequest(MoreTimeRequestContext context, MoreTimeRequest moreTimeRequest);
        List<MoreTimeRequest> AllTimeRequests(MoreTimeRequestContext context);
        void UpdateTimeRequest(MoreTimeRequestContext context, MoreTimeRequest request);
    }
}