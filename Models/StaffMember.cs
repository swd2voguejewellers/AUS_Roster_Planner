namespace ShiftPlanner.Models
{
    public class StaffMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPermanent { get; set; }
        public bool IsManager { get; set; }
    }
}
