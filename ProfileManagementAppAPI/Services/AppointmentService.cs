using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using ProfileManagementAppAPI.DTOs;

using ProfileManagementAppAPI.Repositories;
using ProfileManagementAppAPI.Utilities;
using MySqlX.XDevAPI.Common;
using AppAppointmentManagementAPI.DTOs.ReviewModel;
using ProfileManagementAppAPI.DTOs.CategoryClinicServiceDTO;
using ProfileManagementAppAPI.DTOs.AppointmentDTOs;
using ProfileManagementAppAPI.DTOs.PromotionDTOs;
using System.Globalization;
using ProfileManagementAppAPI.Helper;

namespace ProfileManagementAppAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IMessageBus _messageBus;
        public AppointmentService(IAppointmentRepository repository, IMessageBus messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
        }

        public async Task<ResultModel> AddReview(string token, ReviewModel reviewModel)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var checkReview = await _repository.CheckReview(reviewModel.AppointmentId);
                var appointment = await _repository.GetAppointmentById(reviewModel.AppointmentId);
                if(checkReview)
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "Appointment already has review";
                    return result;
                }
                var reviewEntity = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Content = reviewModel.Content,
                    Rating = reviewModel.Rating,
                    UserId = id,
                    AppointmentId = reviewModel.AppointmentId,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                };
                if (reviewModel.Rating > 5 || reviewModel.Rating < 0)
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "Rating less than 5 and more than 0";
                    return result;
                }
                if ( reviewEntity.CreatedAt - appointment.EndAt> TimeSpan.FromDays(30))
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "Review edits are limited to one time within 30 days of the appointment. Further changes are not allowed.";
                    return result;
                }
                await _repository.AddReview(reviewEntity);

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = reviewModel;
                result.Message = "Successfully added new review";
                _messageBus.PublishFeedbacktActivity
                (
                    "FEEDBACK_RECEIVED", reviewEntity.Id
                );

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> GetCategory(string token)
        {
            var result = new ResultModel();

    try
    {
        var categories = await _repository.GetCategory();
        if (categories == null || !categories.Any())
        {
            result.IsSuccess = false;
            result.Code = 404;
            result.Message = "Not found category";
            return result;
        }

        var categoriesDto = categories.Select(c => new CategoryListReqModel
        {
            CategoryId = c.Id,
            CategoryName = c.Name,
            CategoryStatus = c.Status,
            ClinicServices = c.ClinicServices
                .Where(s => s.IsBlocked == 1 && s.Status == "ACTIVE")
                .Select(s => new ServiceListReqModel
                {
                    ServiceId = s.Id,
                    ServiceName = s.Name,
                    ServiceDescription = s.Description,
                    ServiceCreateAt = s.CreateAt,
                    ServicePrice = s.Price,
                    ServiceStatus = s.Status,
                    ServiceEstimateTime = s.EstimateTime,
                    ServiceDiscountAmount = s.DiscountAmount,
                    ServiceDiscountFrom = s.DiscountFrom,
                    ServiceDiscountTo = s.DiscountTo,
                    ServiceImage = s.Image,
                    ServiceDiscountedPrice = s.DiscountedPrice,
                    ServiceIsBlocked = s.IsBlocked
                }).ToList()
        }).ToList();

        result.IsSuccess = true;
        result.Code = 200;
        result.Data = categoriesDto;
        result.Message = "Successfully get all categories";
        }
        catch (Exception ex)
        {
            result.Code = 500;
            result.ResponseFailed = ex.InnerException != null
                ? ex.InnerException.Message + "\n" + ex.StackTrace
                : ex.Message + "\n" + ex.StackTrace;
        }

        return result;
        }

        public async Task<ResultModel> GetListReview(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {

                var review = await _repository.GetReview();
                if (review == null || !review.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found review";
                    return result;
                }
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = review;
                result.Message = "Successfully get all review";


            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;

            }
            return result;
        }

        public async Task<ResultModel> UpdateReview(string token, ReviewUpdateModel reviewUpdateModel)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var existingReview = await _repository.GetReviewByAppointmentId(reviewUpdateModel.AppointmentId);
                var appointment = await _repository.GetAppointmentById(reviewUpdateModel.AppointmentId);
                if (existingReview == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Review not found";
                    return result;
                }
                
                if (existingReview.UserId.ToString() != userId)
                {
                    result.IsSuccess = false;
                    result.Code = 403; // Forbidden
                    result.Message = "You are not authorized to update this review";
                    return result;
                }

                if (reviewUpdateModel.Rating > 5 || reviewUpdateModel.Rating < 0)
                {
                    result.IsSuccess = false;
                    result.Code = 403; // Forbidden
                    result.Message = "Rating less than 5 and more than 1";
                    return result;
                }

                if(existingReview.UpdatedAt!= null)
                {
                    result.IsSuccess = false;
                    result.Code = 403; // Forbidden
                    result.Message = "Review only update once time";
                    return result;
                }
                existingReview.Content = reviewUpdateModel.Content;
                existingReview.Rating = reviewUpdateModel.Rating;
                existingReview.UpdatedAt = DateTime.UtcNow.AddHours(7);
                if (existingReview.CreatedAt - appointment.EndAt > TimeSpan.FromDays(30))
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "Review edits are limited to one time within 30 days of the appointment. Further changes are not allowed.";
                    return result;
                }
                await _repository.UpdateReview(existingReview);


                result.IsSuccess = true;
                result.Code = 200;
                result.Data = reviewUpdateModel;
                result.Message = "Successfully update review";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetClinicInformation(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {

                var clinicInformation = await _repository.GetClinicInformation();
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = clinicInformation;
                result.Message = "Successfully get all review";


            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;

            }
            return result;
        }
