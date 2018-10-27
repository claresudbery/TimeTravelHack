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
    }
}
