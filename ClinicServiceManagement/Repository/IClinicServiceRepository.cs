namespace ClinicServiceManagement.Repository
{
    public interface IClinicServiceRepository
    {
        Task<IEnumerable<dynamic>> GetAllClinicService();
    }
}
