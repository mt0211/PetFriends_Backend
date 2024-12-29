using DataAccess.Models;
using DataAccess.Repositories;

namespace AppointmentManagementAPI.Repository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<dynamic>> GetAllApointment();
        Task<(string Email, string FullName, string status, DateTime? CreatedAt, DateTime? StartAt, DateTime? EndAt)> GetAppointmentAndUserEmail(Guid AppointmentID);
        Task<IEnumerable<ClinicService>> GetListClinicservices();
        Task<User> GetUserByPhoneNumber(string phonenumber);
        Task<Pet> GetPetByNameAndUserId(string petName, Guid userId);
        Task<GuestUser> GetGuestUserByPhoneNumber(string phoneNumber);
        Task<GuestPet> GetGuestPetByNameAndGuestUserId(string petName, Guid guestUserId);
        Task InsertGuestUser(GuestUser guestUser);
        Task InsertGuestPet(GuestPet guestPet);
        Task<Appointment> GetAppointmentByID(Guid appointmentId);
        Task InsertAppointmentClinicService(AppointmentClinicService appointmentClinicService);
        Task RemoveAppointmentClinicService(AppointmentClinicService appointmentClinicService);

    }
}
