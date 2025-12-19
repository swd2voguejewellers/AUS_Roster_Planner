using ShiftPlanner.DTO;
using ShiftPlanner.Models;
using ShiftPlanner.ViewModels;

namespace ShiftPlanner.Interfaces
{
    public interface IShiftRepositary
    {
        Task<IEnumerable<StaffDto>> GetStaffAsync();
        Task<(bool IsValid, string Message)> SaveOrUpdateRosterAsync(RosterDto dto);

        Task<Roster?> GetRosterByWeekAsync(DateTime weekStart);
        Task<List<Staff>> GetPermanentStaffAsync();
    }
}
