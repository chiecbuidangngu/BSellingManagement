using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSellingManagement.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class ChangePassword : Controller
    {
     
        public IActionResult Index()
        {

            return View();
        }
    }
}