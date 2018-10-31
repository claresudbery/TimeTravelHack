using System.Collections.Generic;

namespace TimeTravelApi.Models
{
    public interface ITimeRequestData
    {
        void SaveChanges();
        int NumTimeRequests();
        void AddTimeRequest(MoreTimeRequest moreTimeRequest);
        List<MoreTimeRequest> AllTimeRequests();
        void UpdateTimeRequest(MoreTimeRequest request);
    }
}