///Note: Phòng ngừa khi get category call api không được.
        // public async Task<ResultModel> GetServiceByCategoryID(string token, Guid categoryID)
        // {
        //     var result = new ResultModel();
        //     var userId = Encoder.DecodeToken(token, "userid");
        //     if (!Guid.TryParse(userId, out Guid id))
        //     {
        //         result.IsSuccess = false;
        //         result.Code = 400; // Bad request
        //         result.Message = "Invalid user ID";
        //         return result;
        //     }
        //     if (userId == null)
        //     {
        //         result.IsSuccess = false;
        //         result.Code = 400; // Bad request
        //         result.Message = "Please authorize";
        //         return result;
        //     }
        //     try
        //     {
        //         var services = await _repository.GetServiceByCategoryID(categoryID);
        //         //Success response
        //         result.IsSuccess = true;
        //         result.Code = 200;
        //         result.Data = services;
        //         result.Message = "Successfully get all review";
        //     }
        //     catch (Exception ex)
        //     {
        //         result.Code = 500;
        //         result.ResponseFailed = ex.InnerException != null
        //             ? ex.InnerException.Message + "\n" + ex.StackTrace
        //             : ex.Message + "\n" + ex.StackTrace;

        //     }
        //     return result;

        // }

        public async Task<ResultModel> GetPetListByUserId(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var pets = await _repository.GetPetListByUserId(id);
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = pets;
                result.Message = "Successfully get all review";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;

            }
            return result;
        }       
        //BOOK APPOINTMENT
        public async Task<ResultModel> AddToCart(string token, AddToCartDTO addToCartDTO)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var checkUserCart = await _repository.CheckUserCart(id);
                if(checkUserCart == null)
                {
                    var newCart = new UserCart
                    {
                        Id = Guid.NewGuid(),
                        UserId = id,
                        Status = 0
                    };
                    await _repository.AddNewCart(newCart);
                    var newCartItem = new UserCartItem
                    {
                      Id = Guid.NewGuid(),
                      CartId = newCart.Id,
                      ClinicServiceId = addToCartDTO.ServiceId,
                      PetId = addToCartDTO.PetId
                    };
                    await _repository.AddNewCartItem(newCartItem);
                }
                else
                {
                    var checkUserCartItem = await _repository.CheckUserCartItemByServiceId(addToCartDTO.ServiceId, id);
                
                    if(checkUserCartItem != null)
                    {
                        result.IsSuccess = false;
                        result.Code = 200;
                        result.Message = "Service already in cart";
                        return result;
                    }
                     var existingCartItems = await _repository.GetCartItemsByCartId(checkUserCart.Id);
            
                    if (existingCartItems != null && existingCartItems.Any())
                    {
                        var firstPetId = existingCartItems.First().PetId;
                        if (firstPetId != addToCartDTO.PetId)
                        {
                            result.IsSuccess = false;
                            result.Code = 400;
                            result.Message = "Please choose the same pet for all services";
                            return result;
                        }
                    }
                    var newCartItem = new UserCartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = checkUserCart.Id,
                        ClinicServiceId = addToCartDTO.ServiceId,
                        PetId = addToCartDTO.PetId
                    };
                    await _repository.AddNewCartItem(newCartItem);
                }
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = addToCartDTO;
                result.Message = "Successfully add to cart";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetCartByUserId(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var cart = await _repository.GetCartByUserId(id);
                if (cart == null)
                {
                    result.IsSuccess = false;
                    result.Code = 200;
                    result.Message = "Cart is empty";
                    return result;
                }

                // Map data to DTO
                var cartDetail = new CartDetailDTO
                {
                    CartId = cart.Id,
                    UserName = cart.User.FullName,
                    UserPhone = cart.User.PhoneNumber,
                    UserEmail = cart.User.Email,
                    UserAddress = cart.User.Address,
                    DateBook = cart.Datebook,
                    Notes = cart.Notes,
                    Services = cart.UserCartItems.Select(item => new CartServiceDTO
                    {
                        ServiceId = item.ClinicService.Id,
                        ServiceName = item.ClinicService.Name,
                        EstimateTime = item.ClinicService.EstimateTime,
                        DiscountedPrice = item.ClinicService.DiscountedPrice,
                        PetId = item.Pet.Id,
                        PetName = item.Pet.Name
                    }).ToList(),
                    TotalAmount = cart.UserCartItems.Sum(item => item.ClinicService.DiscountedPrice)
                };

            result.IsSuccess = true;
            result.Code = 200;
            result.Data = cartDetail;
            result.Message = "Successfully get cart";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;

            }
            return result;
        }
        public async Task<ResultModel> BookAppointment(string token, UpdateCartDTO updateCartDTO)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                if (!DateTime.TryParseExact(updateCartDTO.Date, "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid date format. Use yyyy-MM-dd";
                    return result;
                }

                if (!TimeFormatHelper.TryParseTime(updateCartDTO.Time, out DateTime timeValue))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid time format. Use h:mm AM/PM or h:mm SA/CH (e.g., 1:22 AM or 1:22 SA)";
                    return result;
                }
                 var appointmentDateTime = dateValue.Date.Add(timeValue.TimeOfDay);

                var cart = await _repository.GetCartByUserId(id);
                if (cart == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Cart not found";
                    return result;
                }
               


                // Tính tổng giá trị dịch vụ
                decimal totalAmount = cart.UserCartItems.Sum(item => item.ClinicService.DiscountedPrice ?? 0);
                decimal discountAmount = 0;
                
                // Danh sách các promotion đã áp dụng
                var appliedPromotions = new List<(Promotion Promotion, decimal DiscountAmount)>();
                
                // Kiểm tra và áp dụng các promotion nếu có
                if (updateCartDTO.PromotionIds?.Any() == true)
                {
                    // Giới hạn số lượng promotion tối đa là 2
                    var promotionIds = updateCartDTO.PromotionIds.Take(2).ToList();
                    var user = await _repository.GetUserByUserId(id);
                    
                    foreach (var promotionId in promotionIds)
                    {
                        var promotion = await _repository.GetPromotionById(promotionId);
                        if (promotion != null && promotion.Status == "Active")
                        {
                            bool canApply = false;
                            
                            if (promotion.TargetGroup == "All Customers")
                            {
                                canApply = true;
                            }
                            else if (promotion.TargetGroup == "First-Time Visitors" && user.TypeGroup == "First-Time Visitors")
                            {
                                canApply = true;
                            }
                            else if (promotion.TargetGroup == "Loyalty Members" && user.TypeGroup == "Loyalty Members")
                            {
                                canApply = true;
                            }

                            if (canApply)
                            {
                                decimal currentDiscountAmount = 0;
                                
                                if (promotion.Type == 0) // Phần trăm
                                {
                                    currentDiscountAmount = totalAmount * (promotion.DiscountDetail.GetValueOrDefault() / 100);
                                }
                                else if (promotion.Type == 1) // Số tiền cố định
                                {
                                    currentDiscountAmount = promotion.DiscountDetail.GetValueOrDefault();
                                }
                                
                                appliedPromotions.Add((promotion, currentDiscountAmount));
                                discountAmount += currentDiscountAmount;
                            }
                        }
                    }
                    
                    // Đảm bảo tổng giảm giá không vượt quá tổng tiền
                    if (discountAmount > totalAmount)
                    {
                        discountAmount = totalAmount;
                    }
                }
                foreach (var checkpromotion in appliedPromotions)
                {
                    if (checkpromotion.Promotion.EndDate < appointmentDateTime)
                    {
                        result.IsSuccess = false;
                        result.Code = 200;
                        result.Message = $"Promotion {checkpromotion.Promotion.Name} has expired after your appointment {appointmentDateTime}!";
                        return result;
                    }
                }
                // Tạo appointment mới
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    PetId = cart.UserCartItems.FirstOrDefault()?.PetId,
                    CreatedAt = DateTime.UtcNow.AddHours(7),
                    StartAt = appointmentDateTime,
                    Status = "Pending",
                    Note = updateCartDTO.Notes,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = totalAmount - discountAmount,
                    IsReminderSent = false,
                    IsReminder1HourSent = false,
                };
                await _repository.AddAppointment(appointment);

                // Thêm các dịch vụ vào appointment
                foreach (var cartItem in cart.UserCartItems)
                {
                    var appointmentService = new AppointmentClinicService
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        ClinicServiceId = cartItem.ClinicServiceId.Value,
                        DateGiven = appointmentDateTime,
                        Notes = updateCartDTO.Notes,
                        Price = cartItem.ClinicService.DiscountedPrice
                    };
                    await _repository.AddAppointmentClinicService(appointmentService);
                }

                // Lưu thông tin các promotion đã áp dụng (nếu có)
                foreach (var (promotion, promotionDiscountAmount) in appliedPromotions)
                {
                    var appointmentPromotion = new AppointmentPromotion
                    {
                        Id = Guid.NewGuid(),
                        AppointmentId = appointment.Id,
                        PromotionId = promotion.Id,
                        DiscountAmount = promotionDiscountAmount,
                        CreateAt = DateTime.UtcNow
                    };
                    await _repository.AddAppointmentPromotion(appointmentPromotion);
                }

                // Cập nhật trạng thái giỏ hàng
                var updateCart = new UserCart
                {
                    Id = updateCartDTO.CartId,
                    Datebook = appointmentDateTime,
                    Notes = updateCartDTO.Notes,
                    Status = 1 // Đã đặt lịch
                };
                await _repository.UpdateCart(updateCart);

                // Tạo kết quả trả về
                var bookingResult = new BookingResultDTO
                {
                    AppointmentId = appointment.Id,
                    DateBook = appointmentDateTime,
                    Notes = updateCartDTO.Notes,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = totalAmount - discountAmount,
                    AppliedPromotions = appliedPromotions.Select(p => new AppliedPromotionDTO
                    {
                        PromotionId = p.Promotion.Id,
                        PromotionName = p.Promotion.Name,
                        DiscountType = p.Promotion.Type,
                        DiscountAmount = p.Promotion.DiscountDetail
                    }).ToList(),
                    Services = cart.UserCartItems.Select(item => new CartServiceDTO
                    {
                        ServiceId = item.ClinicService.Id,
                        ServiceName = item.ClinicService.Name,
                        EstimateTime = item.ClinicService.EstimateTime,
                        DiscountedPrice = item.ClinicService.DiscountedPrice,
                        PetId = item.Pet.Id,
                        PetName = item.Pet.Name
                    }).ToList()
                };

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = bookingResult;
                result.Message = "Successfully book appointment";
                _messageBus.PublishAppointmentActivity
                (
                    "APP_APPOINTMENT_CREATED",
                    appointment.Id
                );
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> RemoveServiceFromCart(string token, Guid serviceId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                // Lấy cart hiện tại của user
                var cart = await _repository.GetCartByUserId(id);
                if (cart == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Cart not found";
                    return result;
                }

                // Tìm cart item cần xóa
                var cartItem = cart.UserCartItems
                    .FirstOrDefault(item => item.ClinicServiceId == serviceId);

                if (cartItem == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Service not found in cart";
                    return result;
                }

                // Xóa cart item
                await _repository.RemoveCartItem(cartItem);

                // Kiểm tra xem cart còn item nào không
                var remainingItems = await _repository.GetCartItemsByCartId(cart.Id);
                if (remainingItems == null || !remainingItems.Any())
                {
                    // Nếu không còn item nào, xóa luôn cart
                    await _repository.RemoveCart(cart.Id);
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Message = "Service removed and cart is now empty";
                    return result;
                }

                // Lấy thông tin cart sau khi xóa
                var updatedCart = await _repository.GetCartByUserId(id);
                var cartDetail = new CartDetailDTO
                {
                    CartId = updatedCart.Id,
                    UserName = updatedCart.User.FullName,
                    UserPhone = updatedCart.User.PhoneNumber,
                    UserEmail = updatedCart.User.Email,
                    UserAddress = updatedCart.User.Address,
                    DateBook = updatedCart.Datebook,
                    Notes = updatedCart.Notes,
                    Services = updatedCart.UserCartItems.Select(item => new CartServiceDTO
                    {
                        ServiceId = item.ClinicService.Id,
                        ServiceName = item.ClinicService.Name,
                        EstimateTime = item.ClinicService.EstimateTime,
                        DiscountedPrice = item.ClinicService.DiscountedPrice,
                        PetId = item.Pet.Id,
                        PetName = item.Pet.Name
                    }).ToList()
                };

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = cartDetail;
                result.Message = "Service removed from cart successfully";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetListPromotion(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var promotionAllMember = await _repository.GetPromotionTypeAllMember();
                var user = await _repository.GetUserByUserId(id);
                 var promotionNewMember = (List<Promotion>?)null;
                 var promotionLoyaltyMember = (List<Promotion>?)null;
                if(user.TypeGroup == "First-Time Visitors")
                {
                     promotionNewMember = await _repository.GetPromotionTypeNewMember();
                }
                if(user.TypeGroup == "Loyalty Members")
                {
                    promotionLoyaltyMember = await _repository.GetPromotionTypeLoyaltyMember();
                }
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new PromotionListResDTO
                {
                    PromotionAllMember = promotionAllMember,
                    PromotionNewMember = promotionNewMember,
                    PromotionLoyaltyMember = promotionLoyaltyMember
                };
                result.Message = "Successfully get promotion";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetPromotionByID(string token, Guid promotionId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Invalid user ID";
                return result;
            }
            if (userId == null)
            {
                result.IsSuccess = false;
                result.Code = 400; // Bad request
                result.Message = "Please authorize";
                return result;
            }
            try
            {
                var promotion = await _repository.GetPromotionById(promotionId);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = promotion;
                result.Message = "Successfully get promotion";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetBookingHistory(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var appointments = await _repository.GetBookingHistory(id);
                var bookingHistoryDTOs = appointments.Select(a => new BookingHistoryDTO
                {
                    Id = a.Id,
                    UserName = a.User?.FullName ?? string.Empty,
                    UserPhone = a.User?.PhoneNumber ?? string.Empty,
                    UserEmail = a.User?.Email ?? string.Empty,
                    UserAvatar = a.User?.AvatarUrl ?? string.Empty,
                    PetName = a.Pet?.Name ?? string.Empty,
                    CreatedAt = a.CreatedAt ?? DateTime.UtcNow,
                    StartAt = a.StartAt ?? DateTime.UtcNow,
                    Status = a.Status ?? string.Empty,
                    Notes = a.Note ?? string.Empty,
                    TotalAmount = a.TotalAmount ?? 0,
                    DiscountAmount = a.DiscountAmount ?? 0,
                    FinalAmount = a.FinalAmount ?? 0,
                    ReviewContent = a.Feedbacks?.FirstOrDefault()?.Content ?? string.Empty,
                    Rating = a.Feedbacks?.FirstOrDefault()?.Rating ?? 0,
                    Services = a.AppointmentClinicServices?.Select(s => new BookingServiceDTO
                    {
                        ServiceName = s.ClinicService?.Name ?? string.Empty,
                        EstimateTime = s.ClinicService?.EstimateTime ?? string.Empty,
                        Price = s.ClinicService?.DiscountedPrice ?? 0,
                        PetName = a.Pet?.Name ?? string.Empty
                    }).ToList() ?? new List<BookingServiceDTO>()
                }).ToList();

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = bookingHistoryDTOs;
                result.Message = "Successfully get booking history";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetListPromotionByAppointmentId(string token, Guid appointmentId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var listPromotion = await _repository.GetListPromotionByAppointmentId(appointmentId);
                var listPromotionDTO = listPromotion.Select(p => new AppliedPromotionListDTO
                {
                    PromotionId = p.Promotion.Id,
                    PromotionName = p.Promotion.Name,
                    PromotionType = p.Promotion.Type,
                    PromotionStartDate = p.Promotion.StartDate,
                    PromotionEndDate = p.Promotion.EndDate,
                    PromotionTargetGroup = p.Promotion.TargetGroup,
                    PromotionCategoryId = p.Promotion.CategoryId,
                    PromotionUsageLimit = p.Promotion.UsageLimit,
                    PromotionStatus = p.Promotion.Status,
                    PromotionDescription = p.Promotion.Description,
                    PromotionDiscountDetail = p.Promotion.DiscountDetail
                }).ToList();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = listPromotionDTO;
                result.Message = "Successfully get applied promotion";
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> CancelAppointment(string token, Guid appointmentId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var appointment = await _repository.GetAppointmentById(appointmentId);
                if(appointment.Status != "Pending")
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Appointment status is not allowed to cancel";
                    return result;
                }
                await _repository.CancelAppointment(appointmentId);
                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully cancel appointment";
                _messageBus.PublishAppointmentActivity
                (
                    "APP_APPOINTMENT_CANCELED",
                    appointment.Id
                );
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> UpdateAppointment(string token, UpdateAppointmentDTO updateDTO)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                // Parse date và time
                if (!DateTime.TryParseExact(updateDTO.Date, "yyyy-MM-dd", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid date format. Use yyyy-MM-dd";
                    return result;
                }
               
                if (!TimeFormatHelper.TryParseTime(updateDTO.Time, out DateTime timeValue))
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Invalid time format. Use h:mm AM/PM or h:mm SA/CH (e.g., 1:22 AM or 1:22 SA)";
                    return result;
                }

                // Combine date và time
                var combinedDateTime = dateValue.Date.Add(timeValue.TimeOfDay);

                // Lấy thông tin appointment
                var appointment = await _repository.GetAppointmentById(updateDTO.AppointmentId);
                if (appointment == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Appointment not found";
                    return result;
                }

                // Kiểm tra quyền sở hữu và trạng thái
                if (appointment.UserId != id)
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "You don't have permission to edit this appointment";
                    return result;
                }

                if (appointment.Status != "Pending")
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Can only edit appointments in Pending status";
                    return result;
                }

                // Cập nhật datetime và notes
                appointment.StartAt = combinedDateTime;
                if (!string.IsNullOrEmpty(updateDTO.Notes))
                    appointment.Note = updateDTO.Notes;

                await _repository.UpdateAppointment(appointment);

                // Lấy thông tin services để trả về
                var services = await _repository.GetAppointmentServices(appointment.Id);
                
                // Tạo response
                var bookingResult = new BookingResultDTO
                {
                    AppointmentId = appointment.Id,
                    DateBook = appointment.StartAt ?? DateTime.UtcNow,
                    Notes = appointment.Note,
                    TotalAmount = appointment.TotalAmount,
                    DiscountAmount = appointment.DiscountAmount,
                    FinalAmount = appointment.FinalAmount,
                    Services = services.Select(s => new CartServiceDTO
                    {
                        ServiceId = s.ClinicService.Id,
                        ServiceName = s.ClinicService.Name,
                        EstimateTime = s.ClinicService.EstimateTime,
                        DiscountedPrice = s.ClinicService.DiscountedPrice
                    }).ToList()
                };

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = bookingResult;
                result.Message = "Successfully updated appointment";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> GetAppointmentDetail(string token, Guid appointmentId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                var appointment = await _repository.GetAppointmentDetailById(appointmentId);
                if (appointment.UserId != id)
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "You don't have permission to view this appointment";
                    return result;
                }

                var appointmentDetail = new AppointmentDetailDTO
                {
                    AppointmentId = appointment.Id,
                    UserName = appointment.User.FullName,
                    UserPhone = appointment.User.PhoneNumber,
                    UserEmail = appointment.User.Email,
                    UserAddress = appointment.User.Address,
                    DateBook = appointment.StartAt,
                    Notes = appointment.Note,
                    Status = appointment.Status,
                    TotalAmount = (int)Math.Round(appointment.TotalAmount ?? 0),
                    DiscountAmount = (int)Math.Round(appointment.DiscountAmount ?? 0),
                    FinalAmount = (int)Math.Round(appointment.FinalAmount ?? 0),
                    Services = appointment.AppointmentClinicServices.Select(acs => new AppointmentServiceDTO
                    {
                        ServiceId = acs.ClinicService.Id,
                        ServiceName = acs.ClinicService.Name,
                        EstimateTime = acs.ClinicService.EstimateTime,
                        Price = acs.ClinicService.DiscountedPrice,
                        PetId = appointment.PetId ?? Guid.Empty,
                        PetName = appointment.Pet?.Name
                    }).ToList()
                };

                result.IsSuccess = true;
                result.Code = 200;
                result.Data = appointmentDetail;
                result.Message = "Successfully get appointment detail";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> CheckReview(string token, Guid appointmentId)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }

            try
            {
                var checkReview = await _repository.CheckReview(appointmentId);
                if(checkReview)
                {
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = true;
                    result.Message = "Appointment has been reviewed";
                }
                else
                {
                    result.IsSuccess = true;
                    result.Code = 200;
                    result.Data = false;
                    result.Message = "Appointment has not been reviewed";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
        public async Task<ResultModel> CountService(string token)
        {
            var result = new ResultModel();
            var userId = Encoder.DecodeToken(token, "userid");
            if (!Guid.TryParse(userId, out Guid id))
            {
                result.IsSuccess = false;
                result.Code = 400;
                result.Message = "Invalid user ID";
                return result;
            }
            try
            {
                var cart = await _repository.GetCartByUserId(id);
                if (cart == null)
                {
                    result.IsSuccess = false;
                    result.Code = 200;
                    result.Message = "Cart is empty!";
                    return result;
                }
                var count = await _repository.CountService(cart.Id);
                if (count == 0)
                {
                    result.IsSuccess = false;
                    result.Code = 200;
                    result.Message = "Cart is empty!";
                    return result;
                }
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = count;
                result.Message = "Successfully count service";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Code = 500;
                result.ResponseFailed = ex.InnerException != null
                    ? ex.InnerException.Message + "\n" + ex.StackTrace
                    : ex.Message + "\n" + ex.StackTrace;
            }
            return result;
        }
    }
}
