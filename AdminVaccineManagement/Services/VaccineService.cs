using AdminVaccineManagement.DTOs.ResultModel;
using AdminVaccineManagement.DTOs.VaccineDTOs;
using AdminVaccineManagement.Repositories;
using AdminVaccineManagement.Utilities;
using AutoMapper;
using DataAccess.Models;
using System.Reflection.PortableExecutable;

namespace AdminVaccineManagement.Services
{
    public class VaccineService : IVaccineService
    {
        private readonly IVaccineRepository _repository;
        public VaccineService(IVaccineRepository repository)
        {
            _repository = repository;
        }
        public async Task<ResultModel> GetListVaccines(string token)
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
                var vaccines = vaccine.Select(v => new VaccineListResModel
                {
                    Id = v.Id,
                    Name = v.Name,
                    NumberOfDoses = v.NumberOfDoses,
                    FirstInject = v.FirstInject,
                    Recommendation = v.Recommendation,
                    Status = v.Status,
                }).ToList();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccines;
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
        public async Task<ResultModel> DeleteVaccine(string token, Guid VaccineID)
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
                var vaccine = await _repository.Get(VaccineID);
                await _repository.Remove(vaccine);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Delete vaccine successfully!";
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
        public async Task<ResultModel> AddNewVaccine(string token, VaccineAddReqModel AddModel)
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
            if (AddModel.NumberOfDoses <= 0)
            {
                result.IsSuccess = true;
                result.Code = 400;
                result.Message = "Doses must be greater than 0!";
                return result ;
            }

