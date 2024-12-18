using AppointmentManagementAPI.DTOs.ResultModel;
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using DataAccess.Models;

namespace AppointmentManagementAPI.Services
{
    public interface IAppointmentService
    {
        Task<ResultModel> GetAllAppointment(string token, int page);
        Task<ResultModel> UpdateAppointmentStatus(string token, AppointmentUpdateStatusModel appointmentstatusmodel);
        Task<ResultModel> GetListClinicservice(string token);
        Task<ResultModel> CheckUserByPhoneNumber(string token, string phonenumber);
        Task<ResultModel> CheckPetByNameAndUserID(string token, string petName, Guid UserId);
        Task<ResultModel> AddAppointment(string token, AppointmentAddModel appointment);
        Task<ResultModel> GetAppointmentDetail(string token, Guid guid);
        Task<ResultModel> DeleteAppointment(string token, Guid appointmentID);
        Task<ResultModel> UpdateAppointment(string token, AppointmentUpdateModel appointmentUpdate);
    }
}
