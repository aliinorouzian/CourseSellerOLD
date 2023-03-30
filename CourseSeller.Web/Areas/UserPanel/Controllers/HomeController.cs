using CourseSeller.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseSeller.Web.Areas.UserPanel.Controllers
{
    [Area("UserPanel")]
    [Authorize]
    public class HomeController : Controller
    {
        private IUserPanelService _userPanelService;

        public HomeController(IUserPanelService userPanelService)
        {
            _userPanelService = userPanelService;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _userPanelService.GetUserInfo(User.Identity.Name));
        }
    }
}
