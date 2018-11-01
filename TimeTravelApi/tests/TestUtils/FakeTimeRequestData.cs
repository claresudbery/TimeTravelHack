using System.Collections.Generic;
using System.Linq;
using TimeTravelApi.Models;

namespace TimeTravelApi.Tests.TestUtils
{
    public class FakeTimeRequestData : ITimeRequestData
    {
        private List<MoreTimeRequest> _moreTimeRequests;
        private int _idIncrementer = 0;

        public FakeTimeRequestData()
        {
            _moreTimeRequests = new List<MoreTimeRequest>();
        }

        public void SaveChanges(MoreTimeRequestContext context)
        {
            // No need to do anything because we are not using Entity Framework.
        }

        public int NumTimeRequests(MoreTimeRequestContext context)
        {
            return _moreTimeRequests.Count;
        }

        public void AddTimeRequest(MoreTimeRequestContext context, MoreTimeRequest moreTimeRequest)
        {
            _idIncrementer++;
            moreTimeRequest.Id = _idIncrementer;
            _moreTimeRequests.Add(moreTimeRequest);
        }

        public List<MoreTimeRequest> AllTimeRequests(MoreTimeRequestContext context)
        {
            return _moreTimeRequests;
        }

        public void UpdateTimeRequest(MoreTimeRequestContext context, MoreTimeRequest request)
        {
            var timeRequestToUpdate = _moreTimeRequests.Where(x => x.Id == request.Id).First();
            timeRequestToUpdate.Update(request);
        }

        public void RemoveAllTimeRequests()
        {
            _moreTimeRequests.Clear();
        }

        public MoreTimeRequest LastTimeRequest()
        {
            return _moreTimeRequests.Last();
        }
    }
}