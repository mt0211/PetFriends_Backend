using AdminVaccineManagement.DTOs.ResultModel;
using AdminVaccineManagement.DTOs.VaccineDTOs;

namespace AdminVaccineManagement.Services
{
    public interface IVaccineService
    {
        Task<ResultModel> GetListVaccines(string token, int page);
        Task<ResultModel> GetVaccineDetail(string token, Guid VaccineID);
        Task<ResultModel> UpdateVaccineStatus(string token, VaccineUpdateStatusReqModel UpdateModel);
    }
}
