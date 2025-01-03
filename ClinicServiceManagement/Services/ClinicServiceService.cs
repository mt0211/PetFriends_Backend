using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
using ClinicServiceManagement.Repository;
using ClinicServiceManagement.Utilites;

namespace ClinicServiceManagement.Services
{
    public class ClinicServiceService : IClinicServiceService
    {
        private readonly IClinicServiceRepository _clinicServiceRepository;
        public ClinicServiceService(IClinicServiceRepository clinicServiceRepository)
        {
            _clinicServiceRepository = clinicServiceRepository;
        }
        public async Task<ResultModel> GetAllService(string token)
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
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var Services = await _clinicServiceRepository.GetAllClinicService();
              var ServiceList = Services.Select(c=> new ServiceListDTO
              {
                  Id = c.Id,
                  Name = c.Name,
                  CategoryName = c.CategoryName,
                  EstimateTime = c.EstimateTime,
                  Price = c.Price,
                  Image = c.Image,
              }).ToList();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = Services;
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
