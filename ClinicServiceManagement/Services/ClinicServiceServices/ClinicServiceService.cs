using ClinicServiceManagement.DTOs.ResultModel;
using ClinicServiceManagement.DTOs.ServiceDTOs;
using ClinicServiceManagement.Utilites;
using ClinicServiceManagementAPI.Repository.ClinicServiceRepository;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.CompilerServices;

namespace ClinicServiceManagementAPI.Services.ClinicServiceServices
{
    public class ClinicServiceService : IClinicServiceService
    {
        private readonly IClinicServiceRepository _clinicServiceRepository;
       // private readonly PetfriendsContext _context;
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
                var ServiceList = Services.Select(c => new ServiceListDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    CategoryName = c.CategoryName,
                    CreateAt = c.CreateAt,
                    EstimateTime = c.EstimateTime,
                    Price = c.Price,
                    Status = c.Status,
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
                    Status = "ACTIVE",
                    EstimateTime = serviceAddDTO.EstimateTime,
                    DiscountAmount = serviceAddDTO.DiscountAmount,
                    DiscountFrom = serviceAddDTO.DiscountFrom,
                    DiscountTo = serviceAddDTO.DiscountTo,
                    Image = serviceAddDTO.Image,
                };
                await _clinicServiceRepository.AddService(newService);
                //  await _clinicServiceRepository.UpdateDiscountedPrice(newService);


                result.IsSuccess = true;
                result.Code = 200;
                result.Data = newService;
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
                    result.Message = "Not found category";
                    return result;
                }
                //Success response
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = vaccines;
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
                result.Message = "Successfully get service detail";
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

        public async Task<ResultModel> UpdateServiceStatus(string token, Guid serviceId)
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
                // Lấy thông tin dịch vụ
                var service = await _clinicServiceRepository.GetServiceStatusByID(serviceId);
                if (service == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Service not found";
                    return result;
                }

                // Cập nhật trạng thái
                if (service.Status == "ACTIVE")
                {
                    service.Status = "INACTIVE";
                }
                else if (service.Status == "INACTIVE")
                {
                    service.Status = "ACTIVE";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Code = 400; // Bad request
                    result.Message = "Invalid service status";
                    return result;
                }

                // Cập nhật trạng thái vào database
                await _clinicServiceRepository.UpdateStatus(service);

                // Chuẩn bị dữ liệu trả về
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new
                {
                    service.Id,
                    service.Status
                };
                result.Message = "Successfully updated service status";
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
        public async Task<ResultModel> UpdateService(string token, ServiceUpdateDTO serviceUpdateDTO)
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
                // Lấy thông tin service từ database
                var service = await _clinicServiceRepository.GetServiceStatusByID(serviceUpdateDTO.Id);
                if (service == null)
                {
                    result.IsSuccess = false;
                    result.Code = 404; // Not found
                    result.Message = "Service not found";
                    return result;
                }

                // Cập nhật thông tin service từ DTO
                service.Name = serviceUpdateDTO.Name ?? service.Name;
                service.Description = serviceUpdateDTO.Description ?? service.Description;
                service.Price = serviceUpdateDTO.Price ?? service.Price;
                service.Status = serviceUpdateDTO.Status ?? service.Status;
                service.Category = serviceUpdateDTO.Category ?? service.Category;
                service.EstimateTime = serviceUpdateDTO.EstimateTime ?? service.EstimateTime;
                service.DiscountAmount = serviceUpdateDTO.DiscountAmount ?? service.DiscountAmount;
                service.DiscountFrom = serviceUpdateDTO.DiscountFrom;
                service.DiscountTo = serviceUpdateDTO.DiscountTo;
                service.Image = serviceUpdateDTO.Image ?? service.Image;

                // Gọi repository để cập nhật service
                await _clinicServiceRepository.UpdateService(service);

                // Trả về kết quả
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = new
                {
                    service.Id,
                    service.Name,
                    service.Description,
                    service.Price,
                    service.Status,
                    service.Category,
                    service.EstimateTime,
                    service.DiscountAmount,
                    service.DiscountFrom,
                    service.DiscountTo,
                    service.Image
                };
                result.Message = "Successfully updated service";
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
