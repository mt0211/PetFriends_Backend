using AppointmentManagementAPI.DTOs.ResultModel;

namespace AppointmentManagementAPI.Services
{
    public interface IAppointmentService
    {
        Task<ResultModel> GetAllAppointment(string token, int page);
    }
}
