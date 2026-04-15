using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.Repositories.Abstractions;
using TaskTrackerApi.Repositories.Interfaces;

namespace TaskTrackerApi.Repositories;

public class UserRepository(TaskTrackerDbContext context) : RepositoryBase<User, Guid>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await Entities.FirstOrDefaultAsync(u => u.Username == username, ct);
}