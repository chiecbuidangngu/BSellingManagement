
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers 
{
    [Area("Admin")]
    [Authorize(Roles= "Quản trị viên, Nhân viên")]
    public class HomeController : Controller 
    {
        private readonly DataContext _dataContext;

        public HomeController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
           
            return View();
        }
    }
}
