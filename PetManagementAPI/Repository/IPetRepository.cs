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
        Task UpdatePetAsync(Pet pet);
        Task UpdatePetVaccinesAsync(Guid petId, List<Guid> newVaccineIds);
        Task<Pet> GetPetByUpdate(Guid petId);
        Task RemoveAllPetVaccinesAsync(Guid petId);
        Task AddPetVaccinesAsync(Guid petId, List<Guid> newVaccineIds);
        Task<Vaccine> GetVaccineById(Guid vaccineId);
    }
}
