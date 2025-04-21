using AccountManagementAPI.DTOs.ResultModel;
using AccountManagementAPI.DTOs.UserDTOs;
using AccountManagementAPI.Repositories;
using AccountManagementAPI.Utilities;
using AutoMapper;
using DataAccess.Models;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Utilities;
using System.Security.Principal;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace AccountManagementAPI.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly IMessageBus _messageBus;
        public AccountService(IAccountRepository repository, IMessageBus messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
        }
        public async Task<ResultModel> GetAllAccount(string token, int page)
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
            var user = await _repository.Get(id);
            if (user.Role != "ADMIN")
            {
                result.IsSuccess = false;
                result.Code = 401;
                result.Message = "Permission Denied";
                return result;
            }
            try
            {
                var accounts = await _repository.GetAllAccount();
                if (page == 0)
                {
                    page = 1;
                }
                var accountList = accounts.Select(a => new UserListModel
                {
                    Id = a.Id,
                    Role = a.Role,
                    Name = a.FullName,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    AvatarUrl = a.AvatarUrl,
                    Status = a.Status,
                }).ToList();
                var paginatedResult = await Pagination.GetPagination(accountList, page, 1000);
                result.IsSuccess = true;
                result.Code = 200;
                result.Data = accounts;
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

        public async Task<ResultModel> UpdateAccountStatus(string token, UserUpdateStatusModel userUpdateStatusModel)
        {
            ResultModel Result = new();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }

            try
            {
                var account = await _repository.Get(userUpdateStatusModel.Id);

                if (userUpdateStatusModel.Status == "INACTIVE")
                {
                    if (!string.IsNullOrWhiteSpace(account.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "Notification.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", account.FullName);
                        Html = Html.Replace("{{Email}}", account.Email);
                        Html = Html.Replace("{{Reason}}", userUpdateStatusModel.ReasonToBlock);
                        bool EmailSent = await Email.SendEmail(account.Email, "Ban Account", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                }

                if (userUpdateStatusModel.Status == "ACTIVE")
                {
                    if (!string.IsNullOrWhiteSpace(account.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "UnbanNotification.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", account.FullName);
                        Html = Html.Replace("{{Email}}", account.Email);
                        bool EmailSent = await Email.SendEmail(account.Email, "Unban Account", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                }
                account.Status = userUpdateStatusModel.Status;
                await _repository.Update(account);
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Message = "Account updated successfully";
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 500; // Internal Server Error
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return Result;
        }

        public async Task<ResultModel> AddNewAccount(string token, UserAddModel userAddModel)
        {
            ResultModel Result = new();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }

            try
            {
                var User = await _repository.GetUserByEmail(userAddModel.Email);
                var UserPhoneNumber = await _repository.GetUserByPhoneNumber(userAddModel.PhoneNumber);
                if (User != null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Email is already registered!";
                }
                else if (UserPhoneNumber != null)
                {
                    Result.IsSuccess = false;
                    Result.Code = 400;
                    Result.Message = "Phone number is already registered!";
                }
                else
                {
                    var config = new MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<UserAddModel, User>().ForMember(dest => dest.Password, opt => opt.Ignore());
                    });
                    IMapper mapper = config.CreateMapper();
                    User NewAccount = mapper.Map<UserAddModel, User>(userAddModel);
                    NewAccount.Id = Guid.NewGuid();
                    NewAccount.FullName = userAddModel.FullName;
                    NewAccount.PhoneNumber = userAddModel.PhoneNumber;
                    NewAccount.Dob = userAddModel.Dob;
                    NewAccount.Status = userAddModel.Status;
                    NewAccount.Email = userAddModel.Email;
                    NewAccount.Address = userAddModel.Address;
                    NewAccount.Role = userAddModel.Role;
                    NewAccount.AvatarUrl = userAddModel.AvatarURL;
                    var rawPassword = string.IsNullOrEmpty(userAddModel.Password) ? "123" : userAddModel.Password;
                    var hashedPasswordModel = Encoder.CreateHashPassword(rawPassword);
                    NewAccount.Password = hashedPasswordModel.HashedPassword;
                    NewAccount.Salt = hashedPasswordModel.Salt;
                    NewAccount.CreatedAt = DateTime.Now.AddHours(7);
                    NewAccount.TypeGroup = "First-Time Visitors";
                    
                    _ = await _repository.Insert(NewAccount);
                    Result.IsSuccess = true;
                    Result.Code = 200;
                    Result.Data = NewAccount;
                    Result.Message = "Add new account successfully!";
                    _messageBus.PublicUserActivity
                    (
                        "USER_CREATED",
                        NewAccount.Id
                    );
                }
            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 500; // Internal Server Error
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return Result;
        }
        public async Task<ResultModel> GetAccountDetail(string token, Guid AccountID)
        {
            ResultModel Result = new();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }

            try
            {
                var account = await _repository.Get(AccountID);

                var accountDetail = new UserDetailModel
                {
                    Id = account.Id,
                    FullName = account.FullName,
                    PhoneNumber = account.PhoneNumber,
                    Dob = account.Dob,
                    Status = account.Status,
                    Email = account.Email,
                    Address = account.Address,
                    Role = account.Role,
                    Password = account.Password.ToString(),
                    AvatarURL = account.AvatarUrl,
                };
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = account;
                Result.Message = "Get account detail successfully!";

            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 500; // Internal Server Error
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return Result;
        }
        public async Task<ResultModel> UpdateAccount(string token, UserUpdateModel userUpdateModel)
        {
            ResultModel Result = new();
            var userId = Encoder.DecodeToken(token, "userid");

            if (!Guid.TryParse(userId, out Guid id))
            {
                Result.IsSuccess = false;
                Result.Code = 400; // Bad request
                Result.Message = "Invalid user ID";
                return Result;
            }

            try
            {
                var account = await _repository.Get(userUpdateModel.Id);
                if (account.Status == "ACTIVE" && userUpdateModel.Status == "INACTIVE")
                {
                    if (!string.IsNullOrWhiteSpace(account.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "Notification.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", account.FullName);
                        Html = Html.Replace("{{Email}}", account.Email);
                        bool EmailSent = await Email.SendEmail(account.Email, "Ban Account", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                }
                else if (account.Status == "INACTIVE" && userUpdateModel.Status == "ACTIVE")
                {
                    if (!string.IsNullOrWhiteSpace(account.Email))
                    {
                        string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateEmail", "UnbanNotification.html");
                        string Html = File.ReadAllText(FilePath);
                        Html = Html.Replace("{{CustomerName}}", account.FullName);
                        Html = Html.Replace("{{Email}}", account.Email);
                        bool EmailSent = await Email.SendEmail(account.Email, "Unban Account", Html);
                    }
                    else
                    {
                        Console.WriteLine("Email does not exist. Skipping email notification.");
                    }
                }
                else if ((account.Status == "ACTIVE" && userUpdateModel.Status == "ACTIVE") || (account.Status == "INACTIVE" && userUpdateModel.Status == "INACTIVE"))
                {
                    // Trạng thái không thay đổi, không cần gửi email
                    Console.WriteLine("Status unchanged. No email will be sent.");
                }
                account.FullName = userUpdateModel.FullName;
                //  account.Email = userUpdateModel.Email;
                //   account.PhoneNumber = userUpdateModel.PhoneNumber;
                account.Address = userUpdateModel.Address;
                account.Dob = userUpdateModel.Dob;
                account.Role = userUpdateModel.Role;
                account.Status = userUpdateModel.Status;
                account.AvatarUrl = userUpdateModel.AvatarURL;

                _ = await _repository.Update(account);
                Result.IsSuccess = true;
                Result.Code = 200;
                Result.Data = account;
                Result.Message = "Update account successfully!";

            }
            catch (Exception e)
            {
                Result.IsSuccess = false;
                Result.Code = 500; // Internal Server Error
                Result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return Result;
        }


    }
}
