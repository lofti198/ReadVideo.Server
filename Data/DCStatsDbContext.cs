using Microsoft.EntityFrameworkCore;
using ReadVideo.Server.Controllers;

namespace ReadVideo.Server.Data
{

    public class CampaignLaunchStatsElement
    {
        public int Id { get; set; }  // This will serve as the primary key
        public string Email { get; set; }
        public string Domain { get; set; }
    }

    public class DCStatsDbContext : DbContext
    {
        public DCStatsDbContext(DbContextOptions<DCStatsDbContext> options) : base(options)
        {
        }

        public DbSet<CampaignLaunchStatsElement> CampaignLaunchStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CampaignLaunchStatsElement>()
                .HasIndex(p => new { p.Email, p.Domain })
                .IsUnique(true);
        }
    }

}
