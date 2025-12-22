using ShiftPlanner.Interfaces;
using ShiftPlanner.DTO;
using ShiftPlanner;
using Microsoft.EntityFrameworkCore;

public class LoginRepository : ILoginRepository
{
    private readonly UserDbContext _context;

    public LoginRepository(UserDbContext context)
    {
        _context = context;
    }

    public UserDTO? Authenticate(string username, string password)
    {
        try
        {
            return _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.UserName == username &&
                    u.Password == password)
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                    Level = u.Level,
                    UserType = u.UserType
                })
                .FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }


}
