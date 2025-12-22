using ShiftPlanner.DTO;

namespace ShiftPlanner.Interfaces
{
    public interface ILoginRepository
    {
        UserDTO? Authenticate(string username, string password);
    }
}
