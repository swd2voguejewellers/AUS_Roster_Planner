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
        public async Task<bool> SaveOrUpdateRosterAsync(RosterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if a roster already exists for this week
                var existing = await _context.Rosters
                    .Include(r => r.Entries)
                    .FirstOrDefaultAsync(r => r.WeekStart.Date == dto.WeekStart.Date && !r.IsDeleted);

                if (existing != null)
                {
                    // --- UPDATE EXISTING ROSTER ---
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.CreatedBy = dto.CreatedBy;
                    existing.DeletedBy = dto.DeletedBy;
                    existing.IsDeleted = dto.IsDeleted;

                    // Remove old entries completely (you could also soft-delete instead)
                    _context.RosterEntries.RemoveRange(existing.Entries);

                    // Add new entries (map manually)
                    foreach (var entryDto in dto.Entries)
                    {
                        var newEntry = new RosterEntry
                        {
                            StaffId = entryDto.StaffId,
                            DayName = entryDto.DayName,
                            FromTime = entryDto.FromTime,
                            ToTime = entryDto.ToTime,
                            IsLeave = entryDto.IsLeave,
                            CreatedAt = DateTime.UtcNow
                        };
                        existing.Entries.Add(newEntry);
                    }
                }
                else
                {
                    // --- CREATE NEW ROSTER ---
                    var newRoster = new Roster
                    {
                        WeekStart = dto.WeekStart,
                        CreatedBy = dto.CreatedBy,
                        DeletedBy = dto.DeletedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = dto.IsDeleted,
                        Entries = new List<RosterEntry>()
                    };

                    // Map entries
                    foreach (var entryDto in dto.Entries)
                    {
                        var newEntry = new RosterEntry
                        {
                            StaffId = entryDto.StaffId,
                            DayName = entryDto.DayName,
                            FromTime = entryDto.FromTime,
                            ToTime = entryDto.ToTime,
                            IsLeave = entryDto.IsLeave,
                            CreatedAt = DateTime.UtcNow
                        };
                        newRoster.Entries.Add(newEntry);
                    }

                    _context.Rosters.Add(newRoster);
                }

                // Save all changes
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
