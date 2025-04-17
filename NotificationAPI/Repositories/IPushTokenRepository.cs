using DataAccess.Models;

public interface IPushTokenRepository
{
    Task<UserPushToken> GetByUserId(Guid userId);
    Task<UserPushToken> GetByToken(string token);
    Task<List<UserPushToken>> GetAllByUserId(Guid userId);
    Task<bool> SaveToken(Guid userId, string token, string deviceType);
    Task<bool> DeleteToken(string token);
}