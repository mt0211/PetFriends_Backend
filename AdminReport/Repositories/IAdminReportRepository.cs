using DataAccess.Models;

namespace AdminReport.Repositories
{
    public interface IAdminReportRepository
    {
        Task<IEnumerable<dynamic>> GetDataUserStatus(int year, int? month);
        Task<IEnumerable<dynamic>> GetDataNewUser(int year, int? month);
        Task<IEnumerable<dynamic>> GetPostDistribution();
        Task<User> GetUserByID(Guid id);
    
        Task<List<dynamic>> GetTopVaccine();
        Task<IEnumerable<dynamic>> GetDataUserStatusForExcel(int year, int? month);
        Task<(List<dynamic> vaccineData, List<dynamic> postDistribution, IEnumerable<dynamic> newUsers, IEnumerable<dynamic> userStatus)> GetAllReportsDataForExport(int year, int? month);
    }
}
