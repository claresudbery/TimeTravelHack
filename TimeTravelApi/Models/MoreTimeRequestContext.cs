using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TimeTravelApi.Models
{
    public class MoreTimeRequestContext : DbContext
    {
        public MoreTimeRequestContext(DbContextOptions<MoreTimeRequestContext> options)
            : base(options)
        {
        }

        public DbSet<MoreTimeRequest> MoreTimeRequests { get; set; }

        public void RemoveAllTimeRequests()
        {
            foreach (var id in MoreTimeRequests.Select(e => e.Id))
            {
                var entity = new MoreTimeRequest { Id = id };
                MoreTimeRequests.Attach(entity);
                MoreTimeRequests.Remove(entity);
            }
            SaveChanges();
        }
    }
}
