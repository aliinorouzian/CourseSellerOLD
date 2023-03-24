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
        Task<byte> ActiveAccount(string activeCode);
        Task<bool> RevokeActiveCodeAndNewSendEmail(User user);

    }
}
