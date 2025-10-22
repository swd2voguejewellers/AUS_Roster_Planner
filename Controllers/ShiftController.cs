using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.Models;
using ShiftPlanner.ViewModels;
using ClosedXML.Excel;
using System.Text;

namespace ShiftPlanner.Controllers
{
    public class ShiftController : Controller
    {
        private static readonly List<StaffMember> _staff = new()
        {
            new StaffMember{Id=1, Name="Alice (Manager)", IsPermanent=true, IsManager=true},
            new StaffMember{Id=2, Name="Bob", IsPermanent=true},
            new StaffMember{Id=3, Name="Carol", IsPermanent=true},
            new StaffMember{Id=4, Name="Dave", IsPermanent=true},
            new StaffMember{Id=5, Name="Eve (Casual)", IsPermanent=false},
            new StaffMember{Id=6, Name="Frank (Casual)", IsPermanent=false}
        };

        private static readonly List<(string day, string time, double hrs)> _template = new()
        {
            ("Sun","10:00-17:00",7.0),("Mon","09:00-17:30",8.5),("Tue","09:00-17:30",8.5),
            ("Wed","09:00-17:30",8.5),("Thu","09:00-21:00",12.0),("Fri","09:00-21:00",12.0),("Sat","09:00-17:00",8.0)
        };

        public IActionResult Index()
        {
            var vm = new WeekShiftViewModel
            {
                Staff = _staff,
                Slots = _template.Select(t => new ShiftSlot{Day=t.day, TimeRange=t.time, Hours=t.hrs}).ToList()
            };
            ComputeTotals(vm);
            return View(vm);
        }

        [HttpPost]
        public IActionResult Save(WeekShiftViewModel vm)
        {
            // basic validation
            var errors = new List<string>();
            foreach(var slot in vm.Slots)
            {
                if (slot.StaffId != null && !_staff.Any(s => s.Id == slot.StaffId))
                    errors.Add($"Invalid staff id {slot.StaffId} on {slot.Day}");
            }
            if(errors.Any())
            {
                TempData["Errors"] = string.Join("; ", errors);
            }
            else
            {
                TempData["Message"] = "Shift saved in-memory.";
            }
            vm.Staff = _staff;
            ComputeTotals(vm);
            return View("Index", vm);
        }

        [HttpPost]
        public IActionResult ExportCsv(WeekShiftViewModel vm)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Day,TimeRange,StaffName,Hours");
            foreach(var slot in vm.Slots)
            {
                var name = _staff.FirstOrDefault(s => s.Id == slot.StaffId)?.Name ?? "(Unassigned)";
                sb.AppendLine($"{slot.Day},{slot.TimeRange},{name},{slot.Hours}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "WeekRoster.csv");
        }

        [HttpPost]
        public IActionResult ExportExcel(WeekShiftViewModel vm)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Week Roster");
            ws.Cell(1,1).Value = "Day";
            ws.Cell(1,2).Value = "TimeRange";
            ws.Cell(1,3).Value = "StaffName";
            ws.Cell(1,4).Value = "Hours";
            int r = 2;
            foreach(var slot in vm.Slots)
            {
                var name = _staff.FirstOrDefault(s=>s.Id==slot.StaffId)?.Name ?? "(Unassigned)";
                ws.Cell(r,1).Value = slot.Day;
                ws.Cell(r,2).Value = slot.TimeRange;
                ws.Cell(r,3).Value = name;
                ws.Cell(r,4).Value = slot.Hours;
                r++;
            }
            // summary
            ws.Cell(r+1,1).Value = "Summary";
            ws.Cell(r+1,1).Style.Font.Bold = true;
            int cr = r+2;
            foreach(var s in _staff)
            {
                vm.TotalHours.TryGetValue(s.Id, out double hrs);
                vm.OvertimeHours.TryGetValue(s.Id, out double ot);
                ws.Cell(cr,1).Value = s.Name;
                ws.Cell(cr,2).Value = hrs;
                ws.Cell(cr,3).Value = ot;
                cr++;
            }

            using var ms = new System.IO.MemoryStream();
            wb.SaveAs(ms);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WeekRoster.xlsx");
        }

        private void ComputeTotals(WeekShiftViewModel vm)
        {
            vm.TotalHours = new Dictionary<int,double>();
            vm.OvertimeHours = new Dictionary<int,double>();
            foreach(var s in _staff) { vm.TotalHours[s.Id] = 0; vm.OvertimeHours[s.Id] = 0; }
            foreach(var slot in vm.Slots)
            {
                if(slot.StaffId == null) continue;
                vm.TotalHours[slot.StaffId.Value] += slot.Hours;
            }
            // compute overtime vs 40 hours
            foreach(var id in vm.TotalHours.Keys.ToList())
            {
                var hrs = vm.TotalHours[id];
                var ot = hrs > 40 ? hrs - 40 : 0;
                vm.OvertimeHours[id] = ot;
            }
        }
    }
}
