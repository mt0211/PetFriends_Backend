using DataAccess.Models;
using DataAccess.Repositories;

namespace PetManagementAPI.Repository
{
    public interface IPetRepository : IRepository<Pet>
    {
        Task<IEnumerable<dynamic>> GetAllPets();
        Task<User> GetUserByPhoneNumber(string phoneNumber);

        Task<IEnumerable<Vaccine>> GetAllVaccine();
        Task AddUserPetVaccineAsync(UserPetVaccine petVaccine);
        Task<dynamic> GetPetById(Guid petId);
        Task DeletePetWithVaccinesAsync(Guid petId);
        Task<Vaccine> GetVaccineById(Guid vaccineId);
        Task UpdatePetBasicInfo(Guid petId,string name,string gender,string species,string breed,DateTime? dateOfBirth,Guid? userId,string userPhoneNumber,byte vaccinated);
        Task RemoveAllPetVaccinesAsync(Guid petId);
        Task UpdatePetVaccinesAsync(Guid petId, List<Guid> newVaccineIds);
        Task<User> GetUserById(Guid userId);
        Task<UserCartItem> GetUserCartItemByPetId(Guid petId);
    }
}
