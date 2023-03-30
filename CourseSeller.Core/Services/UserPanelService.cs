using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseSeller.Core.DTOs.UserPanel;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Contexts;
using CourseSeller.DataLayer.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace CourseSeller.Core.Services
{
    public class UserPanelService : IUserPanelService
    {
        private MSSQLSContext _context;
        private IAccountService _accountService;

        public UserPanelService(MSSQLSContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }


        public async Task<UserInfoViewModel> GetUserInfo(string userName)
        {
            User user = await _accountService.GetUserByUserName(userName);
            UserInfoViewModel userInfo = new UserInfoViewModel()
            {
                Username = user.UserName,
                Email = user.Email,
                RegisterDate = user.RegisterDateTime,
                Wallet = 0,
            };

            return userInfo;
        }

        public async Task<SideBarViewModel> GetSideBarData(string userName)
        {
            var query = _context.Users.Where(u => u.UserName == userName)
                .Select(u => new SideBarViewModel()
                {
                    UserName = u.UserName,
                    ImageName = u.UserAvatar,
                    RegisterDateTime = u.RegisterDateTime,
                });

            return await query.SingleOrDefaultAsync();
        }
    }
}
