using Microsoft.EntityFrameworkCore;
using ShiftPlanner.Models;

namespace ShiftPlanner
{
    public class UserDbContext :DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
    }
}
