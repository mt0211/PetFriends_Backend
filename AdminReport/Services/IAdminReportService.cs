using AdminReport.DTO.ResultModel;

namespace AdminReport.Services
{
    public interface IAdminReportService
    {
        Task<ResultModel> GetDataUserStatus(string token, int year, int month);
        Task<ResultModel> GetDataNewUser(string token, int year, int month);
        Task<ResultModel> GetPostDistribution(string token);
        Task<ResultModel> GetSystemVaccineCount(string token);
        Task<ResultModel> ExportAllReportsToExcel(string token, int year, int? month);
    }
}
