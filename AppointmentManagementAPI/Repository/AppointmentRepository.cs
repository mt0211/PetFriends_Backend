
using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
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
            // Bước 1: Lấy thông tin cơ bản của tất cả appointment
            var appointments = await _context.Appointments
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt) // Sử dụng index IX_Appointment_CreatedAt
                .Select(a => new
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    Status = a.Status,
                    Note = a.Note,
                    UserId = a.UserId,
                    GuestUserId = a.GuestUserId,
                    PetId = a.PetId,
                    GuestPetId = a.GuestPetId
                })
                .ToListAsync();

            if (!appointments.Any())
                return new List<dynamic>();

            // Bước 2: Lấy thông tin người dùng và thú cưng
            var userIds = appointments.Where(a => a.UserId.HasValue).Select(a => a.UserId.Value).Distinct().ToList();
            var guestUserIds = appointments.Where(a => a.GuestUserId.HasValue).Select(a => a.GuestUserId.Value).Distinct().ToList();
            var petIds = appointments.Where(a => a.PetId.HasValue).Select(a => a.PetId.Value).Distinct().ToList();
            var guestPetIds = appointments.Where(a => a.GuestPetId.HasValue).Select(a => a.GuestPetId.Value).Distinct().ToList();

            // Lấy thông tin người dùng
            var users = userIds.Any() ? await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => u.FullName) : new Dictionary<Guid, string>();

            var guestUsers = guestUserIds.Any() ? await _context.GuestUsers
                .AsNoTracking()
                .Where(gu => guestUserIds.Contains(gu.Id))
                .Select(gu => new { gu.Id, gu.FullName })
                .ToDictionaryAsync(gu => gu.Id, gu => gu.FullName) : new Dictionary<Guid, string>();

            // Lấy thông tin thú cưng
            var pets = petIds.Any() ? await _context.Pets
                .AsNoTracking()
                .Where(p => petIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name })
                .ToDictionaryAsync(p => p.Id, p => p.Name) : new Dictionary<Guid, string>();

            var guestPets = guestPetIds.Any() ? await _context.GuestPets
                .AsNoTracking()
                .Where(gp => guestPetIds.Contains(gp.Id))
                .Select(gp => new { gp.Id, gp.Name })
                .ToDictionaryAsync(gp => gp.Id, gp => gp.Name) : new Dictionary<Guid, string>();

            // Bước 3: Lấy thông tin dịch vụ
            var appointmentIds = appointments.Select(a => a.Id).ToList();
            var servicesByAppointment = await _context.AppointmentClinicServices
                .AsNoTracking()
                .Where(acs => appointmentIds.Contains(acs.AppointmentId))
                .Join(_context.ClinicServices,
                    acs => acs.ClinicServiceId,
                    cs => cs.Id,
                    (acs, cs) => new
                    {
                        AppointmentId = acs.AppointmentId,
                        ServiceName = cs.Name
                    })
                .GroupBy(x => x.AppointmentId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ServiceName).ToList());

            // Bước 4: Kết hợp tất cả thông tin
            var result = appointments.Select(a => new
            {
                Id = a.Id,
                CreatedAt = a.CreatedAt,
                StartAt = a.StartAt,
                EndAt = a.EndAt,
                Status = a.Status,
                Note = a.Note,
                UserName = a.UserId.HasValue && users.ContainsKey(a.UserId.Value) 
                    ? users[a.UserId.Value] 
                    : (a.GuestUserId.HasValue && guestUsers.ContainsKey(a.GuestUserId.Value) 
                        ? guestUsers[a.GuestUserId.Value] 
                        : null),
                PetName = a.PetId.HasValue && pets.ContainsKey(a.PetId.Value) 
                    ? pets[a.PetId.Value] 
                    : (a.GuestPetId.HasValue && guestPets.ContainsKey(a.GuestPetId.Value) 
                        ? guestPets[a.GuestPetId.Value] 
                        : null),
                ServiceNames = servicesByAppointment.ContainsKey(a.Id) 
                    ? servicesByAppointment[a.Id] 
                    : new List<string>()
            }).ToList<dynamic>();

            return result;
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

        public async Task<List<AppointmentPromotion>> GetAppointmentPromotions(Guid appointmentId)
        {
            return await _context.AppointmentPromotions
                .Include(ap => ap.Promotion)
                .Where(ap => ap.AppointmentId == appointmentId)
                .ToListAsync();
        }

        public async Task UpdatePromotion(Promotion promotion)
        {
            _context.Promotions.Attach(promotion);
            _context.Entry(promotion).Property(p => p.UsageLimit).IsModified = true;
            await _context.SaveChangesAsync();
        }
       
       public async Task<List<ClinicService>> GetVaccinationServices(List<Guid> serviceIds)
        {
            return await _context.ClinicServices
                .Include(cs => cs.CategoryNavigation)
                .Where(cs => serviceIds.Contains(cs.Id) && 
                    cs.CategoryNavigation.Name.ToLower() == "Vaccination")
                .ToListAsync();
        }

        public async Task<UserPetVaccine> GetPetVaccineByNameAndPetId(string vaccineName, Guid petId)
        {
            return await _context.UserPetVaccines
                .Include(upv => upv.UserPetVaccineDoses)
                .Where(upv => upv.PetId == petId && upv.Name == vaccineName)
                .FirstOrDefaultAsync();
        }

        public async Task<Vaccine> GetVaccineByName(string name)
        {
            return await _context.Vaccines
                .Where(v => v.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task AddUserPetVaccine(UserPetVaccine petVaccine)
        {
            await _context.UserPetVaccines.AddAsync(petVaccine);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserPetVaccineDose(UserPetVaccineDose petVaccineDose)
        {
            await _context.UserPetVaccineDoses.AddAsync(petVaccineDose);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserPetVaccine(UserPetVaccine petVaccine)
        {
            _context.UserPetVaccines.Update(petVaccine);
            await _context.SaveChangesAsync();
        }

        //FIX API ADD APPOINTMENT
        // 1) Tìm user thật (User) theo phone/email
        public async Task<Guid?> GetUserIdByPhoneOrEmail(string? phone, string? email)
        {
            // Nếu không có phone/email gì hết thì bỏ qua
            if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
                return null;

            var userId = await _context.Users
                .Where(u =>
                    (!string.IsNullOrEmpty(phone) && u.PhoneNumber == phone)
                    || (!string.IsNullOrEmpty(email) && u.Email == email)
                )
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            // Nếu FirstOrDefaultAsync() trả về Guid.Empty => ko tìm thấy => ta return null
            return userId == Guid.Empty ? null : userId;
        }

        // 2) Tạo hoặc lấy GuestUser
        public async Task<GuestUser> CreateOrGetGuestUser(AppointmentAddModel appointment)
        {
            // Nếu không có phone number => không thể lookup GuestUser
            if (string.IsNullOrEmpty(appointment.GuestPhoneNumber))
                return null; // tuỳ bạn xử lý

            // Tìm GuestUser theo phone
            var existingGuest = await GetGuestUserByPhoneNumber(appointment.GuestPhoneNumber);
            if (existingGuest != null)
            {
                return existingGuest;
            }

            // Không có => tạo mới
            var guestUserId = Guid.NewGuid();
            var newGuestUser = new GuestUser
            {
                Id = guestUserId,
                PhoneNumber = appointment.GuestPhoneNumber,
                FullName = appointment.GuestFullName,
                Email = appointment.GuestEmail,
                Address = appointment.Address,
                CreatedAt = DateTimeOffset.Now.DateTime
            };
            await InsertGuestUser(newGuestUser);
            return newGuestUser;
        }

        // 3) Tạo hoặc lấy GuestPet
        public async Task<GuestPet> CreateOrGetGuestPet(AppointmentAddModel appointment, Guid guestUserId)
        {
            if (string.IsNullOrEmpty(appointment.GuestPetName))
                return null; // tuỳ bạn xử lý

            // Tìm GuestPet theo tên
            var existingGuestPet = await GetGuestPetByNameAndGuestUserId(appointment.GuestPetName, guestUserId);
            if (existingGuestPet != null)
            {
                return existingGuestPet;
            }

            // Không có => tạo mới
            var guestPetId = Guid.NewGuid();
            var newGuestPet = new GuestPet
            {
                Id = guestPetId,
                Name = appointment.GuestPetName,
                DateOfBirth = appointment.GuestPetDateOfBirth,
                Gender = appointment.GuestPetGender,
                Species = appointment.GuestPetSpecies,
                GuestUserId = guestUserId,
                CreatedAt = DateTimeOffset.Now.DateTime
            };
            await InsertGuestPet(newGuestPet);
            return newGuestPet;
        }

    }

}