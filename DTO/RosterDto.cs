using ShiftPlanner.Models;

namespace ShiftPlanner.DTO
{
    public class RosterDto
    {
        public DateTime WeekStart { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string DeletedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public List<RosterEntryDto> Entries { get; set; } = new();
    }
}
