using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
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
        Task<List<AppointmentServiceDetailModel>> GetAppointmentServices(Guid appointmentId);
        Task<UserBookingSummary> GetUserBookingSummary(Guid userId);
        Task AddUserBookingSummary(UserBookingSummary userBookingSummary);
        Task UpdateUserBookingSummary(UserBookingSummary userBookingSummary);
        Task UpdateDailyRevenue(decimal amount);
        Task UpdateServiceRevenue(Guid serviceTypeId, decimal amount);
        Task<List<AppointmentPromotion>> GetAppointmentPromotions(Guid appointmentId);
        Task UpdatePromotion(Promotion promotion);
        Task<List<ClinicService>> GetVaccinationServices(List<Guid> serviceIds);
        Task<UserPetVaccine> GetPetVaccineByNameAndPetId(string vaccineName, Guid petId);
        Task<Vaccine> GetVaccineByName(string name);
        Task AddUserPetVaccine(UserPetVaccine petVaccine);
        Task AddUserPetVaccineDose(UserPetVaccineDose petVaccineDose);
        Task UpdateUserPetVaccine(UserPetVaccine petVaccine);
        Task<Guid?> GetUserIdByPhoneOrEmail(string? phone, string? email);
        Task<GuestUser> CreateOrGetGuestUser(AppointmentAddModel appointment);
        Task<GuestPet> CreateOrGetGuestPet(AppointmentAddModel appointment, Guid guestUserId);
        
        //FIX UPDATE APPOINTMENT
        Task<GuestUser> GetGuestUserByID(Guid? id);
        Task<GuestPet> GetGuestPetByID(Guid? id);
        Task UpdateGuestUser(GuestUser guestUser);
        Task UpdateGuestPet(GuestPet guestPet);
        Task<User> GetUserByID(Guid? id);
        Task<Pet> GetPetByID(Guid? id);
        Task RemoveAppointmentClinicServiceById(Guid serviceId);
        Task UpdateAppointmentBasicInfo(Guid appointmentId, string status, DateTime? startAt, string note, DateTime? endAt = null);
    }
}
