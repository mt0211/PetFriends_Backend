using AppPetManagementAPI.DTOs.PetDTOs;
using AppPetManagementAPI.DTOs.ResultModel;
using AppPetManagementAPI.DTOs.VaccineDTOs;
using AppPetManagementAPI.Repositories;
using AppPetManagementAPI.Utilities;
using DataAccess.Models;

namespace AppPetManagementAPI.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repository;
        public PetService(IPetRepository repository)
        {
            _repository = repository;
        }

        public async Task<ResultModel> GetListPetByUserID(string token)
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
                var pets = await _repository.GetListPetByUserId(id);
                if (pets == null || !pets.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found pet";
                    return result;
                }

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = pets;
                result.Message = "Successfully get all pet";
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
        public async Task<ResultModel> UpdatePetInformation(string token, PetUpdateReqModel updateModel)
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
                var pet = await _repository.Get(updateModel.Id);
                var updatePet = new Pet
                {
                    Id = updateModel.Id,
                    Name = updateModel.Name,
                    Gender = updateModel.Gender,
                    Species = updateModel.Species,
                    Breed = updateModel.Breed,
                    DateOfBirth = updateModel.DateOfBirth,
                    Weight = updateModel.Weight,
                    Description = updateModel.Description,
                };
                await _repository.UpdatePetInformation(updatePet);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = updatePet;
                result.Message = "Successfully updated pet information";
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
        public async Task<ResultModel> AddPetInformation(string token, PetAddReqModel addmodel)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            var userPhoneNumber = Encoder.DecodeToken(token, "phonenumber");
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
                var NewPet = new Pet
                {
                    Id = Guid.NewGuid(),
                    Name = addmodel.Name,
                    Gender = addmodel.Gender,
                    Species = addmodel.Species,
                    UserId = id,
                    UserPhoneNumber = userPhoneNumber,
                    Breed = addmodel.Breed,
                    DateOfBirth = addmodel.DateOfBirth,
                    Weight = addmodel.Weight,
                    Description = addmodel.Description,
                };
                await _repository.Insert(NewPet);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = NewPet;
                result.Message = "Successfully add pet information";
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
        public async Task<ResultModel> DeletePet(string token, Guid PetID)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            var userPhoneNumber = Encoder.DecodeToken(token, "phonenumber");
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
                var pet = await _repository.Get(PetID);
                await _repository.RemoveUserPetVaccinesByPetId(PetID);
                await _repository.RemovePetVaccinesByPetId(PetID);
                var deletePet = await _repository.Remove(pet);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully delete pet.";
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
        public async Task<ResultModel> AddUserPetVaccine(string token, AddUserPetVaccineReqModel model)
        {
            var result = new ResultModel();
            try
            {
                var existingVaccine = await _repository.GetVaccineByName(model.VaccineName);
                Guid? vaccineId = existingVaccine?.Id;
                var newPetVaccine = new UserPetVaccine
                {
                    Id = Guid.NewGuid(),
                    PetId = model.PetID,
                    VaccineId = vaccineId,
                    Name = model.VaccineName,
                    NumberOfDoses = model.NumberOfDoses
                };
                var checkVaccineName = await _repository.CheckVaccineName(model.PetID);
                if(checkVaccineName != null){
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = $"Can't add vaccine name {model.VaccineName} because it already exists";
                    return result;
                }

                await _repository.AddUserPetVaccine(newPetVaccine);
                var injectionsDTO = new List<UserPetVaccineDoseDTO>();
                foreach (var injection in model.Injections)
                {
                    var petVaccineDose = new UserPetVaccineDose
                    {
                        Id = Guid.NewGuid(),
                        UserPetVaccineId = newPetVaccine.Id,
                        DoseNumber = injection.DoseNumber,
                        DateGiven = injection.DateGiven
                    };

                    await _repository.AddUserPetVaccineDose(petVaccineDose);
                    injectionsDTO.Add(new UserPetVaccineDoseDTO
                    {
                        DoseNumber = injection.DoseNumber,
                        DateGiven = injection.DateGiven
                    });
                }

                var petVaccineDTO = new UserPetVaccineDTO
                {
                    VaccineId = newPetVaccine.Id,
                    PetID = newPetVaccine.PetId,
                    VaccineName = newPetVaccine.Name,
                    NumberOfDoses = newPetVaccine.NumberOfDoses,
                    Injections = injectionsDTO
                };

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = petVaccineDTO;
                result.Message = "Successfully added pet vaccine";
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
        public async Task<ResultModel> UpdateUserPetVaccine(string token, UpdateVaccineDoseReqModel model)
        {
            var result = new ResultModel();
            try
            {
                // 1️⃣ Kiểm tra user
                var userId = Encoder.DecodeToken(token, "userid");
                if (!Guid.TryParse(userId, out Guid userGuid))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid user ID";
                    return result;
                }
                var userPetVaccine = await _repository.GetUserPetVaccineById(model.VaccineId);
                var checkVaccineSystem = await _repository.CheckVaccineSystem(model.VaccineId);
               
                if (userPetVaccine == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "UserPetVaccine not found";
                    return result;
                }
                userPetVaccine.Name = model.VaccineName;
                int oldNumberOfDoses = userPetVaccine.NumberOfDoses ?? 0;
                int newNumberOfDoses = model.NumberOfDoses;
                userPetVaccine.NumberOfDoses = newNumberOfDoses;

                // 4️⃣ Xử lý tăng / giảm mũi tiêm
                if (newNumberOfDoses > oldNumberOfDoses)
                {
                    // Thêm (newNumberOfDoses - oldNumberOfDoses) liều
                    for (int doseNumber = oldNumberOfDoses + 1; doseNumber <= newNumberOfDoses; doseNumber++)
                    {
                        // Tìm injectionDetail trong model.Injections (nếu user có nhập)
                        var injectionDetail = model.Injections
                            .FirstOrDefault(x => x.DoseNumber == doseNumber);

                        var dateGiven = injectionDetail != null
                            ? injectionDetail.DateGiven
                            : DateTime.UtcNow; // hoặc default

                        var newDose = new UserPetVaccineDose
                        {
                            Id = Guid.NewGuid(),
                            UserPetVaccineId = userPetVaccine.Id,
                            DoseNumber = doseNumber,
                            DateGiven = dateGiven
                        };
                        await _repository.AddUserPetVaccineDose(newDose);
                    }
                }
                else if (newNumberOfDoses < oldNumberOfDoses)
                {
                    // Xoá các liều dư
                    var removeDoses = userPetVaccine.UserPetVaccineDoses
                        .Where(d => d.DoseNumber > newNumberOfDoses)
                        .ToList();

                    foreach (var dose in removeDoses)
                    {
                        await _repository.RemoveUserPetVaccineDose(dose);
                    }
                }

                // 5️⃣ Nếu cần cập nhật ngày tiêm cho các liều cũ
                //    Lặp model.Injections, cập nhật DateGiven
                foreach (var injection in model.Injections)
                {
                    // Tìm liều cũ
                    var existingDose = userPetVaccine.UserPetVaccineDoses
                        .FirstOrDefault(d => d.DoseNumber == injection.DoseNumber);
                    if (existingDose != null)
                    {
                        existingDose.DateGiven = injection.DateGiven;
                        await _repository.UpdateUserPetVaccineDose(existingDose);
                        // SaveChange sau khi cập nhật
                    }
                }
                 if(checkVaccineSystem.VaccineId != null){
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can't update vaccine system's name";
                    return result;
                }
                // 6️⃣ Lưu thay đổi UserPetVaccine
                await _repository.UpdateUserPetVaccine(userPetVaccine);

                var updatedDto = new UserPetVaccineDTO
                {
                    VaccineId = userPetVaccine.Id,
                    PetID = userPetVaccine.PetId, // hoặc null, tuỳ DB
                    VaccineName = userPetVaccine.Name,
                    NumberOfDoses = userPetVaccine.NumberOfDoses,
                    Injections = userPetVaccine.UserPetVaccineDoses
                    .Select(d => new UserPetVaccineDoseDTO
                    {
                        DoseNumber = d.DoseNumber,
                        DateGiven = d.DateGiven ?? DateTime.MinValue
                    })
        .ToList()
                };

                // Trả về DTO
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully updated user pet vaccine";
                result.Data = updatedDto;
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
        public async Task<ResultModel> GetVaccineDetailByID(string token, Guid VaccineID)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Please authorize";
                return result;
            }

            try
            {
                var userPetVaccine = await _repository.GetVaccineDetailByID(VaccineID);
                var vaccineDTO = new UserPetVaccineDTO
                {
                    VaccineId = userPetVaccine.Id,
                    PetID = userPetVaccine.PetId,  // tuỳ cột
                    VaccineName = userPetVaccine.Name,
                    NumberOfDoses = userPetVaccine.NumberOfDoses,
                    Injections = userPetVaccine.UserPetVaccineDoses
                        .Select(d => new UserPetVaccineDoseDTO
                        {
                            DoseNumber = d.DoseNumber,
                            DateGiven = d.DateGiven ?? DateTime.MinValue
                        })
                        .ToList()
                };

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccineDTO; // Trả về DTO thay vì entity
                result.Message = "Successfully get vaccine detail";
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
        public async Task<ResultModel> RemovePetVaccine(string token, Guid vaccineId)
        {
            var result = new ResultModel();
            try
            {
                // 1️⃣ Kiểm tra user
                var userId = Encoder.DecodeToken(token, "userid");
                if (!Guid.TryParse(userId, out Guid userGuid))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid user ID";
                    return result;
                }

                // 2️⃣ Thử tìm trong UserPetVaccine (do user thêm)
                var userPetVaccine = await _repository.GetUserPetVaccineById(vaccineId);
                if (userPetVaccine != null)
                {
                    // Xóa vaccine user thêm (ngoài hệ thống => VaccineId = null
                    // hoặc trùng hệ thống => VaccineId != null)
                    await _repository.RemoveUserPetVaccine(userPetVaccine);

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Successfully removed user pet vaccine.";
                    return result;
                }

                // 3️⃣ Nếu không tìm thấy, thử tìm trong PetVaccine (do hệ thống thêm)
                var petVaccine = await _repository.GetPetVaccineById(vaccineId);
                if (petVaccine != null)
                {
                    // Xóa vaccine do hệ thống thêm
                    await _repository.RemovePetVaccine(petVaccine);

                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Successfully removed system pet vaccine.";
                    return result;
                }

                //4️⃣ Không tìm thấy ở cả 2 bảng => 404
                result.IsSuccess = false;
                result.Code = 404;
                result.Message = "Vaccine not found.";
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
            try
            {
                var vaccines = await _repository.GetListVaccines();
                if (vaccines == null || !vaccines.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found pet";
                    return result;
                }

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccines;
                result.Message = "Successfully get all vaccine";
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
