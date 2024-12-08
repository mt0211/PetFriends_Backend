using DataAccess.Models;
using DataAccess.Repositories;

namespace AppointmentManagementAPI.Repository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<dynamic>> GetAllApointment();
    }
}
