using Microsoft.EntityFrameworkCore;
using ShiftPlanner.DTO;
using ShiftPlanner.Interfaces;
using ShiftPlanner.Models;

namespace ShiftPlanner.Repositary
{
    public class ShiftRepositary : IShiftRepositary
    {
        private readonly AppDbContext _context;

        public ShiftRepositary(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StaffDto>> GetStaffAsync()
        {
            try
            {
                return await _context.Staff
                    .Select(s => new StaffDto
                    {
                        EmployeeID = s.EmployeeID,
                        FirstName = s.FirstName,
                        IsPermanent = s.IsPermanent,
                        IsManager = s.IsManager
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Staff Load Error] {ex.Message}");
                return Enumerable.Empty<StaffDto>();
            }
        }

        // =============================
        // Save or Update Roster
        // =============================
        public async Task<bool> SaveOrUpdateRosterAsync(Roster roster)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = await _context.Rosters
                    .Include(r => r.Entries)
                    .FirstOrDefaultAsync(r => r.WeekStart.Date == roster.WeekStart.Date && !r.IsDeleted);

                if (existing != null)
                {
                    // Soft delete old entries
                    foreach (var e in existing.Entries)
                    {
                        e.IsDeleted = true;
                        e.DeletedAt = DateTime.UtcNow;
                    }

                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    roster.RosterId = existing.RosterId;
                }
                else
                {
                    _context.Rosters.Add(roster);
                    await _context.SaveChangesAsync();
                }

                // Add new entries
                foreach (var entry in roster.Entries)
                {
                    entry.RosterId = roster.RosterId;
                    _context.RosterEntries.Add(entry);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[Roster Save Error] {ex.Message}");
                return false;
            }
        }

        public async Task<Roster?> GetRosterByWeekAsync(DateTime weekStart)
        {
            try
            {
                return await _context.Rosters
                    .Include(r => r.Entries.Where(e => !e.IsDeleted))
                    .FirstOrDefaultAsync(r => r.WeekStart == weekStart && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Roster Load Error] {ex.Message}");
                return null;
            }
        }

    }
}
