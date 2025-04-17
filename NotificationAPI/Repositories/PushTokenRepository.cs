using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

public class PushTokenRepository : IPushTokenRepository
{
    private readonly PetfriendsContext _context;
    public PushTokenRepository(PetfriendsContext context)
    {
        _context = context;
    }
    public async Task<UserPushToken> GetByUserId(Guid userId)
    {
        return await _context.UserPushTokens
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<UserPushToken> GetByToken(string token)
    {
        return await _context.UserPushTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<List<UserPushToken>> GetAllByUserId(Guid userId)
    {
        return await _context.UserPushTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> SaveToken(Guid userId, string token, string deviceType)
    {
        try
        {
            var existingToken = await _context.UserPushTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (existingToken != null)
            {
                // Cập nhật token nếu đã tồn tại
                existingToken.UserId = userId;
                existingToken.DeviceType = deviceType;
                existingToken.UpdatedAt = DateTime.UtcNow;
                _context.UserPushTokens.Update(existingToken);
            }
            else
            {
                // Tạo mới nếu chưa tồn tại
                var newToken = new UserPushToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = token,
                    DeviceType = deviceType,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserPushTokens.AddAsync(newToken);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteToken(string token)
    {
        try
        {
            var existingToken = await GetByToken(token);

            if (existingToken != null)
            {
                _context.UserPushTokens.Remove(existingToken);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
