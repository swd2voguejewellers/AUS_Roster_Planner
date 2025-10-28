using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.ViewModels;
using ShiftPlanner.Interfaces;
using ShiftPlanner.Models;
using ShiftPlanner.Repositary;
using ClosedXML.Excel;
using System.Text;
using ShiftPlanner.DTO;
using ShiftPlanner.Helpers;

namespace ShiftPlanner.Controllers
{
    public class ShiftController : Controller
    {
        private readonly IShiftRepositary _shiftRepository;

        public ShiftController(IShiftRepositary shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        // ----------------------------
        //  API: Get staff list
        // ----------------------------
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

        // ----------------------------
        //  View: Week shift index
        // ----------------------------
        public IActionResult Index()
        {
            var vm = new WeekShiftViewModel
            {
                Slots = ShiftTimeConfig.Template.Select(t => new ShiftSlot
                {
                    Day = t.Day,
                    TimeRange = $"{t.From}-{t.To}",
                    Hours = t.Hours
                }).ToList()
            };

            return View(vm);
        }

        // ----------------------------
        //  API: Suggest automatic roster
        // ----------------------------
        [HttpGet("api/roster/load")]
        public async Task<IActionResult> GetRosterOrSuggestion([FromQuery] DateTime? weekStart)
        {
            var startOfWeek = weekStart ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

            var existingRoster = await _shiftRepository.GetRosterByWeekAsync(startOfWeek);

            if (existingRoster != null && existingRoster.Entries.Any())
            {
                return Ok(new
                {
                    type = "saved",
                    weekStart = existingRoster.WeekStart,
                    entries = existingRoster.Entries.Select(e => new
                    {
                        day = e.DayName,
                        staffId = e.StaffId,
                        from = e.FromTime?.ToString(@"hh\:mm") ?? "",
                        to = e.ToTime?.ToString(@"hh\:mm") ?? "",
                        isLeave = e.IsLeave
                    })
                });
            }

            // Fallback to suggested defaults
            var days = ShiftTimeConfig.Template.Select(t => new
            {
                day = t.Day,
                timeRange = $"{t.From}-{t.To}",
                hours = t.Hours,
                needCasuals = t.Hours >= 12
            }).ToList();

            var permanentLeave = new Dictionary<string, string[]>
            {
                { "Tami",    new[] { "Wednesday", "Sunday" } },
                { "Pathirage", new[] { "Monday", "Thursday" } },
                { "Kalani", new[] { "Tuesday", "Friday" } },
                { "Nimesha", new[] { "Saturday", "Sunday" } }
            };

            return Ok(new
            {
                type = "suggested",
                days,
                permanentLeave
            });
        }



        [HttpPost("api/roster/save")]
        public async Task<IActionResult> Save([FromBody] RosterDto dto)
        {
            if (dto == null || dto.Entries.Count == 0)
                return BadRequest("Invalid roster data.");

            var (isValid, message) = await _shiftRepository.SaveOrUpdateRosterAsync(dto);

            if (!isValid)
                return BadRequest(message);

            return Ok("Roster saved successfully!");
        }
    }
}
