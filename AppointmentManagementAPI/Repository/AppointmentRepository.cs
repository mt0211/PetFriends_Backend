using AppointmentManagementAPI.DTOs.ResultModel.AppointmentDTOs;
using AutoMapper;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AppointmentManagementAPI.Repository
{
    
    public class CacheEntry<T>
    {
        public T Data { get; set; }
        public long Created { get; set; }
    }

    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        private readonly PetfriendsContext _context;
        private readonly ILogger<AppointmentRepository> _logger;
        private readonly IMemoryCache _cache;

        public AppointmentRepository(PetfriendsContext context, ILogger<AppointmentRepository> logger, IMemoryCache cache) : base(context)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IEnumerable<dynamic>> GetAllApointment()
        {
           string cacheKey = "all_appointments_no_pagination";
    
            // Kiểm tra timestamp vô hiệu hóa cache
            long? invalidationTimestamp = null;
            _cache.TryGetValue("appointment_cache_invalidation_timestamp", out invalidationTimestamp);
            
            // Kiểm tra cache trước
            if (_cache.TryGetValue(cacheKey, out CacheEntry<IEnumerable<dynamic>> cachedEntry))
            {
                // So sánh timestamp entry cache với timestamp vô hiệu hóa
                if (invalidationTimestamp == null || cachedEntry.Created > invalidationTimestamp)
                {
                    _logger.LogInformation("Returning cached appointment list without pagination");
                    return cachedEntry.Data;
                }
                
                _logger.LogInformation("Cache entry found but invalidated, fetching fresh data");
            }
            
            try
            {
                _logger.LogInformation("Cache miss. Fetching all appointments from database without pagination");
                
                // Truy vấn chính - chỉ lấy thông tin appointments cơ bản
                var appointmentsQuery = _context.Appointments
                    .AsNoTracking()
                    .OrderByDescending(a => a.CreatedAt)
                    .TagWith("GetAllAppointmentsWithoutPagination_Main_Query")
                    .Select(a => new 
                    {
                        a.Id,
                        a.CreatedAt,
                        a.StartAt,
                        a.EndAt,
                        a.Status,
                        a.Note,
                        UserId = a.UserId,
                        GuestUserId = a.GuestUserId,
                        PetId = a.PetId,
                        GuestPetId = a.GuestPetId
                    });
                
                var appointments = await appointmentsQuery.ToListAsync();
                
                // Nếu không có appointments, trả về danh sách rỗng sớm
                if (!appointments.Any())
                {
                    return new List<dynamic>();
                }
                
                // Lấy danh sách các ID của appointments đã tìm thấy
                var appointmentIds = appointments.Select(a => a.Id).ToList();
                
                // Lấy user names và pet names
                var userIds = appointments.Where(a => a.UserId.HasValue).Select(a => a.UserId.Value).ToList();
                var guestUserIds = appointments.Where(a => a.GuestUserId.HasValue).Select(a => a.GuestUserId.Value).ToList();
                var petIds = appointments.Where(a => a.PetId.HasValue).Select(a => a.PetId.Value).ToList();
                var guestPetIds = appointments.Where(a => a.GuestPetId.HasValue).Select(a => a.GuestPetId.Value).ToList();
                
                // Dictionary để lưu trữ tên
                var userNameDict = new Dictionary<Guid, string>();
                var petNameDict = new Dictionary<Guid, string>();
                
                // Chỉ truy vấn nếu có dữ liệu
                if (userIds.Any())
                {
                    var users = await _context.Users
                        .Where(u => userIds.Contains(u.Id))
                        .Select(u => new { u.Id, u.FullName })
                        .ToDictionaryAsync(u => u.Id, u => u.FullName);
                    
                    foreach (var user in users)
                    {
                        userNameDict[user.Key] = user.Value;
                    }
                }
                
                if (guestUserIds.Any())
                {
                    var guestUsers = await _context.GuestUsers
                        .Where(gu => guestUserIds.Contains(gu.Id))
                        .Select(gu => new { gu.Id, gu.FullName })
                        .ToDictionaryAsync(gu => gu.Id, gu => gu.FullName);
                    
                    foreach (var guestUser in guestUsers)
                    {
                        userNameDict[guestUser.Key] = guestUser.Value;
                    }
                }
                
                if (petIds.Any())
                {
                    var pets = await _context.Pets
                        .Where(p => petIds.Contains(p.Id))
                        .Select(p => new { p.Id, p.Name })
                        .ToDictionaryAsync(p => p.Id, p => p.Name);
                    
                    foreach (var pet in pets)
                    {
                        petNameDict[pet.Key] = pet.Value;
                    }
                }
                
                if (guestPetIds.Any())
                {
                    var guestPets = await _context.GuestPets
                        .Where(gp => guestPetIds.Contains(gp.Id))
                        .Select(gp => new { gp.Id, gp.Name })
                        .ToDictionaryAsync(gp => gp.Id, gp => gp.Name);
                    
                    foreach (var guestPet in guestPets)
                    {
                        petNameDict[guestPet.Key] = guestPet.Value;
                    }
                }
                
                // Lấy service names cho mỗi appointment
                var serviceNameDict = new Dictionary<Guid, List<string>>();
                
                var serviceQuery = await _context.AppointmentClinicServices
                    .Where(acs => appointmentIds.Contains(acs.AppointmentId))
                    .Select(acs => new 
                    {
                        acs.AppointmentId,
                        ServiceName = acs.ClinicService.Name
                    })
                    .ToListAsync();
                
                foreach (var service in serviceQuery)
                {
                    if (!serviceNameDict.ContainsKey(service.AppointmentId))
                    {
                        serviceNameDict[service.AppointmentId] = new List<string>();
                    }
                    serviceNameDict[service.AppointmentId].Add(service.ServiceName);
                }
                
                // Kết hợp kết quả
                var result = appointments.Select(a => new
                {
                    Id = a.Id,
                    CreatedAt = a.CreatedAt,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    Status = a.Status,
                    Note = a.Note,
                    UserName = GetName(a.UserId, a.GuestUserId, userNameDict),
                    PetName = GetName(a.PetId, a.GuestPetId, petNameDict),
                    ServiceNames = serviceNameDict.ContainsKey(a.Id) ? serviceNameDict[a.Id] : new List<string>()
                }).ToList<dynamic>();

                // Lưu vào cache với thời gian sống là 2 phút
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                    .SetPriority(CacheItemPriority.High);
                
                // Lưu dữ liệu kèm timestamp tạo
                var entry = new CacheEntry<IEnumerable<dynamic>>
                {
                    Data = result,
                    Created = DateTime.Now.Ticks
                };
                
                _cache.Set(cacheKey, entry, cacheOptions);
                
                return result;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError($"Error in GetAllAppointmentsWithoutPagination: {ex.Message}");
                throw;
            }
        }
        
        // Helper method để lấy tên từ dictionary
        private string GetName(Guid? regularId, Guid? guestId, Dictionary<Guid, string> nameDict)
        {
            if (regularId.HasValue && nameDict.TryGetValue(regularId.Value, out string regularName))
                return regularName;
                
            if (guestId.HasValue && nameDict.TryGetValue(guestId.Value, out string guestName))
                return guestName;
                
            return null;
        }

        // Xóa cache khi có thay đổi dữ liệu
        public void InvalidateAppointmentCache()
        {
            // Vì không thể liệt kê tất cả cache keys trong IMemoryCache,
            // chúng ta sẽ sử dụng một regex pattern để xóa cache trên đầu mỗi lần
            var cacheEntriesPattern = "appointments_page*";
            
            // Ghi log
            _logger.LogInformation("Invalidating appointment cache with pattern: {Pattern}", cacheEntriesPattern);
            
            // Đối với IMemoryCache, không có cách trực tiếp để xóa bằng pattern
            // Vì vậy ta cần tạo một cache key mới để đánh dấu thời điểm cache trở nên không hợp lệ
            var timestamp = DateTime.Now.Ticks;
            _cache.Set("appointment_cache_invalidation_timestamp", timestamp);
            
            _logger.LogInformation("Appointment cache invalidated at {Timestamp}", timestamp);
        }

        // Ghi đè phương thức Insert
        public new async Task Insert(Appointment entity)
        {
            await base.Insert(entity);
            InvalidateAppointmentCache();
        }

        // Ghi đè phương thức Update
        public new async Task Update(Appointment entity)
        {
            await base.Update(entity);
            InvalidateAppointmentCache();
        }

        // Ghi đè phương thức Remove
        public new async Task Remove(Appointment entity)
        {
            await base.Remove(entity);
            InvalidateAppointmentCache();
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
            InvalidateAppointmentCache();
        }

        public async Task RemoveAppointmentClinicService(AppointmentClinicService appointmentClinicService)
        {
            _context.AppointmentClinicServices.Remove(appointmentClinicService);
            await _context.SaveChangesAsync();
            InvalidateAppointmentCache();
        }

        public async Task RemoveAppointmentClinicServiceById(Guid serviceId)
        {
            // Sử dụng lệnh SQL trực tiếp hoặc tìm entity trước
            var service = await _context.AppointmentClinicServices.FindAsync(serviceId);
            if (service != null)
            {
                _context.AppointmentClinicServices.Remove(service);
                await _context.SaveChangesAsync();
                InvalidateAppointmentCache();
            }
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
            await _context.SaveChangesAsync();
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
        //FIX API UPDATE APPOINTMENT
        public async Task<GuestUser> GetGuestUserByID(Guid? id)
        {
            return await _context.GuestUsers.FindAsync(id);
        }
        public async Task<GuestPet> GetGuestPetByID(Guid? id)
        {
            return await _context.GuestPets.FindAsync(id);
        }

        public async Task UpdateGuestUser(GuestUser guestUser)
        {
            _context.GuestUsers.Update(guestUser);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateGuestPet(GuestPet guestPet)
        {
            _context.GuestPets.Update(guestPet);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByID(Guid? id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<Pet> GetPetByID(Guid? id)
        {
            return await _context.Pets.FindAsync(id);
        }

        public async Task UpdateAppointmentBasicInfo(Guid appointmentId, string status, DateTime? startAt, string note, DateTime? endAt = null)
        {
            // Kiểm tra xem entity đã được theo dõi chưa
            var existingEntity = _context.ChangeTracker.Entries<Appointment>()
                .FirstOrDefault(e => e.Entity.Id == appointmentId);

            if (existingEntity != null)
            {
                // Nếu đã theo dõi, detach nó
                existingEntity.State = EntityState.Detached;
            }

            // Tiếp tục với cách tiếp cận hiện tại
            var appointment = new Appointment
            {
                Id = appointmentId,
                Status = status,
                StartAt = startAt,
                Note = note
            };

            if (endAt.HasValue)
            {
                appointment.EndAt = endAt;
            }

            _context.Appointments.Attach(appointment);
            _context.Entry(appointment).Property(x => x.Status).IsModified = true;
            _context.Entry(appointment).Property(x => x.StartAt).IsModified = true;
            _context.Entry(appointment).Property(x => x.Note).IsModified = true;

            if (endAt.HasValue)
            {
                _context.Entry(appointment).Property(x => x.EndAt).IsModified = true;
            }

            await _context.SaveChangesAsync();
            
            // Vô hiệu hóa cache sau khi cập nhật
            InvalidateAppointmentCache();
        }
        
        public async Task<int> GetAppointmentCount()
        {
            string cacheKey = "appointment_total_count";
            
            // Kiểm tra timestamp vô hiệu hóa cache
            long? invalidationTimestamp = null;
            _cache.TryGetValue("appointment_cache_invalidation_timestamp", out invalidationTimestamp);
            
            // Kiểm tra cache trước
            if (_cache.TryGetValue(cacheKey, out CacheEntry<int> cachedEntry))
            {
                // So sánh timestamp entry cache với timestamp vô hiệu hóa
                if (invalidationTimestamp == null || cachedEntry.Created > invalidationTimestamp)
                {
                    _logger.LogInformation("Returning cached appointment count");
                    return cachedEntry.Data;
                }
                
                _logger.LogInformation("Cache count entry found but invalidated, fetching fresh data");
            }
            
            try
            {
                _logger.LogInformation("Cache miss for count. Counting appointments from database");
                
                // Sử dụng truy vấn tối ưu hơn chỉ đếm ID thay vì COUNT(*)
                var count = await _context.Appointments
                    .AsNoTracking() // Không theo dõi entity
                    .Select(a => a.Id) // Chỉ lấy ID, không lấy các cột khác 
                    .CountAsync();
                
                // Lưu vào cache với thời gian sống dài hơn vì count thay đổi chậm hơn
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                    .SetPriority(CacheItemPriority.High);
                
                // Lưu dữ liệu kèm timestamp tạo
                var entry = new CacheEntry<int>
                {
                    Data = count,
                    Created = DateTime.Now.Ticks
                };
                
                _cache.Set(cacheKey, entry, cacheOptions);
                
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAppointmentCount: {ex.Message}");
                throw;
            }
        }

        

        public async Task<List<Pet>> GetPetsByPhoneOrEmail(string? phone, string? email)
        {
            try
            {
                if (string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
                {
                    return new List<Pet>();
                }

                // Tìm user dựa trên phone hoặc email
                var user = await _context.Users
                    .Where(u => (!string.IsNullOrEmpty(phone) && u.PhoneNumber == phone)
                           || (!string.IsNullOrEmpty(email) && u.Email == email))
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new List<Pet>();
                }

                // Lấy danh sách pet của user
                return await _context.Pets
                    .Where(p => p.UserId == user.Id)
                    .AsNoTracking()
                    .Select(p => new Pet
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DateOfBirth = p.DateOfBirth,
                        Gender = p.Gender,
                        Species = p.Species,
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetPetsByPhoneOrEmail: {ex.Message}");
                throw;
            }
        }

        //Delete appointment
             public async Task DeleteAppointment(Appointment entity)
            {
                // First delete related activities
                var activities = await _context.Activities
                    .Where(a => a.AppointmentId == entity.Id)
                    .ToListAsync();
                
                _context.Activities.RemoveRange(activities);
                await _context.SaveChangesAsync();

                // Then delete the appointment
                _context.Appointments.Remove(entity);
                await _context.SaveChangesAsync();
                
                InvalidateAppointmentCache();
            }

        public async Task<ClinicService> GetClinicServiceById(Guid serviceId)
        {
            return await _context.ClinicServices
            .Where(cs => cs.Id == serviceId)
            .FirstOrDefaultAsync();
        }
            
            
    }
}