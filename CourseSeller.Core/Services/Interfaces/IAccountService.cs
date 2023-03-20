using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseSeller.Core.Services.Interfaces
{
    public interface IAccountService
    {
        Task<bool> IsExistUserName(string userName);
        Task<bool> IsExistEmail(string email);
    }
}
