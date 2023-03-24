using System.Security.Claims;
using CourseSeller.Core.Convertors;
using CourseSeller.Core.DTOs.Account;
using CourseSeller.Core.Generators;
using CourseSeller.Core.Security;
using CourseSeller.Core.Senders;
using CourseSeller.Core.Services.Interfaces;
using CourseSeller.DataLayer.Entities.Users;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CourseSeller.Web.Controllers
{
    public class AccountController : Controller
    {
        private IAccountService _accountService;
        private IViewRenderService _viewRender;
        private IConfiguration _configuration;
        private IBackgroundJobClient _backgroundJobClient;
        private ISendEmail _sendEmail;

        public AccountController(IAccountService accountService, IViewRenderService viewRender, IConfiguration configuration, IBackgroundJobClient backgroundJobClient, ISendEmail sendEmail)
        {
            _accountService = accountService;
            _viewRender = viewRender;
            _configuration = configuration;
            _backgroundJobClient = backgroundJobClient;
            _sendEmail = sendEmail;
        }


        #region Register

        [AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [AllowAnonymous]
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

            _backgroundJobClient.Enqueue(() =>
                _sendEmail.Send(user.Email, "فعالسازی", body));

            #endregion


            return View("SuccessRegister", user);
        }

        #endregion


        #region Login

        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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
                    await _accountService.RevokeActiveCodeAndNewSendEmail(user);

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

        [AllowAnonymous]
        public async Task<IActionResult> Activate(string id)
        {
            ViewData["IsActive"] = await _accountService.ActiveAccount(id);

            return View();
        }

        #endregion


        #region Logout

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout(string id)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return Redirect("/");
        }

        #endregion


        #region Forgot Password

        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
        {
            bool errorFlag = false;
            if (!ModelState.IsValid)
                errorFlag = true;

            string fixedEmail = FixText.FixEmail(viewModel.Email);
            User user = await _accountService.GetUserByEmail(fixedEmail);
            if (user == null)
            {
                ModelState.AddModelError("Email", "کاربری یافت نشد.");
                errorFlag = true;
            }

            if (errorFlag) { return View(viewModel); }

            await _accountService.RevokeActiveCodeAndNewSendEmail(user, "Emails/_ForgotPassword", "بازیابی حساب کاربری");
            ViewData["IsSuccess"] = true;

            return View();
        }


        #endregion


        #region Reset Password

        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string id)
        {
            return View(new ResetPasswordViewModel()
            {
                ActiveCode = id,
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            bool errorFlag = false;
            if (!ModelState.IsValid)
                errorFlag = true;

            User user = await _accountService.GetUserByActiveCode(viewModel.ActiveCode);
            if (user == null)
            {
                ModelState.AddModelError("Email", "لینک شما منقضی شده است. لینک جدید برای شما ارسال شد.");
                errorFlag = true;
            }

            if (errorFlag) { return View(viewModel); }

            // This will reset it and expire used token.
            await _accountService.ResetPassword(user, viewModel.Password);

            TempData["ResetPasswordIsSuccess"] = true;


            return RedirectToAction(nameof(Login));
        }

        #endregion
    }
}
