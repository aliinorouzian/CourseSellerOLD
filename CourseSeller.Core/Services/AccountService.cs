using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CourseSeller.Core.Convertors;
using CourseSeller.Core.DTOs.Account;
using CourseSeller.Core.Generators;
using CourseSeller.Core.Security;
using CourseSeller.Core.Senders;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using CourseSeller.DataLayer.Entities.Users;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CourseSeller.Core.Services
{
    public class AccountService : IAccountService
    {
        public const byte SuccessActivated = 0;
        public const byte NotFoundAccount = 1;
        public const byte AllreadyActivated = 2;
        public const byte TokenExipered = 3;


        private MSSQLSContext _context;
        private IConfiguration _conf;
        private IBackgroundJobClient _backgroundJobClient;
        private ISendEmail _sendEmail;
        private IViewRenderService _viewRender;

        public AccountService(MSSQLSContext context, IConfiguration conf, IBackgroundJobClient backgroundJobClient, ISendEmail sendEmail, IViewRenderService viewRender)
        {
            _context = context;
            _conf = conf;
            _backgroundJobClient = backgroundJobClient;
            _sendEmail = sendEmail;
            _viewRender = viewRender;
        }


        public async Task<bool> IsExistUserName(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<bool> IsExistEmail(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> AddUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> GetUserByActiveCode(string activeCode)
        {
            User user = await _context.Users.SingleOrDefaultAsync(u => u.ActiveCode == activeCode);
            if (user == null)
                return null;
            // expire token after 10 minute. 
            int expireTimePerMin = Convert.ToInt32(_conf.GetSection("Emails").GetSection("ExpireTimePerMin").Value);
            if (user.ActiveCodeGenerateDateTime.AddMinutes(expireTimePerMin) < DateTime.Now)
            {
                // send new link
                await RevokeActiveCodeAndNewSendEmail(user, "Emails/_ForgotPassword", "بازیابی حساب کاربری");

                return null;
            }

            return user;
        }

        public async Task<bool> UpdateUser(User user)
        {
            _context.Users.Update(user);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<byte> ActiveAccount(string activeCode)
        {
            User user = await _context.Users.SingleOrDefaultAsync(u => u.ActiveCode == activeCode);
            if (user == null) return NotFoundAccount;
            if (user.IsActive) return AllreadyActivated;
            // expire token after 10 minute. 
            int expireTimePerMin = Convert.ToInt32(_conf.GetSection("Emails").GetSection("ExpireTimePerMin").Value);
            if (user.ActiveCodeGenerateDateTime.AddMinutes(expireTimePerMin) < DateTime.Now)
            {
                // send new link
                await RevokeActiveCodeAndNewSendEmail(user);

                return TokenExipered;
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return SuccessActivated;
        }

        public async Task<bool> RevokeActiveCodeAndNewSendEmail(User user, string emailBody = "Emails/_ActivateEmail", string emailSubject = "فعالسازی")
        {
            user.ActiveCode = CodeGenerators.GenerateUniqueCode();
            user.ActiveCodeGenerateDateTime = DateTime.Now;
            await UpdateUser(user);

            string body = _viewRender.RenderToStringAsync(emailBody, user);
            _backgroundJobClient.Enqueue(() =>
                _sendEmail.Send(user.Email, emailSubject, body));

            return true;
        }

        public async Task<bool> RevokeActiveCode(User user)
        {
            user.ActiveCode = CodeGenerators.GenerateUniqueCode();
            user.ActiveCodeGenerateDateTime = DateTime.Now;
            await UpdateUser(user);

            return true;
        }

        public async Task<bool> ResetPassword(User user, string newPassword)
        {
            string hashedNewPassword = PasswordHelper.HashPassword(newPassword);
            user.Password = hashedNewPassword;
            // expire old link to
            user.ActiveCode = CodeGenerators.GenerateUniqueCode();
            user.ActiveCodeGenerateDateTime = DateTime.Now;

            try
            {
                await UpdateUser(user);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}