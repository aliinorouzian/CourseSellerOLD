using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseSeller.Core.DTOs.Account;
using CourseSeller.DataLayer.Entities.Users;

namespace CourseSeller.Core.Services.Interfaces
{
    public interface IAccountService
    {
        Task<bool> IsExistUserName(string userName);
        Task<bool> IsExistEmail(string email);
        Task<User> AddUser(User user);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUserName(string userName);
        Task<User> GetUserByActiveCode(string activeCode);
        Task<bool> UpdateUser(User user);
        Task<byte> ActiveAccount(string activeCode);
        Task<bool> RevokeActiveCodeAndNewSendEmail(User user, string emailBody = "Emails/_ActivateEmail", string emailSubject = "فعالسازی");
        Task<bool> RevokeActiveCode(User user);
        Task<bool> ResetPassword(User user, string newPassword);
    }
}
