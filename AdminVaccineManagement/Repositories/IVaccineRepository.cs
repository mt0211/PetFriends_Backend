using DataAccess.Models;
using DataAccess.Repositories;

namespace AdminVaccineManagement.Repositories
{
    public interface IVaccineRepository : IRepository<Vaccine>
    {
        Task<IEnumerable<dynamic>> GetListVaccines();
        Task<dynamic> GetVaccineDetail(Guid id);
        Task<User> GetUserByID(Guid id);
        Task AddVaccineDoses(VaccineDose vaccineDose);
        Task<List<VaccineDose>> GetVaccineDosesByVaccineId(Guid vaccineId);
        Task DeleteVaccineDoses(List<VaccineDose> doses);
        Task UpdateVaccineDose(VaccineDose vaccineDose);
    }
}
