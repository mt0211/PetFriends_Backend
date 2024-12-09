using AppointmentManagementAPI.DTOs.ResultModel;
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;

namespace AppointmentManagementAPI.Services
{
    public interface IAppointmentService
    {
        Task<ResultModel> GetAllAppointment(string token, int page);
        Task<ResultModel> UpdateAppointmentStatus(AppointmentUpdateStatusModel appointmentstatusmodel);
    }
}
