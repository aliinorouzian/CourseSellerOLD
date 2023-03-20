using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace CourseSeller.Core.Services
{
    public class AccountService : IAccountService
    {
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
    }
}
