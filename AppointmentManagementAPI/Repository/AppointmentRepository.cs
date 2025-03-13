
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
       .Include(a => a.AppointmentClinicServices)
           .ThenInclude(acs => acs.ClinicService)
       .Select(appointment => new
       {
           Id = appointment.Id,
           CreatedAt = appointment.CreatedAt,
           StartAt = appointment.StartAt,
           EndAt = appointment.EndAt,
           Status = appointment.Status,
           Note = appointment.Note,
           UserName = appointment.UserId != null ? appointment.User.FullName : appointment.GuestUser.FullName,
           PetName = appointment.PetId != null ? appointment.Pet.Name : appointment.GuestPet.Name,
           ServiceNames = appointment.AppointmentClinicServices
               .Select(service => service.ClinicService.Name)
               .ToList()
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
            return await _context.ClinicServices
        .Where(service => service.Status == "ACTIVE" && service.CategoryNavigation.Status == 1)
        .ToListAsync();
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
        .Include(a => a.AppointmentClinicServices)
            .ThenInclude(acs => acs.ClinicService)
        .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        public async Task InsertAppointmentClinicService(AppointmentClinicService appointmentClinicService)
        {
            _context.AppointmentClinicServices.Add(appointmentClinicService);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAppointmentClinicService(AppointmentClinicService appointmentClinicService)
        {
            _context.AppointmentClinicServices.Remove(appointmentClinicService);
            await _context.SaveChangesAsync();
        }


        public async Task<List<AppointmentServiceDetailModel>> GetAppointmentServices(Guid appointmentId)
        {
            return await _context.AppointmentClinicServices
                .Where(acs => acs.AppointmentId == appointmentId)
                .Select(acs => new AppointmentServiceDetailModel
                {
                    ClinicServiceId = acs.ClinicServiceId,
                    ServiceName = acs.ClinicService.Name,
                    Price = acs.ClinicService.DiscountedPrice
                })
                .ToListAsync();
        }

        public async Task<UserBookingSummary> GetUserBookingSummary(Guid userId)
        {
            return await _context.UserBookingSummaries.FirstOrDefaultAsync(ubs => ubs.UserId == userId);
        }

        public async Task AddUserBookingSummary(UserBookingSummary userBookingSummary)
        {
            await _context.UserBookingSummaries.AddAsync(userBookingSummary);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserBookingSummary(UserBookingSummary userBookingSummary)
        {
            _context.UserBookingSummaries.Update(userBookingSummary);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDailyRevenue(decimal amount)
        {
            var today = DateTime.UtcNow.Date;
            DateOnly dateOnly = new DateOnly(today.Year, today.Month, today.Day);
            var dailyRevenue = await _context.DailyRevenueSummaries
                .FirstOrDefaultAsync(dr => dr.Date == dateOnly);

            if (dailyRevenue != null)
            {
                dailyRevenue.TotalRevenue += amount;
                dailyRevenue.UpdatedAt = DateTime.UtcNow;
                _context.DailyRevenueSummaries.Update(dailyRevenue);
            }
            else
            {
                var newDailyRevenue = new DailyRevenueSummary
                {
                    Id = Guid.NewGuid(),
                    Date = dateOnly,
                    TotalRevenue = amount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.DailyRevenueSummaries.AddAsync(newDailyRevenue);
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServiceRevenue(Guid serviceTypeId, decimal amount)
        {
            var today = DateTime.UtcNow.Date;
            DateOnly dateOnly = new DateOnly(today.Year, today.Month, today.Day);
            var revenueEntry = await _context.ServiceRevenues
                .FirstOrDefaultAsync(sr => sr.ClinicServiceId == serviceTypeId && sr.Date == dateOnly);

            if (revenueEntry != null)
            {
                revenueEntry.Revenue += amount;
                revenueEntry.UpdatedAt = DateTime.UtcNow;
                _context.ServiceRevenues.Update(revenueEntry);
            }
            else
            {
                var newRevenueEntry = new ServiceRevenue
                {
                    Id = Guid.NewGuid(),
                    ClinicServiceId = serviceTypeId,
                    Date = dateOnly,
                    Revenue = amount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.ServiceRevenues.AddAsync(newRevenueEntry);
            }
            await   _context.SaveChangesAsync();
        }
        public async Task UpdatePromotionUsageLimit(Guid promotionId)
        {
            var promotion = await _context.Promotions.FindAsync(promotionId);
            if (promotion != null)  // Bỏ điều kiện && promotion.UsageLimit > 0
            {
                promotion.UsageLimit = Math.Max(0, promotion.UsageLimit - 1); 
                await _context.SaveChangesAsync();
            }
        }

       
        public async Task<Appointment> GetAppointmentWithDetails(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.AppointmentPromotions)
                    .ThenInclude(ap => ap.Promotion)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

    }
}
