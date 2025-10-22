using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.ViewModels;
using ShiftPlanner.Interfaces;
using ShiftPlanner.Models;
using ClosedXML.Excel;
using System.Text;
using ShiftPlanner.Repositary;

namespace ShiftPlanner.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IShiftRepositary _shiftRepository;

        public ShiftController(IShiftRepositary shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        [HttpGet("api/staff")]
        public async Task<IActionResult> GetStaff()
        {
            try
            {
                var staffList = await _shiftRepository.GetStaffAsync();
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to load staff list.", detail = ex.Message });
            }
        }

        public IActionResult Index()
        {
            var vm = new WeekShiftViewModel
            {
                Slots = _template.Select(t => new ShiftSlot
                {
                    Day = t.day,
                    TimeRange = t.time,
                    Hours = t.hrs
                }).ToList()
            };
            return View(vm);
        }

        private static readonly List<(string day, string time, double hrs)> _template = new()
        {
            ("Sun","10:00-17:00",7.0),("Mon","09:00-17:30",8.5),("Tue","09:00-17:30",8.5),
            ("Wed","09:00-17:30",8.5),("Thu","09:00-21:00",12.0),("Fri","09:00-21:00",12.0),("Sat","09:00-17:00",8.0)
        };
    }
}
