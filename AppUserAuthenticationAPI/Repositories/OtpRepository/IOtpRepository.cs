using DataAccess.Models;
using DataAccess.Repositories;

namespace AppUserAuthenticationAPI.Repository.OtpRepository;
public interface IOtpRepository : IRepository<OtpVerify>
{
    Task<OtpVerify> GetOTPByUser(string otp, string email);
    Task<OtpVerify> GetOTPByUserId(Guid id);
}