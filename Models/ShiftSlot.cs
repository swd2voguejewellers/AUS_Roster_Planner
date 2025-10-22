namespace ShiftPlanner.Models
{
    public class ShiftSlot
    {
        public string Day { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public int? StaffId { get; set; }
        public double Hours { get; set; }
    }
}
