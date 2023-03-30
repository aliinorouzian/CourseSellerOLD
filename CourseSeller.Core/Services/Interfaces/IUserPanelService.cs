using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseSeller.Core.DTOs.UserPanel;
using CourseSeller.DataLayer.Entities.Users;

namespace CourseSeller.Core.Services.Interfaces
{
    public interface IUserPanelService
    {
        Task<UserInfoViewModel> GetUserInfo(string userName);
    }
}
