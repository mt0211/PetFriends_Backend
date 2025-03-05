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
                var existingReview = await _repository.GetReviewById(reviewUpdateModel.Id);

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
                
                // Tính tổng giá trị dịch vụ
                decimal totalAmount = cart.UserCartItems.Sum(item => item.ClinicService.DiscountedPrice ?? 0);
                decimal discountAmount = 0;
                
                // Danh sách các promotion đã áp dụng
                var appliedPromotions = new List<(Promotion Promotion, decimal DiscountAmount)>();
                
                // Kiểm tra và áp dụng các promotion
                if (updateCartDTO.PromotionIds != null && updateCartDTO.PromotionIds.Any())
                {
                    // Giới hạn số lượng promotion tối đa là 2
                    var promotionIds = updateCartDTO.PromotionIds.Take(2).ToList();
                    var user = await _repository.GetUserByUserId(id);
                    
                    // Lấy thông tin các promotion được chọn
                    foreach (var promotionId in promotionIds)
                    {
                        var promotion = await _repository.GetPromotionById(promotionId);
                        if (promotion != null && promotion.Status == "Active")
                        {
                            // Kiểm tra điều kiện áp dụng promotion
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
                                
                                // Tính giảm giá dựa trên loại promotion
                                if (promotion.Type == 0)
                                {
                                    currentDiscountAmount = totalAmount * ((decimal)(promotion.DiscountDetail ?? 0) / 100);                                   
                                }
                                else if (promotion.Type == 1)
                                {
                                    currentDiscountAmount = promotion.DiscountDetail??0;
                                }
                                
                                // Thêm vào danh sách promotion đã áp dụng
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
                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    PetId = cart.UserCartItems.FirstOrDefault()?.PetId, // Lấy PetId từ cart item đầu tiên
                    CreatedAt = DateTime.UtcNow,
                    StartAt = updateCartDTO.DateBook,
                    Status = "Pending", 
                    Note = updateCartDTO.Notes,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    FinalAmount = totalAmount - discountAmount
                };
                 await _repository.AddAppointment(appointment);

                //Add appointment clinic service
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
                    // Lưu thông tin các promotion đã áp dụng
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
                var updateCart = new UserCart
                {
                    Id = updateCartDTO.CartId,
                    Datebook = updateCartDTO.DateBook,
                    Notes = updateCartDTO.Notes,
                    Status = 1
                };
                await _repository.UpdateCart(updateCart);
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
    }
}
