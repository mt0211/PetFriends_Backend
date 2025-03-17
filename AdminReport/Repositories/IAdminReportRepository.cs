using DataAccess.Models;

namespace AdminReport.Repositories
{
    public interface IAdminReportRepository
    {
        Task<IEnumerable<dynamic>> GetDataUserStatus(int year, int? month);
        Task<IEnumerable<dynamic>> GetDataNewUser(int year, int? month);
        Task<IEnumerable<dynamic>> GetPostDistribution();
        Task<User> GetUserByID(Guid id);
        Task<IEnumerable<dynamic>> GetSystemVaccineCount();
    }
}
