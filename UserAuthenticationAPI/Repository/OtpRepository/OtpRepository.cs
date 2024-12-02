using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UserAuthenticationAPI.Repository.OtpRepository;
public class OtpRepository : Repository<OtpVerify>, IOtpRepository
{
    private readonly PetFriendsContext _context;

    public OtpRepository(PetFriendsContext context) : base(context)
    {
        _context = context;

    }

    public async Task<OtpVerify> GetOTPByUser(string otp, string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return null;
        }
        var otpverify = await _context.OtpVerifies
            .AsNoTracking()
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.OtpCode == otp && o.IsUsed == 1 && o.ExpiredAt > DateTime.UtcNow);
        if (otpverify == null)
        {
            return null;
        }

        if (user.Id != otpverify.User.Id)
        {
            return null;
        }
        return otpverify;
    }

    public Task<OtpVerify> GetOTPByUserId(Guid id)
    {
        var otp = _context.OtpVerifies.OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(o => o.UserId == id);
        return otp;
    }
}

