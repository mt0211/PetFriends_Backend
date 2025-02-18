using AdminVaccineManagement.DTOs.ResultModel;
using AdminVaccineManagement.DTOs.VaccineDTOs;
using AdminVaccineManagement.Repositories;
using AdminVaccineManagement.Utilities;

namespace AdminVaccineManagement.Services
{
    public class VaccineService : IVaccineService
    {
        private readonly IVaccineRepository _repository;
        public VaccineService(IVaccineRepository repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetListVaccines(string token, int page)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var vaccine = await _repository.GetListVaccines();
                if (page == 0)
                {
                    page = 1;
                }
                var vaccines = vaccine.Select(v => new VaccineListResModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    NumberOfDoses = v.NumberOfDoses,
                    FirstInject = v.FirstInject,
                    Recommendation = v.Recommendation,
                    Status = v.Status,
                }).ToList();
                var paginatedResult = await Pagination.GetPagination(vaccines, page, 10);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = paginatedResult;
                result.Message = "Successfully get data";
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

        public async Task<ResultModel> GetVaccineDetail(string token, Guid VaccineID)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var vaccine = await _repository.GetVaccineDetail(VaccineID);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccine;
                result.Message = "Successfully get data";
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
        public async Task<ResultModel> UpdateVaccineStatus(string token, VaccineUpdateStatusReqModel UpdateModel)
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
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var vaccine = await _repository.Get(UpdateModel.Id);
                if(vaccine == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Vaccine not found";
                    return result;
                }
                vaccine.Status = UpdateModel.Status;
                await _repository.Update(vaccine);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccine;
                result.Message = "Update successfully";
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
