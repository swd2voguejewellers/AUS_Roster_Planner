using Microsoft.EntityFrameworkCore;

namespace ShiftPlanner.Models
{
    [Keyless]
    public class Staff
    {
        public string? EmployeeID { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }  
        public string? LastName { get; set; }
        public string? Title { get; set; }
        public string? Courtesy { get; set; }
        public string? Dept { get; set; }
        public short? Extension { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? IDNo { get; set; }
        public DateTime? HireDate { get; set; }
        public string? HomeAddress { get; set; }
        public string? HomePhone { get; set; }
        public string? MobilePhone { get; set; }
        public string? Notes { get; set; }
        public string NickName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Authorised { get; set; } = string.Empty;
        public decimal? Target { get; set; }
        public string? Keyword { get; set; }
        public int? EPF_NO { get; set; }
        public string? Current_Loc { get; set; }
        public string? Rank_For_Purchase { get; set; }
        public bool? IsPermanent { get; set; }
        public bool? IsManager { get; set; }
    }
}
