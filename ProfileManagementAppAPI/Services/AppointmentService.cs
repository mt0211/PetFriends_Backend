using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using ProfileManagementAppAPI.DTOs;

using ProfileManagementAppAPI.Repositories;
using ProfileManagementAppAPI.Utilities;
using ProfileManagementAppAPI.DTOs.ClinicProfileModel;
using MySqlX.XDevAPI.Common;

namespace ProfileManagementAppAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _profileManagementRepository;
        public AppointmentService(IAppointmentRepository profileManagementRepository)
        {
            _profileManagementRepository = profileManagementRepository;
        }

        public async Task<ResultModel> GetCategory(string token)
        {
            var result = new ResultModel();

            try
            {

                var category = await _profileManagementRepository.GetCategory();
                if (category == null || !category.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found category";
                    return result;
                }
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = category;
                result.Message = "Successfully get all category";
            

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
