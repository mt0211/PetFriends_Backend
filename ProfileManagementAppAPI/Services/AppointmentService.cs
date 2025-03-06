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

namespace ProfileManagementAppAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
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

                var reviewEntity = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Content = reviewModel.Content,
                    Rating = reviewModel.Rating,
                    UserId = id,
                    AppointmentId = reviewModel.AppointmentId,
                    CreatedAt = DateTime.UtcNow
                };
                if (reviewModel.Rating > 5 || reviewModel.Rating < 0)
                {
                    result.IsSuccess = false;
                    result.Code = 403;
                    result.Message = "Rating less than 5 and more than 0";
                    return result;
                }
                await _repository.AddReview(reviewEntity);

                // success 
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = reviewModel;
                result.Message = "Successfully added new review";
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

                existingReview.Content = reviewUpdateModel.Content;
                existingReview.Rating = reviewUpdateModel.Rating;
                //CreatedAt = DateTime.UtcNow

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
                var checkUserCartItem = await _repository.CheckUserCartItemByServiceId(addToCartDTO.ClinicServiceId);
                
                if(checkUserCartItem != null)
                {
                    result.IsSuccess = false;
                    result.Code = 400;
                    result.Message = "Service already in cart";
                    return result;
                }
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
                      ClinicServiceId = addToCartDTO.ClinicServiceId,
                      PetId = addToCartDTO.PetId
                    };
                    await _repository.AddNewCartItem(newCartItem);
                }
                else
                {
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
                        ClinicServiceId = addToCartDTO.ClinicServiceId,
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
                    result.Code = 404;
                    result.Message = "Cart not found";
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
                    }).ToList()
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

                // Tạo appointment mới
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    PetId = cart.UserCartItems.FirstOrDefault()?.PetId,
                    CreatedAt = DateTime.UtcNow,
                    StartAt = updateCartDTO.DateBook,
                    Status = "Pending",
                    Note = updateCartDTO.Notes,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = totalAmount - discountAmount
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
                        DateGiven = updateCartDTO.DateBook,
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
                    Datebook = updateCartDTO.DateBook,
                    Notes = updateCartDTO.Notes,
                    Status = 1 // Đã đặt lịch
                };
                await _repository.UpdateCart(updateCart);

                // Tạo kết quả trả về
                var bookingResult = new BookingResultDTO
                {
                    AppointmentId = appointment.Id,
                    DateBook = updateCartDTO.DateBook,
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
                    await _repository.RemoveCart(cart);
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
                    UserName = a.User.FullName,
                    UserPhone = a.User.PhoneNumber,
                    UserEmail = a.User.Email,
                    PetName = a.Pet.Name,
                    CreatedAt = a.CreatedAt ?? DateTime.UtcNow,
                    StartAt = a.StartAt ?? DateTime.UtcNow,
                    Status = a.Status,
                    Notes = a.Note,
                    TotalAmount = a.TotalAmount,
                    DiscountAmount = a.DiscountAmount,
                    FinalAmount = a.FinalAmount,
                    Services = a.AppointmentClinicServices.Select(s => new BookingServiceDTO
                    {
                        ServiceName = s.ClinicService.Name,
                        EstimateTime = s.ClinicService.EstimateTime,
                        Price = s.ClinicService.DiscountedPrice ?? 0,
                        PetName = a.Pet.Name
                    }).ToList()
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

                // Cập nhật thông tin cơ bản nếu có thay đổi
                if (updateDTO.DateBook.HasValue)
                    appointment.StartAt = updateDTO.DateBook;
                
                if (!string.IsNullOrEmpty(updateDTO.Notes))
                    appointment.Note = updateDTO.Notes;

                // Xử lý thay đổi services
                var currentServices = await _repository.GetAppointmentServices(appointment.Id);

                if (updateDTO.ServiceIds != null)
                {
                    // Xóa những service không còn trong danh sách mới
                    var servicesToRemove = currentServices
                        .Where(s => !updateDTO.ServiceIds.Contains(s.ClinicServiceId));
                    foreach (var service in servicesToRemove)
                    {
                        await _repository.RemoveAppointmentService(service);
                    }

                    // Thêm những service mới
                    foreach (var serviceId in updateDTO.ServiceIds)
                    {
                        if (!currentServices.Any(s => s.ClinicServiceId == serviceId))
                        {
                            var appointmentService = new AppointmentClinicService
                            {
                                Id = Guid.NewGuid(),
                                AppointmentId = appointment.Id,
                                ClinicServiceId = serviceId,
                                DateGiven = appointment.StartAt ?? DateTime.UtcNow,
                                Notes = appointment.Note
                            };
                            await _repository.AddAppointmentClinicService(appointmentService);
                        }
                    }
                }

                // Tính toán lại tổng tiền sau khi thay đổi services
                var updatedServices = await _repository.GetAppointmentServices(appointment.Id);
                decimal totalAmount = updatedServices.Sum(s => s.ClinicService.DiscountedPrice ?? 0);

                // Xử lý thay đổi promotions
                if (updateDTO.PromotionIds != null)
                {
                    var currentPromotions = await _repository.GetAppointmentPromotions(appointment.Id);
                    var newPromotionIds = updateDTO.PromotionIds.Take(2).ToList();
                    
                    // Xóa những promotion không còn trong danh sách mới
                    foreach (var promotion in currentPromotions)
                    {
                        if (!newPromotionIds.Contains(promotion.PromotionId ?? Guid.Empty))
                        {
                            await _repository.RemoveAppointmentPromotion(promotion);
                        }
                    }

                    // Thêm những promotion mới
                    decimal discountAmount = 0;
                    var user = await _repository.GetUserByUserId(id);

                    foreach (var promotionId in newPromotionIds)
                    {
                        if (!currentPromotions.Any(p => p.PromotionId == promotionId))
                        {
                            var promotion = await _repository.GetPromotionById(promotionId);
                            if (promotion != null && promotion.Status == "Active")
                            {
                                bool canApply = false;
                                if (promotion.TargetGroup == "All Customers") canApply = true;
                                else if (promotion.TargetGroup == "First-Time Visitors" && 
                                        user.TypeGroup == "First-Time Visitors") canApply = true;
                                else if (promotion.TargetGroup == "Loyalty Members" && 
                                        user.TypeGroup == "Loyalty Members") canApply = true;

                                if (canApply)
                                {
                                    decimal currentDiscountAmount = 0;
                                    if (promotion.Type == 0) // Phần trăm
                                        currentDiscountAmount = totalAmount * (promotion.DiscountDetail.GetValueOrDefault() / 100);
                                    else if (promotion.Type == 1) // Số tiền cố định
                                        currentDiscountAmount = promotion.DiscountDetail.GetValueOrDefault();

                                    var appointmentPromotion = new AppointmentPromotion
                                    {
                                        Id = Guid.NewGuid(),
                                        AppointmentId = appointment.Id,
                                        PromotionId = promotion.Id,
                                        DiscountAmount = currentDiscountAmount,
                                        CreateAt = DateTime.UtcNow
                                    };
                                    await _repository.AddAppointmentPromotion(appointmentPromotion);
                                    discountAmount += currentDiscountAmount;
                                }
                            }
                        }
                    }

                    // Cập nhật tổng giảm giá
                    if (discountAmount > totalAmount)
                        discountAmount = totalAmount;

                    appointment.DiscountAmount = discountAmount;
                    appointment.FinalAmount = totalAmount - discountAmount;
                }

                appointment.TotalAmount = totalAmount;
                await _repository.UpdateAppointment(appointment);

                // Tạo response
                var updatedAppointment = await _repository.GetAppointmentById(appointment.Id);
                var bookingResult = new BookingResultDTO
                {
                    AppointmentId = updatedAppointment.Id,
                    DateBook = updatedAppointment.StartAt ?? DateTime.UtcNow,
                    Notes = updatedAppointment.Note,
                    TotalAmount = updatedAppointment.TotalAmount,
                    DiscountAmount = updatedAppointment.DiscountAmount,
                    FinalAmount = updatedAppointment.FinalAmount,
                    Services = updatedServices.Select(s => new CartServiceDTO
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
                    TotalAmount = appointment.TotalAmount,
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
    }
}
