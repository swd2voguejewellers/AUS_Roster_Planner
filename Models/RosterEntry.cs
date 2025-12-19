using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftPlanner.Models
{
    public class RosterEntry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RosterEntryId { get; set; }
        public int RosterId { get; set; }
        public int StaffId { get; set; }
        public string DayName { get; set; } = string.Empty;
        public DateTime RosterDate { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
        public bool IsLeave { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
