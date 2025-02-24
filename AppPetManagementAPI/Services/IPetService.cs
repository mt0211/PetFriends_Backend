using AppPetManagementAPI.DTOs.PetDTOs;
using AppPetManagementAPI.DTOs.ResultModel;
using AppPetManagementAPI.DTOs.VaccineDTOs;

namespace AppPetManagementAPI.Services
{
    public interface IPetService
    {
        Task<ResultModel> GetListPetByUserID(string token);
        Task<ResultModel> UpdatePetInformation(string token, PetUpdateReqModel updateModel);
        Task<ResultModel> AddPetInformation(string token, PetAddReqModel addmodel);
        Task<ResultModel> DeletePet(string token, Guid PetID);
        Task<ResultModel> AddUserPetVaccine(string token, AddUserPetVaccineReqModel model);
        Task<ResultModel> UpdateUserPetVaccine(string token, UpdateVaccineDoseReqModel model);
        Task<ResultModel> GetVaccineDetailByID(string token, Guid VaccineID);
        Task<ResultModel> RemovePetVaccine(string token, Guid vaccineId);
    }
}
