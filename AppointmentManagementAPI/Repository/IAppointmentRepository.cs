namespace AppointmentManagementAPI.Repository
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<dynamic>> GetAllApointment();
    }
}
