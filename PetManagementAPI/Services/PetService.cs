using DataAccess.Models;
using PetManagementAPI.DTOs.PetDTOs;
using PetManagementAPI.DTOs.ResultModel;
using PetManagementAPI.DTOs.VaccineDTOs;
using PetManagementAPI.Repository;
using PetManagementAPI.Utilities;
namespace PetManagementAPI.Services
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _petRepository;
        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }
        public async Task<ResultModel> GetAllPet(string token, int page)
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
                var pets = await _petRepository.GetAllPets();
                if (pets == null || !pets.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found pet";
                    return result;
                }
                if (page == 0)
                {
                    page = 1;
                }
                var petList = pets.Select(p => new PetModel
                {
                    Id = p.Id,
                    Name = p.PetName,
                    Gender = p.Gender,
                    Species = p.Species,
                    Breed = p.Breed,
                    DateOfBirth = p.DateOfBirth,
                    OwnerName = p.OwnerName,
                    OwnerPhoneNumber = p.OwnerPhoneNumber,
                    Vaccinated = p.Vaccinated,
                    VaccineNames = p.VaccineNames
                }).ToList();
                //Paging
                var pageinatedResult = await Pagination.GetPagination(petList, page, 10);

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = pageinatedResult;
                result.Message = "Successfully get list pet";
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

        public async Task<ResultModel> AddNewPet(string token, AddPetReqModel addPetReqModel)
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
                var checkUser = await _petRepository.GetUserByPhoneNumber(addPetReqModel.PhoneNumber);
                var pet = new Pet
                {
                    Id = Guid.NewGuid(),
                    Name = addPetReqModel.Name,
                    Species = addPetReqModel.Species,
                    Breed = addPetReqModel.Breed,
                    Gender = addPetReqModel.Gender,
                    DateOfBirth = addPetReqModel.DateOfBirth,
                    Vaccinated = (byte)(addPetReqModel.Vaccinated ? 1 : 0),
                    UserId = checkUser?.Id,
                    UserPhoneNumber = addPetReqModel.PhoneNumber,
                };

                await _petRepository.Insert(pet);

                if (addPetReqModel.Vaccinated && addPetReqModel != null && addPetReqModel.VaccineIds.Any())
                {
                    foreach (var VaccineID in addPetReqModel.VaccineIds)
                    {
                        var NewPetVaccine = new PetVaccine
                        {
                            PetId = pet.Id,
                            VaccineId = VaccineID,
                            DateGiven = DateTime.UtcNow,
                            Notes = "Vaccine added for registration"
                        };
                        await _petRepository.AddPetVaccineAsync(NewPetVaccine);
                    }
                }

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully add new pet";
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
        public async Task<ResultModel> GetAllVaccine(string token)
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
                var vaccines = await _petRepository.GetAllVaccine();
                if (vaccines == null || !vaccines.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found vaccines";
                    return result;
                }

                var VaccineList = vaccines.Select(v => new VaccineResModel
                {
                    Id = v.Id,
                    Name = v.Name,
                }).ToList();

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = VaccineList;
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

        public async Task<ResultModel> GetPetDetail(string token, Guid guid)
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
                var pet = await _petRepository.GetPetById(guid);
                if (pet == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found";
                    return result;
                }
                var petDetail =  new PetModel
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Gender = pet.Gender,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    DateOfBirth = pet.DateOfBirth,
                    OwnerName = pet.OwnerName,
                    OwnerPhoneNumber = pet.OwnerPhoneNumber,
                    Vaccinated = pet.Vaccinated,
                    VaccineNames = pet.VaccineNames
                };

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = petDetail;
                result.Message = "Successfully get pet detail";
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


        public async Task<ResultModel> DeletePet(string token, Guid petId)
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
                var pet = await _petRepository.GetPetById(petId);
                if (pet == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Pet not found.";
                    return result;
                }

                // Xóa Pet và các bản ghi liên quan trong PetVaccine
                await _petRepository.DeletePetWithVaccinesAsync(petId);

                // Thành công
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Pet and related vaccines deleted successfully.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500; // Internal server error
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> UpdatePet(string token, UpdatePetReqModel petDto)
        {
            var result = new ResultModel();
            try
            {
                var pet = await _petRepository.GetPetByUpdate(petDto.Id);
                if (pet == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Pet not found.";
                    return result;
                }

                // Nếu có số điện thoại, tìm User liên kết
                if (!string.IsNullOrEmpty(petDto.PhoneNumber))
                {
                    var user = await _petRepository.GetUserByPhoneNumber(petDto.PhoneNumber);
                    pet.UserId = user?.Id;
                    pet.UserPhoneNumber = petDto.PhoneNumber;
                }

                // Cập nhật thông tin Pet
                pet.Name = petDto.Name;
                pet.Gender = petDto.Gender;
                pet.Species = petDto.Species;
                pet.Breed = petDto.Breed;
                pet.DateOfBirth = petDto.DateOfBirth;
                pet.Vaccinated = (byte)(petDto.Vaccinated ? 1 : 0);

               
                await _petRepository.UpdatePetAsync(pet);

                
                if (!petDto.Vaccinated)
                {
                    
                    await _petRepository.RemoveAllPetVaccinesAsync(pet.Id);
                }
                else if (petDto.VaccineIds != null && petDto.VaccineIds.Any())
                {
                    
                    await _petRepository.RemoveAllPetVaccinesAsync(pet.Id); 
                    await _petRepository.AddPetVaccinesAsync(pet.Id, petDto.VaccineIds); 
                }

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Pet updated successfully.";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.Message;
            }

            return result;
        }
    }
}
