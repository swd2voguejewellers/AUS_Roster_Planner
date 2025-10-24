using Microsoft.AspNetCore.Mvc;
using ShiftPlanner.ViewModels;
using ShiftPlanner.Interfaces;
using ShiftPlanner.Models;
using ShiftPlanner.Repositary;
using ClosedXML.Excel;
using System.Text;

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
                Slots = _template.Select(t => new ShiftSlot
                {
                    Day = t.Day,
                    TimeRange = $"{t.From}-{t.To}",
                    Hours = t.Hours
                }).ToList()
            };

            return View(vm);
        }

        // ----------------------------
        //  Template of default hours
        // ----------------------------
        private static readonly List<(string Day, string From, string To, double Hours)> _template = new()
        {
            ("Sunday",    "10:00", "17:00", 7.0),
            ("Monday",    "09:00", "17:30", 8.5),
            ("Tuesday",   "09:00", "17:30", 8.5),
            ("Wednesday", "09:00", "17:30", 8.5),
            ("Thursday",  "09:00", "21:00", 12.0),
            ("Friday",    "09:00", "21:00", 12.0),
            ("Saturday",  "09:00", "17:00", 8.0)
        };

        // ----------------------------
        //  API: Suggest automatic roster
        // ----------------------------
        [HttpGet("api/roster/suggestion")]
        public IActionResult GetRosterSuggestion()
        {
            var days = _template.Select(t => new
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
                days,
                permanentLeave
            });
        }

        [HttpPost("api/roster/save")]
        public async Task<IActionResult> Save([FromBody] Roster roster)
        {
            if (roster == null || roster.Entries.Count == 0)
                return BadRequest("Invalid roster data.");

            var success = await _shiftRepository.SaveOrUpdateRosterAsync(roster);
            if (success)
                return Ok(new { message = "Roster saved successfully." });
            else
                return StatusCode(500, "Failed to save roster.");
        }
    }
}