            if (AddModel.Injecttions == null || AddModel.Injecttions.Count != AddModel.NumberOfDoses)
            {
                result.IsSuccess = true;
                result.Code = 400;
                result.Message = "Number of doses must equal injections days!";
                return result;
            }
            try
            {
                    var newvaccine = new Vaccine
                    {
                        Id = Guid.NewGuid(),
                        Name = AddModel.Name,
                        NumberOfDoses = AddModel.NumberOfDoses,
                        Recommendation = AddModel.Recommendation,
                        Status = 0,
                    };
                    await _repository.AddVaccine(newvaccine);

                    for (int i = 0; i < AddModel.NumberOfDoses; i++)
                    {
                        var vaccineDose = new VaccineDose
                        {
                            Id = Guid.NewGuid(),
                            VaccineId = newvaccine.Id,
                            DoseNumber = i + 1,
                            DaysAfterPrevious = AddModel.Injecttions[i]
                        };
                        await _repository.AddVaccineDoses(vaccineDose); // Sử dụng repository method
                    }
                    var responseDTO = new VaccineResponseDTO
                {
                    Id = newvaccine.Id,
                    Name = newvaccine.Name,
                    NumberOfDoses = newvaccine.NumberOfDoses,
                    Recommendation = newvaccine.Recommendation,
                    Status = newvaccine.Status,
                    VaccineDoses = (await _repository.GetVaccineDosesByVaccineId(newvaccine.Id))
                        .Select(d => new VaccineDoseDTO 
                        {
                            Id = d.Id,
                            DoseNumber = d.DoseNumber,
                            DaysAfterPrevious = d.DaysAfterPrevious
                        }).ToList()
                };

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = responseDTO;
                    result.Message = "Successfully add data";
                
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
        public async Task<ResultModel> EditVaccine(string token, VaccineUpdateReqModel EditModel)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400, // Bad request
                    Message = "Invalid user ID"
                };
            }
            if (userId == null)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400, // Bad request
                    Message = "Please authorize"
                };
            }
            var user = await _repository.GetUserByID(id);
            if (user.Role != "ADMIN")
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 401, // Unauthorized
                    Message = "Permission Denied"
                };
            }
            if (EditModel.NumberOfDoses <= 0)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Doses must be greater than 0!"
                };
            }
            if (EditModel.Injecttions == null || EditModel.Injecttions.Count != EditModel.NumberOfDoses)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Number of doses must equal injections days!"
                };
            }

            try
            {
                var existingVaccine = await _repository.Get(EditModel.Id);
                if (existingVaccine == null)
                {
                    return new ResultModel
                    {
                        IsSuccess = false,
                        Code = 404, // Not found
                        Message = "Vaccine not found!"
                    };
                }
                    try
                    {
                        // Cập nhật thông tin vaccine
                        existingVaccine.Name = EditModel.Name;
                        existingVaccine.NumberOfDoses = EditModel.NumberOfDoses;
                        existingVaccine.Recommendation = EditModel.Recommendation;

                        // Lưu cập nhật vaccine vào database
                        await _repository.Update(existingVaccine);

                        // Lấy danh sách liều tiêm hiện có từ database
                        var existingDoses = await _repository.GetVaccineDosesByVaccineId(EditModel.Id);

                        // *** Xử lý số lượng liều tiêm ***

                        // Nếu số liều mới ít hơn số liều cũ, cần xóa bớt liều tiêm
                        if (EditModel.NumberOfDoses < existingDoses.Count)
                        {
                            // Lấy danh sách liều dư để xóa (bỏ qua n phần tử đầu tiên)
                            var dosesToRemove = existingDoses.Skip(EditModel.NumberOfDoses).ToList();

                            // Xóa các liều dư khỏi database
                            await _repository.DeleteVaccineDoses(dosesToRemove);
                        }
                        // Nếu số liều mới lớn hơn số liều cũ, cần thêm liều mới vào database
                        else if (EditModel.NumberOfDoses > existingDoses.Count)
                        {
                            for (int i = existingDoses.Count; i < EditModel.NumberOfDoses; i++)
                            {
                                var newDose = new VaccineDose
                                {
                                    Id = Guid.NewGuid(), // Tạo ID mới
                                    VaccineId = EditModel.Id, // Liên kết với vaccine hiện tại
                                    DoseNumber = i + 1, // Số thứ tự của liều
                                    DaysAfterPrevious = EditModel.Injecttions[i] // Khoảng cách ngày
                                };

                                // Thêm liều mới vào database
                                await _repository.AddVaccineDoses(newDose);
                            }
                        }

                        // Cập nhật khoảng cách ngày của các liều tiêm đã có
                        for (int i = 0; i < Math.Min(existingDoses.Count, EditModel.NumberOfDoses); i++)
                        {
                            existingDoses[i].DaysAfterPrevious = EditModel.Injecttions[i];

                            // Cập nhật thông tin khoảng cách ngày của liều tiêm trong database
                            await _repository.UpdateVaccineDose(existingDoses[i]);
                        }
                         var responseDTO = new VaccineResponseDTO
                            {
                                Id = existingVaccine.Id,
                                Name = existingVaccine.Name,
                                NumberOfDoses = existingVaccine.NumberOfDoses,
                                Recommendation = existingVaccine.Recommendation,
                                Status = existingVaccine.Status,
                                VaccineDoses = (await _repository.GetVaccineDosesByVaccineId(existingVaccine.Id))
                                    .Select(d => new VaccineDoseDTO 
                                    {
                                        Id = d.Id,
                                        DoseNumber = d.DoseNumber,
                                        DaysAfterPrevious = d.DaysAfterPrevious
                                    })
                                    .OrderBy(d => d.DoseNumber)
                                    .ToList()
                            };

                        return new ResultModel
                        {
                            IsSuccess = true,
                            Code = 200,
                            Message = "Vaccine updated successfully",
                            Data = responseDTO
                        };
                    }
                    catch (Exception ex)
                    {
                        return new ResultModel
                        {
                            IsSuccess = false,
                            Code = 500, // Internal Server Error
                            Message = ex.InnerException?.Message ?? ex.Message
                        };
                    }
                
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    IsSuccess = false,
                    Code = 500, // Internal Server Error
                    Message = ex.InnerException?.Message ?? ex.Message
                };
            }
        }
    }
}
