
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementAPI.Repository
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly PetfriendsContext _context;
        public AppointmentRepository(PetfriendsContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllApointment()
        {
            return await _context.Appointments
        .Include(a => a.User)
        .Include(a => a.Pet)
        .Include(a => a.GuestUser)
        .Include(a => a.GuestPet)
        .Include(a => a.ClinicService)
        .Select(a => new
        {
            Id = a.Id,
            CustomerName = a.UserId != null ? a.User.FullName : a.GuestUser.FullName,
            PetName = a.PetId != null ? a.Pet.Name : a.GuestPet.Name,
            Date = a.PetId != null ? a.Pet.DateOfBirth : a.GuestPet.DateOfBirth,
            StartTime = a.StartAt,
            ServiceType = a.ClinicService.Name,
            Status = a.Status,
        })
        .ToListAsync();
        }

        public async Task<(string Email, string FullName, string status, DateTime? CreatedAt, DateTime? StartAt, DateTime? EndAt)> GetAppointmentAndUserEmail(Guid AppointmentID)
        {
            var appointment = await _context.Appointments
                 .Where(p => p.Id == AppointmentID)
                 .Select(p => new
                 {
                     p.User.Email,
                     p.User.FullName,
                     p.Status,
                     p.CreatedAt,
                     p.StartAt,
                     p.EndAt,
                 }).FirstOrDefaultAsync();
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }
            return (appointment.Email, appointment.FullName, appointment.Status, appointment.CreatedAt, appointment.StartAt, appointment.EndAt);
        }
        public async Task<IEnumerable<ClinicService>> GetListClinicservices()
        {
            return await _context.ClinicServices.ToListAsync();
        }



        public async Task<User> GetUserByPhoneNumber(string phonenumber)
        {
            var userData = await _context.Users
       .Where(u => u.PhoneNumber == phonenumber)
       .FirstOrDefaultAsync();
            return userData;
        }

        public async Task<Pet> GetPetByNameAndUserId(string petName, Guid userId)
        {
            var pet = await _context.Pets
                .Where(p => p.Name == petName && p.UserId == userId).FirstOrDefaultAsync();
            return pet;
        }

        public async Task<GuestUser> GetGuestUserByPhoneNumber(string phoneNumber)
        {
            return await _context.GuestUsers.FirstOrDefaultAsync(g => g.PhoneNumber == phoneNumber);
        }

        public async Task<GuestPet> GetGuestPetByNameAndGuestUserId(string petName, Guid guestUserId)
        {
            return await _context.GuestPets.FirstOrDefaultAsync(p => p.Name == petName && p.GuestUserId == guestUserId);
        }

        public async Task InsertGuestUser(GuestUser guestUser)
        {
            try
            {
                if (guestUser == null)
                    throw new ArgumentNullException(nameof(guestUser));

                await _context.GuestUsers.AddAsync(guestUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("IIIIIIIIIIIIIIIIIIIIIIIIIIInserted GuestUser: " + guestUser.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EEEEEEEEEEEEEEEEEEEEEEEEEEEError inserting GuestUser: " + ex.Message);
                throw;
            }
        }

        public async Task InsertGuestPet(GuestPet guestPet)
        {
            if (guestPet == null)
            {
                throw new ArgumentNullException(nameof(guestPet));
            }

            await _context.GuestPets.AddAsync(guestPet);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment> GetAppointmentByID(Guid appointmentId)
        {
            return await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a => a.GuestUser)
            .Include(a => a.GuestPet)
            .Include(a => a.ClinicService)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
    }
}
