using AdminAuthenticationAPI.DTOs.AdminDTOs;
using AdminAuthenticationAPI.DTOs.ResultModelAdmin;

namespace AdminAuthenticationAPI.Service.AdminService
{
    public interface IAdminService
    {
        Task<ResultModelAdmin> LoginAdmin(AdminReqModel.AdminLoginReqModel adminLoginReqModel);
        Task<ResultModelAdmin> LoginWithGoogle(string googleToken);
    }
}
