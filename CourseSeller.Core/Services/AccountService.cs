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

namespace CourseSeller.Core.Services
{
    public class AccountService : IAccountService
    {
        public const byte SuccessActivated = 0;
        public const byte NotFoundAccount = 1;
        public const byte AllreadyActivated = 2;

        private MSSQLSContext _context;

        public AccountService(MSSQLSContext context)
        {
            _context = context;
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

            user.IsActive = true;
            user.ActiveCode = CodeGenerators.GenerateUniqueCode();
            await _context.SaveChangesAsync();

            return SuccessActivated;
        }
    }
}