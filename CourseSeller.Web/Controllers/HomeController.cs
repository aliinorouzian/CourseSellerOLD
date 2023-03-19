using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CourseSeller.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("Hi!");
        }
    }
}