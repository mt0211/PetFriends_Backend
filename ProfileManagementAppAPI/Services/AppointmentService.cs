using DataAccess.Models;
using ProfileManagementAppAPI.DTOs.ResultModel;
using ProfileManagementAppAPI.DTOs;

using ProfileManagementAppAPI.Repositories;
using ProfileManagementAppAPI.Utilities;
using MySqlX.XDevAPI.Common;
using AppAppointmentManagementAPI.DTOs.ReviewModel;
using ProfileManagementAppAPI.DTOs.CategoryClinicServiceDTO;

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

                var category = await _repository.GetCategory();
                if (category == null || !category.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found category";
                    return result;
                }
                var categoriesDto = category.Select(c => new CategoryListReqModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CategoryStatus = c.Status,
                    ClinicServices = c.ClinicServices.Select(s => new ServiceListReqModel
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
                result.Message = "Successfully get all category";


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
    }
}
