using AppointmentManagementAPI.DTOs.ResultModel;
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using AppointmentManagementAPI.Repository;
using AppointmentManagementAPI.Utilities;

namespace AppointmentManagementAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentrepository;
        public AppointmentService(IAppointmentRepository appointmentrepository)
        {
            _appointmentrepository = appointmentrepository;
        }
        public async Task<ResultModel> GetAllAppointment(string token, int page)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var appointments = await _appointmentrepository.GetAllApointment();
                if (appointments == null || !appointments.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found appointment";
                    return result;
                }
                if (page == 0)
                {
                    page = 1;
                }
                var AppointmentList = appointments.Select( a=> new AppointmentModel
                {
                    Id = a.Id,
                    CustomerName = a.CustomerName,
                    PetName = a.PetName,
                    Date = a.Date,
                    StartTime = a.StartTime,
                    ServiceType = a.ServiceType,
                    Status = a.Status,
                }).ToList();
                var pageinatedResult = await Pagination.GetPagination(AppointmentList, page, 10);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = pageinatedResult;
                result.Message = "Successfully get list appointment";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
    }
}
