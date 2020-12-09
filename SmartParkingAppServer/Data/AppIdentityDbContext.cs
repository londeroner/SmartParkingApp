using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartParkingAppServer.Models;

namespace SmartParkingAppServer.Data
{
    public class AppIdentityDbContext : IdentityDbContext<ParkingUser, PkUserIdentityRole, long>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Creating roles
            long idVal = 0;
            builder.Entity<PkUserIdentityRole>().HasData(
                new { Id = idVal + 1, Name = "Owner", NormalizedName = "OWNER" },
                new { Id = idVal + 2, Name = "CommonUser", NormalizedName = "COMMONUSER" }
                );

            builder.Entity<Tariff>().HasData(
                new Tariff { TariffId = 1, Minutes = 15, Rate = 0 },
                new Tariff { TariffId = 2, Minutes = 60, Rate = 50 },
                new Tariff { TariffId = 3, Minutes = 120, Rate = 100 },
                new Tariff { TariffId = 4, Minutes = 180, Rate = 140 },
                new Tariff { TariffId = 5, Minutes = 240, Rate = 180 },
                new Tariff { TariffId = 6, Minutes = 300, Rate = 200 }
                );
        }

        public DbSet<RefreshTokenModel> RefreshTokenModels { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<PastSession> PastSessions { get; set; }
        public DbSet<ActiveSession> ActiveSessions { get; set; }
    }
}
