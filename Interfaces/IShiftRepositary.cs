using ShiftPlanner.DTO;
using ShiftPlanner.Models;
using ShiftPlanner.ViewModels;

namespace ShiftPlanner.Interfaces
{
    public interface IShiftRepositary
    {
        Task<IEnumerable<StaffDto>> GetStaffAsync();
        Task<bool> SaveOrUpdateRosterAsync(Roster roster);

        Task<Roster?> GetRosterByWeekAsync(DateTime weekStart);
    }
}
