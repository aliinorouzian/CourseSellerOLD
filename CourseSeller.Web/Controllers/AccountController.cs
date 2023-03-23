using System.Security.Claims;
using CourseSeller.Core.Convertors;
using CourseSeller.Core.DTOs.Accounts;
using CourseSeller.Core.Generators;
using CourseSeller.Core.Security;
using CourseSeller.Core.Senders;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Entities.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;


namespace CourseSeller.Web.Controllers
{
    public class AccountController : Controller
    {
        private IAccountService _accountService;
        private IViewRenderService _viewRender;
        private IConfiguration _configuration;

        public AccountController(IAccountService accountService, IViewRenderService viewRender, IConfiguration configuration)
        {
            _accountService = accountService;
            _viewRender = viewRender;
            _configuration = configuration;
        }


        #region Register

        public async Task<IActionResult> Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            bool errorFlag = false;
            if (!ModelState.IsValid) { errorFlag = true; }
            if (!viewModel.AcceptRules)
            {
                ModelState.AddModelError("AcceptRules", "لطفا قوانین را بپذیرید.");
                errorFlag = true;
            }
            if (await _accountService.IsExistUserName(viewModel.UserName))
            {
                ModelState.AddModelError("UserName", "نام کاربری تکراری می باشد.");
                errorFlag = true;
            }
            if (await _accountService.IsExistEmail(FixText.FixEmail(viewModel.Email)))
            {
                ModelState.AddModelError("Email", "ایمیل تکراری می باشد.");
                errorFlag = true;
            }
            if (errorFlag) { return View(viewModel); }

            // register 
            User user = new User()
            {
                ActiveCode = CodeGenerators.GenerateUniqueCode(),
                ActiveCodeGenerateDateTime = DateTime.Now,
                Email = FixText.FixEmail(viewModel.Email),
                UserName = viewModel.UserName.ToLower(),
                IsActive = false,
                Password = PasswordHelper.HashPassword(viewModel.Password),
                RegisterDateTime = DateTime.Now,
                UserAvatar = "Default.png",
            };
            user = await _accountService.AddUser(user);


            #region Send Activate Email

            string body = _viewRender.RenderToStringAsync("Emails/_ActivateEmail", user);
            // todo: queue it
            SendEmail.Send(user.Email, "فعالسازی", body, conf: _configuration);

            #endregion


            return View("SuccessRegister", user);
        }

        #endregion


        #region Login

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            bool errorFlag = false;
            if (!ModelState.IsValid) { errorFlag = true; }

            var user = await _accountService.GetUserByEmail(viewModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "ایمیل مورد نظر یافت نشد.");
                errorFlag = true;
            }
            if (errorFlag) { return View(viewModel); }

            if (PasswordHelper.VerifyPassword(viewModel.Password, user.Password))
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError("Email", "حساب کاربری شما فعال نمی باشد. ایمیل حاوی لینک فعالسازی برای شما ارسال شد.");
                    // todo: send new email

                    return View(viewModel);
                }


                #region Login rules

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties()
                {
                    IsPersistent = viewModel.RememberMe,
                };
                await HttpContext.SignInAsync(principal, properties);

                #endregion


                ViewData["IsSuccess"] = true;


                return View();
            }


            ModelState.AddModelError("Password", "رمز عبور نامعتبر است.");
            return View(viewModel);
        }

        #endregion


        #region Active Account

        public async Task<IActionResult> Activate(string id)
        {
            ViewData["IsActive"] = await _accountService.ActiveAccount(id);

            return View();
        }

        #endregion


        #region Logout

        [HttpPost]
        public async Task<IActionResult> Logout(string id)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return Redirect("/");
        }

        #endregion
    }
}
