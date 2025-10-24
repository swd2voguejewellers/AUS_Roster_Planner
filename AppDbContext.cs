using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Models;

namespace ShiftPlanner
{
    public class AppDbContext :DbContext
    {
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Roster> Rosters { get; set; }
        public DbSet<RosterEntry> RosterEntries { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
