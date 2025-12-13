// Services/InMemoryUserService.cs
using OrderTracker.Models;

namespace OrderTracker.Services;

public interface IUserService
{
    Task<AppUser?> GetByUsernameAsync(string username);
}

public class InMemoryUserService : IUserService
{
    private readonly Dictionary<string, AppUser> _users = new(StringComparer.OrdinalIgnoreCase);

    public void AddUser(AppUser user) => _users[user.Username] = user;

    public Task<AppUser?> GetByUsernameAsync(string username)
        => Task.FromResult(_users.GetValueOrDefault(username));
}