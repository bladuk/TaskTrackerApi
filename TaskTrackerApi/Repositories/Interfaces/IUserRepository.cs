using TaskTrackerApi.Models;

namespace TaskTrackerApi.Repositories.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
}