using DataAccess.Models;
using DataAccess.Repositories;

namespace AppPetManagementAPI.Repositories
{
    public interface IPetRepository : IRepository<Pet>
    {
        Task<IEnumerable<dynamic>> GetListPetByUserId(Guid UserId);
        Task UpdatePetInformation(Pet pet);
        Task AddUserPetVaccine(UserPetVaccine petVaccine);
        Task AddUserPetVaccineDose(UserPetVaccineDose petVaccineDose);
        Task<Vaccine> GetVaccineByName(string name);
        Task<UserPetVaccine> GetUserPetVaccineById(Guid userPetVaccineId);
        Task UpdateUserPetVaccine(UserPetVaccine userPetVaccine);
        Task RemoveUserPetVaccineDose(UserPetVaccineDose petVaccineDose);
        Task<UserPetVaccine> GetVaccineDetailByID(Guid id);
        Task RemoveUserPetVaccine(UserPetVaccine userPetVaccine);
        Task<PetVaccine> GetPetVaccineById(Guid id);
        Task RemovePetVaccine(PetVaccine petVaccine);
        Task RemoveUserPetVaccinesByPetId(Guid petId);
        Task RemovePetVaccinesByPetId(Guid petId);
        Task UpdateUserPetVaccineDose(UserPetVaccineDose dose);
        Task<List<Vaccine>> GetListVaccines();
        Task<UserPetVaccine> CheckVaccineSystem(Guid vaccineId);
        Task<UserPetVaccine> CheckVaccineName(Guid petId);
    }
}
