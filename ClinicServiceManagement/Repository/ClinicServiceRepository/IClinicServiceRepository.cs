using DataAccess.Models;
using DataAccess.Repositories;

namespace ClinicServiceManagementAPI.Repository.ClinicServiceRepository
{
    public interface IClinicServiceRepository : IRepository<ClinicService>
    {
        Task<IEnumerable<dynamic>> GetAllClinicService();
        Task<IEnumerable<Category>> GetAllCategory();
        Task<dynamic> GetServiceByID(Guid id);
        Task<ClinicService> GetServiceStatusByID(Guid id);
        Task UpdateStatus(ClinicService service);
        Task UpdateService(ClinicService service);
        Task AddService(ClinicService service);
        Task<ClinicService> GetServiceByNameAndCategory(string name, Guid? categoryId);
        Task<List<Vaccine>> GetAllVaccine();
        Task<ClinicService> GetServiceByName(string name);
        Task<Vaccine> GetVaccineByName(string name);
        Task<Category> GetCategoryByID(Guid? id);
        Task<List<User>> GetListAdminAccount();
    }
}
