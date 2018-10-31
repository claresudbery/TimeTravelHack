using System.Collections.Generic;
using System.Linq;
using TimeTravelApi.Models;

namespace TimeTravelApi.Tests.TestUtils
{
    public class FakeTimeRequestData : ITimeRequestData
    {
        private List<MoreTimeRequest> _moreTimeRequests;

        public FakeTimeRequestData()
        {
            _moreTimeRequests = new List<MoreTimeRequest>();
        }

        public void SaveChanges()
        {
            // No need to do anything because we are not using Entity Framework.
        }

        public int NumTimeRequests()
        {
            return _moreTimeRequests.Count;
        }

        public void AddTimeRequest(MoreTimeRequest moreTimeRequest)
        {
            _moreTimeRequests.Add(moreTimeRequest);
        }

        public List<MoreTimeRequest> AllTimeRequests()
        {
            return _moreTimeRequests;
        }

        public void UpdateTimeRequest(MoreTimeRequest request)
        {
            var timeRequestToUpdate = _moreTimeRequests.Where(x => x.Id == request.Id).First();
            timeRequestToUpdate.Update(request);
        }

        public void RemoveAllTimeRequests()
        {
            _moreTimeRequests.Clear();
        }
    }
}