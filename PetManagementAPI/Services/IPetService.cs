using PetManagementAPI.DTOs.PetDTOs;
using PetManagementAPI.DTOs.ResultModel;

namespace PetManagementAPI.Services
{
    public interface IPetService
    {
        Task<ResultModel> GetAllPet(string token, int page);
        Task<ResultModel> GetAllVaccine(string token);
        Task<ResultModel> AddNewPet(string token, AddPetReqModel addPetReqModel);
        Task<ResultModel> GetPetDetail(string token, Guid guid);
        Task<ResultModel> DeletePet(string token, Guid petId);
        Task<ResultModel> UpdatePet(string token, UpdatePetReqModel petDto);
    }
}
