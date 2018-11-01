using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimeTravelApi.Models;

namespace TimeTravelApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // NB DbContext is scoped to each request by default, so be careful if using it in a singleton.
            services.AddDbContext<MoreTimeRequestContext>(opt =>
                opt.UseInMemoryDatabase("MoreTimeRequestList"));

            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<ITimeTravelClock, TimeTravelClock>();
            services.AddSingleton<ITimeRequestData, TimeRequestData>();
            services.AddSingleton<ITimeTracker, TimeTracker>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
