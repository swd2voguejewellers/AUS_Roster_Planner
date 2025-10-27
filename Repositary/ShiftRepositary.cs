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
        public async Task<(bool IsValid, string Message)> SaveOrUpdateRosterAsync(RosterDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // -----------------------------
                // 🔹 VALIDATION RULES
                // -----------------------------

                var staffList = await _context.Staff.Select(s => new StaffDto
                {
                    EmployeeID = s.EmployeeID,
                    FirstName = s.FirstName,
                    IsPermanent = (s.IsPermanent ?? false),
                    IsManager = (s.IsManager ?? false)
                }).ToListAsync();
                var permanents = staffList.Where(s => s.IsPermanent == true).ToList();
                var casuals = staffList.Where(s => s.IsPermanent == false).ToList();
                var manager = staffList.FirstOrDefault(s => s.IsManager == true);

                if (permanents.Count != 4 || casuals.Count != 2 || manager == null)
                    return (false, "Staff configuration invalid: must have 4 permanent (incl. manager) and 2 casual staff.");

                // Group entries by staff
                var groupedByStaff = dto.Entries.GroupBy(e => e.StaffId);

                // 1️⃣ Validate permanent staff leave days
                foreach (var group in groupedByStaff)
                {
                    var staff = staffList.FirstOrDefault(s => s.EmployeeID == group.Key.ToString());
                    if (staff == null) continue;

                    if (staff.IsPermanent == true)
                    {
                        var leaveCount = group.Count(e => e.IsLeave);
                        if (leaveCount > 2)
                            return (false, $"Permanent staff {staff.FirstName} has more than 2 leave days.");

                        if (leaveCount < 2)
                            return (false, $"Permanent staff {staff.FirstName} has less than 2 leave days.");

                        var invalidWork = group.Any(e => !e.IsLeave && (e.FromTime == null || e.ToTime == null));
                        if (invalidWork)
                            return (false, $"Permanent staff {staff.FirstName} must have valid working hours or be on leave.");
                    }
                }

                // 2️⃣ Ensure at least 2 permanents at opening & closing
                var openCloseTimes = new Dictionary<string, (TimeSpan open, TimeSpan close)>
                {
                    ["Sunday"] = (TimeSpan.Parse("10:00"), TimeSpan.Parse("17:00")),
                    ["Monday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                    ["Tuesday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                    ["Wednesday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:30")),
                    ["Thursday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("21:00")),
                    ["Friday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("21:00")),
                    ["Saturday"] = (TimeSpan.Parse("09:00"), TimeSpan.Parse("17:00"))
                };

                foreach (var day in openCloseTimes)
                {
                    var dayEntries = dto.Entries.Where(e => e.DayName == day.Key && !e.IsLeave).ToList();

                    int openCount = dayEntries.Count(e =>
                        e.FromTime <= day.Value.open && e.ToTime >= day.Value.open &&
                        staffList.First(s => s.EmployeeID == e.StaffId.ToString()).IsPermanent == true);

                    int closeCount = dayEntries.Count(e =>
                        e.FromTime <= day.Value.close && e.ToTime >= day.Value.close &&
                        staffList.First(s => s.EmployeeID == e.StaffId.ToString()).IsPermanent == true);

                    if (openCount < 2 || closeCount < 2)
                        return (false, $"On {day.Key}, at least 2 permanent staff must be present at opening and closing.");
                }

                // 3️⃣ Validate total weekly hours
                var totalPermanentHours = groupedByStaff
                    .Where(g => staffList.First(s => s.EmployeeID == g.Key.ToString()).IsPermanent == true)
                    .Sum(g => g.Where(e => !e.IsLeave && e.FromTime != null && e.ToTime != null)
                               .Sum(e => (e.ToTime.Value - e.FromTime.Value).TotalHours));

                if (totalPermanentHours < 4 * 40) // 4 permanent staff * 40 hrs each
                    return (false, "Total permanent staff hours less than required (40h per staff).");

                // -----------------------------
                // 🔹 SAVE / UPDATE ROSTER
                // -----------------------------
                var existing = await _context.Rosters
                    .Include(r => r.Entries)
                    .FirstOrDefaultAsync(r => r.WeekStart.Date == dto.WeekStart.Date && !r.IsDeleted);

                if (existing != null)
                {
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.CreatedBy = dto.CreatedBy;
                    existing.DeletedBy = dto.DeletedBy;
                    existing.IsDeleted = dto.IsDeleted;

                    // Remove old entries
                    _context.RosterEntries.RemoveRange(existing.Entries);

                    // Add new entries
                    foreach (var entryDto in dto.Entries)
                    {
                        existing.Entries.Add(new RosterEntry
                        {
                            StaffId = entryDto.StaffId,
                            DayName = entryDto.DayName,
                            FromTime = entryDto.FromTime,
                            ToTime = entryDto.ToTime,
                            IsLeave = entryDto.IsLeave,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    var newRoster = new Roster
                    {
                        WeekStart = dto.WeekStart,
                        CreatedBy = dto.CreatedBy,
                        DeletedBy = dto.DeletedBy,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = dto.IsDeleted,
                        Entries = dto.Entries.Select(e => new RosterEntry
                        {
                            StaffId = e.StaffId,
                            DayName = e.DayName,
                            FromTime = e.FromTime,
                            ToTime = e.ToTime,
                            IsLeave = e.IsLeave,
                            CreatedAt = DateTime.UtcNow
                        }).ToList()
                    };

                    _context.Rosters.Add(newRoster);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "VALID");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[Roster Save Error] {ex.Message}");
                return (false, $"Error: {ex.Message}");
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
