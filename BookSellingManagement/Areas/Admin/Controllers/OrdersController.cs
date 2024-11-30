using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSellingManagement.Models.OrderModel;
using BookSellingManagement.Models;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên, Nhân viên")]
    public class OrdersController : Controller
    {
        private readonly DataContext _dataContext;

        public OrdersController(DataContext context)
        {
            _dataContext = context;
        }


        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10; // Số lượng đơn hàng mỗi trang

            if (pg < 1)
            {
                pg = 1;
            }

            List<OrderModel> orders = await _dataContext.Orders
                .OrderByDescending(c => c.CreateDate) // Sắp xếp theo ngày tạo mới nhất
                .ToListAsync();

            int recsCount = orders.Count(); // Tổng số đơn hàng

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = orders.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }



        public async Task<IActionResult> ConfirmShipping(string orderId)
        {
            var order = await _dataContext.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == 1 || order.Status == 2)
            {
                order.Status = 4; 
                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xác nhận đã gửi đơn hàng thành công";
            }

            return RedirectToAction("Index", "Orders");
        }
      

        public async Task<IActionResult> Delete(string orderId)
        {
            var order = await _dataContext.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound(); 
            }

          
            if (order.Status == 3)
            {
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync(); 
                TempData["SuccessMessage"] = "Xác nhận đã xóa đơn hàng thành công";
            }

            return RedirectToAction("Index", "Orders");
        }
        public async Task<IActionResult> OrderDetail(string orderId)
        {
            // Tìm đơn hàng theo OrderId
            var order = await _dataContext.Orders
                                           .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Lấy OrderCode của đơn hàng vừa tìm được
            var orderCode = order.OrderCode;

            // Lấy chi tiết đơn hàng từ OrderDetails bằng OrderCode
            var orderDetails = await _dataContext.OrderDetails
                                                  .Where(od => od.OrderCode == orderCode)
                                                  .ToListAsync();

            // Lấy thông tin sách từ bảng Books dựa trên BookId trong OrderDetails
            var books = await _dataContext.Books
                                           .Where(b => orderDetails.Select(od => od.BookId).Contains(b.BookId))
                                           .ToListAsync();

            // Kết nối thông tin chi tiết đơn hàng với thông tin sách, bao gồm cả Image
            var orderItems = orderDetails.Select(od => new
            {
                Book = books.FirstOrDefault(b => b.BookId == od.BookId),
                od.Quantity,
                od.Price
            }).ToList();

            // Truyền danh sách orderItems vào ViewBag
            ViewBag.OrderItems = orderItems;

            // Trả về view với thông tin đơn hàng
            return View(order);
        }
    }
}
