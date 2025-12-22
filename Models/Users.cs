using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShiftPlanner.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [StringLength(15)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(15)]
        public string Password { get; set; } = string.Empty;

        [StringLength(2)]
        public string Level { get; set; } = string.Empty ;

        public DateTime? CommentsDate { get; set; }

        [StringLength(15)]
        public string EnterPerson { get; set; } =string.Empty;

        public string Comment { get; set; } = string.Empty;

        public bool Off { get; set; }

        [StringLength(50)]
        public string Print_Auth { get; set; } = string.Empty;

        public int? Branch { get; set; }

        [StringLength(50)]
        public string Key_Word { get; set; } = string.Empty;

        public decimal? EMPNO { get; set; }

        public int? Authority { get; set; }

        [StringLength(20)]
        public string PreviousName { get; set; } = string.Empty;

        public int? Tab { get; set; }

        public int? OrderRelease { get; set; }

        [StringLength(10)]
        public string UserType { get; set; } = string.Empty;

        public int? Perf_View_Br { get; set; }
        public int? Sell_MC_Edit { get; set; }
        public int? Bulk_FGR_Edit { get; set; }
        public int? Order_Release { get; set; }
        public int? Early_Leaving { get; set; }
        public int? Perf_View_Des { get; set; }
    }
}
