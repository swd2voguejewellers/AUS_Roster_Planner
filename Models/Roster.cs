namespace ShiftPlanner.Models
{
    public class Roster
    {
        public int RosterId { get; set; }
        public DateTime WeekStart { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string DeletedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<RosterEntry> Entries { get; set; } = new();
    }
}
