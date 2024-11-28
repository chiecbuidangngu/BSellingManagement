using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BookSellingManagement.Repository;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class CustomersController : Controller
    {
        private readonly DataContext _dataContext;
        public CustomersController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            var customers = (from order in _dataContext.Orders
                             where order.Status != 3 // Loại trừ các đơn hàng có trạng thái là 3 (đã hủy)
                             join user in _dataContext.Users on order.Username equals user.Email // Join với bảng Users
                             join orderDetail in _dataContext.OrderDetails on order.OrderCode equals orderDetail.OrderCode into orderDetails
                             from detail in orderDetails.DefaultIfEmpty()
                             group new { order, detail, user } by order.Username into grouped
                             select new
                             {
                                 Username = grouped.Key,
                                 UserName = grouped.FirstOrDefault().user.UserName, // Lấy tên người dùng đầu tiên trong nhóm
                                 PhoneNumber = grouped.FirstOrDefault().user.PhoneNumber, // Lấy số điện thoại người dùng đầu tiên trong nhóm
                                 Revenue = grouped.Select(g => g.order.TotalAmount).Distinct().Sum(), // Chỉ tính TotalAmount một lần cho mỗi đơn hàng
                                 TotalOrders = grouped.Select(g => g.order.OrderCode).Distinct().Count(), // Đếm số đơn hàng duy nhất
                                 TotalBooks = grouped.Sum(g => g.detail != null ? g.detail.Quantity : 0) // Tổng số sách trong các chi tiết đơn hàng
                             }).ToList();

            return View(customers);
        }



        public IActionResult Orders(string username)
        {
            var orders = (from order in _dataContext.Orders
                          where order.Username == username && order.Status != 3 // Lọc các đơn hàng không bị hủy
                          select new
                          {
                              OrderCode = order.OrderCode,
                              CreateDate = order.CreateDate,
                              Status = order.Status,
                              TotalAmount = order.TotalAmount
                          }).ToList();

            return View(orders); // Trả về View hoặc PartialView
        }


        public IActionResult Books(string username)
        {
            var books = (from order in _dataContext.Orders
                         where order.Username == username && order.Status != 3 // Lọc đơn hàng không bị hủy
                         join orderDetail in _dataContext.OrderDetails on order.OrderCode equals orderDetail.OrderCode into orderDetails
                         from detail in orderDetails.DefaultIfEmpty()
                         join book in _dataContext.Books on detail.BookId equals book.BookId // Join với bảng Books để lấy BookName
                         where detail != null
                         select new
                         {
                             BookCode = book.BookCode,          
                             BookName = book.BookName,      
                             Quantity = detail.Quantity,    
                             Image = book.Image             
                         }).ToList();

            return View(books);
        }









    }
}
