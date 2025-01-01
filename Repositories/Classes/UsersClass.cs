using WMS_Application.Repositories.Interfaces;
using WMS_Application.Models;
using Microsoft.EntityFrameworkCore;
namespace WMS_Application.Repositories.Classes
{
    public class UsersClass : UsersInterface
    {
    private readonly dbMain _context;
    public UsersClass(dbMain context)
    {
        _context = context;
    }

    public async Task SaveUsers(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUsernameExists(string Username)
    {
        return await _context.Users.AnyAsync(x => x.Username == Username);
    }

    public async Task<bool> IsEmailExists(string Email)
    {
        return await _context.Users.AnyAsync(x => x.Email == Email);
    }
}
}
