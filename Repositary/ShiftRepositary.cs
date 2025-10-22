using Microsoft.EntityFrameworkCore;
using ShiftPlanner.DTO;
using ShiftPlanner.Interfaces;

namespace ShiftPlanner.Repositary
{
    public class ShiftRepositary : IShiftRepositary
    {
        private readonly AppDbContext _context;

        public ShiftRepositary(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StaffDto>> GetStaffAsync()
        {
            try
            {
                return await _context.Staff
                    .Select(s => new StaffDto
                    {
                        EmployeeID = s.EmployeeID,
                        FirstName = s.FirstName,
                        IsPermanent = s.IsPermanent,
                        IsManager = s.IsManager
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<StaffDto>();
            }
        }

    }
}
