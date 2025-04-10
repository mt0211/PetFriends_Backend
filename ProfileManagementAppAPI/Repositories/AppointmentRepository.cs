using AppAppointmentManagementAPI.DTOs.ReviewModel;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProfileManagementAppAPI.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly PetfriendsContext _context;

        public AppointmentRepository(PetfriendsContext context)
        {
            _context = context;

        }

        public async Task AddReview(Feedback reviewEntity)
        {
            await _context.Feedbacks.AddAsync(reviewEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetCategory()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.Status == 1)
                .Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Status = c.Status,
                    ClinicServices = c.ClinicServices
                        .Where(cs => cs.IsBlocked == 1 && cs.Status == "ACTIVE").ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<dynamic>> GetReview()
        {
            return await _context.Feedbacks
            .Include(r => r.User)
            .Select(r => new
            {
                r.Id,
                r.Content,
                r.Rating,
                r.CreatedAt,
                UserName = r.User.FullName,
                UserAvatar = r.User.AvatarUrl,
                UserEmail = r.User.Email,
            })
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task<Feedback> GetReviewByAppointmentId(Guid appointmentId)
        {
            return await _context.Feedbacks.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);

        }

        public async Task UpdateReview(Feedback reviewUpdateModel)
        {
            _context.Feedbacks.Attach(reviewUpdateModel);
            _context.Entry(reviewUpdateModel).Property(c => c.Content).IsModified = true;
            _context.Entry(reviewUpdateModel).Property(c => c.Rating).IsModified = true;
            _context.Entry(reviewUpdateModel).Property(c => c.UpdatedAt).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task<dynamic> GetClinicInformation()
        {
            var user = await _context.Users.FirstOrDefaultAsync(email => email.Email == "petfriends.contacts@gmail.com");
            var reviewcount = await _context.Feedbacks.CountAsync();
            var rating = await _context.Feedbacks.AverageAsync(r => r.Rating);
            return new { user, reviewcount, rating };
        }

        ///Note: Phòng ngừa khi get category call api không được.
        // public async Task<dynamic> GetServiceByCategoryID(Guid categoryID)
        // {
        //     return await _context.ClinicServices
        //     .Where(s=>s.Category == categoryID)
        //     .ToListAsync();
        // }

        public async Task<IEnumerable<Pet>> GetPetListByUserId(Guid userId)
        {
            return await _context.Pets.Where(p => p.UserId == userId).ToListAsync();
        }


        public async Task<UserCart> CheckUserCart(Guid userId)
        {
            return await _context.UserCarts.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.Status == 0);
        }
        public async Task AddNewCart(UserCart userCart)
        {
            await _context.UserCarts.AddAsync(userCart);
            await _context.SaveChangesAsync();
        }
        public async Task AddNewCartItem(UserCartItem userCartItem)
        {
            await _context.UserCartItems.AddAsync(userCartItem);
            await _context.SaveChangesAsync();
        }
        public async Task<UserCartItem> CheckUserCartItemByServiceId(Guid serviceId, Guid userId)
        {
            return await _context.UserCartItems
        .Include(uc => uc.Cart)
        .Where(uc => uc.ClinicServiceId == serviceId 
            && uc.Cart.Status == 0
            && uc.Cart.UserId == userId) // Thêm điều kiện check UserId
        .FirstOrDefaultAsync();
        }
        public async Task<UserCart> GetCartByUserId(Guid userId)
        {
            var cart = await _context.UserCarts
            .Include(uc => uc.User)
            .Include(uc => uc.UserCartItems)
            .ThenInclude(uc => uc.ClinicService)
            .Include(uc => uc.UserCartItems)
            .ThenInclude(uc => uc.Pet)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.Status == 0);
            return cart;
        }
        public async Task UpdateCart(UserCart userCart)
        {
            _context.UserCarts.Attach(userCart);
            _context.Entry(userCart).Property(c => c.Datebook).IsModified = true;
            _context.Entry(userCart).Property(c => c.Notes).IsModified = true;
            _context.Entry(userCart).Property(c => c.Status).IsModified = true;
            await _context.SaveChangesAsync();
        }
        public async Task AddAppointment(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task AddAppointmentClinicService(AppointmentClinicService appointmentService)
        {
            await _context.AppointmentClinicServices.AddAsync(appointmentService);
            await _context.SaveChangesAsync();
        }
        public async Task<List<UserCartItem>> GetCartItemsByCartId(Guid cartId)
        {
            return await _context.UserCartItems
                .Where(item => item.CartId == cartId)
                .ToListAsync();
        }
        

        public async Task RemoveCartItem(UserCartItem cartItem)
        {
            var itemToRemove = await _context.UserCartItems
            .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
        
            if (itemToRemove != null)
            {
                _context.UserCartItems.Remove(itemToRemove);
                await _context.SaveChangesAsync();
            }
        }
        public async Task RemoveCart(Guid cartId)
        {
           var cartToRemove = await _context.UserCarts
        .FirstOrDefaultAsync(c => c.Id == cartId);
    
        if (cartToRemove != null)
        {
            _context.UserCarts.Remove(cartToRemove);
            await _context.SaveChangesAsync();
        }
        }
        public async Task<List<Promotion>> GetPromotionTypeAllMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "All Customers" && p.Status == "Active" && p.UsageLimit > 0).ToListAsync();
        }

        public async Task<List<Promotion>> GetPromotionTypeNewMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "First-Time Visitors" && p.Status == "Active" && p.UsageLimit > 0).ToListAsync();
        }
        public async Task<List<Promotion>> GetPromotionTypeLoyaltyMember()
        {
            return await _context.Promotions.Where(p => p.TargetGroup == "Loyalty Members" && p.Status == "Active" && p.UsageLimit > 0).ToListAsync();
        }
        public async Task<User> GetUserByUserId(Guid userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }


        //APPLY PROMOTION
        public async Task<Promotion> GetPromotionById(Guid promotionId)
        {
            return await _context.Promotions.FirstOrDefaultAsync(p => p.Id == promotionId);
        }

        public async Task AddAppointmentPromotion(AppointmentPromotion appointmentPromotion)
        {
            await _context.AppointmentPromotions.AddAsync(appointmentPromotion);
            await _context.SaveChangesAsync();
        }

        //BOOKING HISTORY
        public async Task<List<Appointment>> GetBookingHistory(Guid userId)
        {
            return await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a=> a.Feedbacks)
            .Include(a => a.AppointmentClinicServices)
            .ThenInclude(a => a.ClinicService)
            .Where(a => a.UserId == userId).ToListAsync();
        }
        public async Task<List<AppointmentPromotion>> GetListPromotionByAppointmentId(Guid appointmentId)
        {
            return await _context.AppointmentPromotions
            .Include(a => a.Promotion)
            .Where(a => a.AppointmentId == appointmentId)
            .ToListAsync();
        }

        public async Task CancelAppointment(Guid appointmentId)
        {
            var appointment = await _context.Appointments
        .AsNoTracking()  // Không track thay đổi của entity
        .FirstOrDefaultAsync(a => a.Id == appointmentId);
        
        if (appointment != null)
        {
            await _context.Appointments
                .Where(a => a.Id == appointmentId)
                .ExecuteUpdateAsync(s => 
                    s.SetProperty(b => b.Status, b => "Cancelled")
                );
        }
        }
        public async Task<Appointment> GetAppointmentById(Guid appointmentId)
        {
            return await _context.Appointments.FindAsync(appointmentId);
        }


        //Update Pending Appointment
        public async Task<List<AppointmentClinicService>> GetAppointmentServices(Guid appointmentId)
        {
            return await _context.AppointmentClinicServices
                .Include(acs => acs.ClinicService)
                .Where(acs => acs.AppointmentId == appointmentId)
                .ToListAsync();
        }

        public async Task<List<AppointmentPromotion>> GetAppointmentPromotions(Guid appointmentId)
        {
            return await _context.AppointmentPromotions
                .Include(ap => ap.Promotion)
                .Where(ap => ap.AppointmentId == appointmentId)
                .ToListAsync();
        }

        public async Task RemoveAppointmentService(AppointmentClinicService service)
        {
            _context.AppointmentClinicServices.Remove(service);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAppointmentPromotion(AppointmentPromotion promotion)
        {
            _context.AppointmentPromotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAppointment(Appointment appointment)
        {
            _context.Entry(appointment).Property(x => x.StartAt).IsModified = true;
            _context.Entry(appointment).Property(x => x.Note).IsModified = true;
            _context.Entry(appointment).Property(x => x.TotalAmount).IsModified = true;
            _context.Entry(appointment).Property(x => x.DiscountAmount).IsModified = true;
            _context.Entry(appointment).Property(x => x.FinalAmount).IsModified = true;
            await _context.SaveChangesAsync();
        }
       
       //BOOKING DETAIL
        public async Task<Appointment> GetAppointmentDetailById(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Pet)
                .Include(a => a.AppointmentClinicServices)
                    .ThenInclude(acs => acs.ClinicService)
                .Include(a => a.AppointmentPromotions)
                    .ThenInclude(ap => ap.Promotion)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
        
        //CHECK REVIEW
        public async Task<bool> CheckReview(Guid appointmentId)
        {
            return await _context.Feedbacks.AnyAsync(f => f.AppointmentId == appointmentId);
        }


        //COUNT SERVICE
        public async Task<int> CountService(Guid cartId)
        {
            return await _context.UserCartItems.CountAsync(uc => uc.CartId == cartId);
        }
    }
}
