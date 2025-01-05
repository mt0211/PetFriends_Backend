using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
using ClinicServiceManagement.Repository;
using ClinicServiceManagement.Utilites;
using DataAccess.Models;
using System;
using System.Runtime.CompilerServices;

namespace ClinicServiceManagement.Services
{
    public class ClinicServiceService : IClinicServiceService
    {
        private readonly IClinicServiceRepository _clinicServiceRepository;
        public ClinicServiceService(IClinicServiceRepository clinicServiceRepository)
        {
            _clinicServiceRepository = clinicServiceRepository;
        }
        public async Task<ResultModel> GetAllService(string token)
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
                var Services = await _clinicServiceRepository.GetAllClinicService();
              var ServiceList = Services.Select(c=> new ServiceListDTO
              {
                  Id = c.Id,
                  Name = c.Name,
                  CategoryName = c.CategoryName,
                  EstimateTime = c.EstimateTime,
                  Price = c.Price,
                  DiscountedPrice = c.DiscountedPrice,
                  Image = c.Image,
              }).ToList();
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = Services;
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

        public async Task<ResultModel> AddNewService(string token, ServiceAddDTO serviceAddDTO)
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
            try
            {
                var newService = new ClinicService
                {
                    Id = Guid.NewGuid(),
                    Name = serviceAddDTO.Name,
                    Description = serviceAddDTO.Description,
                    CreateAt = DateTimeOffset.Now.DateTime,
                    Category = serviceAddDTO.Category,
                    Price = serviceAddDTO.Price,
                    Status = serviceAddDTO.Status,
                    EstimateTime = serviceAddDTO.EstimateTime,
                    DiscountAmount = serviceAddDTO.DiscountAmount,
                    DiscountFrom = serviceAddDTO.DiscountFrom,
                    DiscountTo = serviceAddDTO.DiscountTo,
                    Image = serviceAddDTO.Image,
                };

                await _clinicServiceRepository.Insert(newService);
                await _clinicServiceRepository.UpdateDiscountedPrice(newService);
               

                result.IsSuccess = true;
                result.Code = 200;
                result.Message = "Successfully added new service";
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
        public async Task<ResultModel> GetAllCategory(string token)
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
            try
            {
                var vaccines = await _clinicServiceRepository.GetAllCategory();
                if (vaccines == null || !vaccines.Any())
                {
                    result.IsSuccess = false;
                    result.Code = 404;
                    result.Message = "Not found vaccines";
                    return result;
                }
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccines;
                result.Message = "Successfully get all vaccine";
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

        public async Task<ResultModel> GetServiceDetail(string token, Guid ServiceID)
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
            try
            {
                var service = await _clinicServiceRepository.GetServiceByID(ServiceID);
                var ServiceDetail = new ServiceDetailDTO
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    CategoryName = service.CategoryName,
                    Price = service.Price,
                    Status = service.Status,
                    EstimateTime = service.EstimateTime,
                    DiscountAmount = service.DiscountAmount,
                    DiscountFrom = service.DiscountFrom,
                    DiscountTo = service.DiscountTo,
                    Image = service.Image,
                };

                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = ServiceDetail;
                result.Message = "Successfully get pet detail";
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

        //Background job to update discount price
        public async Task UpdateDiscountedPrices()
        {
            var services = await _clinicServiceRepository.GetAllClinicService();

            foreach (var service in services)
            {
                if (service.DiscountFrom.HasValue && service.DiscountTo.HasValue)
                {
                    if (DateTime.UtcNow >= service.DiscountFrom.Value && DateTime.UtcNow <= service.DiscountTo.Value)
                    {
                        service.DiscountedPrice = service.Price - (service.DiscountAmount ?? 0);
                    }
                    else
                    {
                        service.DiscountedPrice = service.Price;
                    }
                    await _clinicServiceRepository.Update(service);
                }
            }
        }
    }
}
