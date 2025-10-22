using ShiftPlanner.DTO;
using ShiftPlanner.Models;
using System.Collections.Generic;

namespace ShiftPlanner.ViewModels
{
    public class WeekShiftViewModel
    {
        public List<StaffDto> Staff { get; set; } = new();
        public List<ShiftSlot> Slots { get; set; } = new();
        public Dictionary<int,double> TotalHours { get; set; } = new();
        public Dictionary<int,double> OvertimeHours { get; set; } = new();
    }
}
