using JavaJotter.Types;

namespace JavaJotter.Interfaces;

public interface IUsernameService
{
    public Task<List<Username>> GetAllUsers();
}