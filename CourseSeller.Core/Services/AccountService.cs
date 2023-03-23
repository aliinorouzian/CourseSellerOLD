using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CourseSeller.Core.Convertors;
using CourseSeller.Core.DTOs.Accounts;
using CourseSeller.Core.Generators;
using CourseSeller.Core.Security;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using CourseSeller.DataLayer.Entities.Users;
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

        public AccountService(MSSQLSContext context, IConfiguration conf)
        {
            _context = context;
            _conf = conf;
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

        public async Task<byte> ActiveAccount(string activeCode)
        {
            User user = await _context.Users.SingleOrDefaultAsync(u => u.ActiveCode == activeCode);
            if (user == null) return NotFoundAccount;
            if (user.IsActive) return AllreadyActivated;
            // expire token after 10 minute. 
            int expireTimePerMin = Convert.ToInt32(_conf.GetSection("Emails").GetSection("ExpireTimePerMin").Value);
            if (user.ActiveCodeGenerateDateTime.AddMinutes(expireTimePerMin) < DateTime.Now)
            {
                // todo: send new link
                user.ActiveCode = CodeGenerators.GenerateUniqueCode();
                user.ActiveCodeGenerateDateTime = DateTime.Now;
                await _context.SaveChangesAsync();
                return TokenExipered;
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return SuccessActivated;
        }
    }
}