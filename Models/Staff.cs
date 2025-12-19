using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShiftPlanner.Models
{
    [Keyless]
    public class Staff
    {
        public int EmployeeID { get; set; }

        public string? FirstName { get; set; }

        [Column("Middle Name")]
        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? FullName { get; set; }

        public string? Title { get; set; }

        public string? Courtesy { get; set; }

        public string? Dept { get; set; }

        public string? Extension { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IDNo { get; set; }

        public DateTime? HireDate { get; set; }

        public string? HomeAddress { get; set; }

        public string? ResidentialAddress { get; set; }

        public string? HomePhone { get; set; }

        public string? MobilePhone { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string NickName { get; set; } = null!;

        [Required]
        public string Status { get; set; } = null!;

        [Required]
        public string Authorised { get; set; } = null!;

        public decimal? Target { get; set; }

        public string? Current_Loc { get; set; }

        public string? Branch { get; set; }

        public string? Relegion { get; set; }

        public string? Gender { get; set; }

        public DateTime? Resign_Date { get; set; }

        public string? DoorAccess { get; set; }

        public string? OtCategory { get; set; }

        public int? HOD { get; set; }

        public int? ASSTMANGER { get; set; }

        public string? Supervisor_Id { get; set; }

        public string? Supervisor_Title { get; set; }

        public string? FoodRefer { get; set; }

        public string? Category { get; set; }

        public string? CraftsmanW { get; set; }

        public string? SubDept { get; set; }

        public int? StaffPurchase { get; set; }

        public string? PurchaseLimitRank { get; set; }

        public DateTime? Contract_Start_Date { get; set; }

        public int? Contract_Period { get; set; }

        public DateTime? Contract_Renewal_Date { get; set; }

        public string? PersName_Emerg { get; set; }

        public string? ContNo_Emerg { get; set; }

        public string? Relship_Emerg { get; set; }
    }
}
