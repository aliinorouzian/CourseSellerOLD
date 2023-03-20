using CourseSeller.Core.Convertors;
using CourseSeller.Core.DTOs.Accounts;
using CourseSeller.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourseSeller.Web.Controllers
{
    public class AccountController : Controller
    {
        private IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        public async Task<IActionResult> Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid) { return View(viewModel); }
            if (await _accountService.IsExistUserName(viewModel.UserName))
            {
                ModelState.AddModelError("UserName", "نام کاربری تکراری می باشد.");
                return View(viewModel);
            }
            if (await _accountService.IsExistEmail(FixText.FixEmail(viewModel.Email)))
            {
                ModelState.AddModelError("Email", "ایمیل تکراری می باشد.");
                return View(viewModel);
            }

            // todo: register user

            return View();
        }
    }
}
