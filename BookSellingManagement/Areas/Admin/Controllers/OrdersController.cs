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


        public async Task<IActionResult> Index(int pg = 1, string search = "")
        {
            const int pageSize = 10; // Số lượng đơn hàng mỗi trang

            if (pg < 1)
            {
                pg = 1;
            }

            // Lấy danh sách đơn hàng
            var ordersQuery = _dataContext.Orders
                .OrderByDescending(c => c.CreateDate) // Sắp xếp theo ngày tạo mới nhất
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm nếu có từ khóa
            if (!string.IsNullOrEmpty(search))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderCode.Contains(search)); // Tìm theo mã đơn hàng
                ViewBag.Search = search; // Để hiển thị lại từ khóa trên giao diện
            }

            // Tổng số đơn hàng sau khi áp dụng tìm kiếm
            int recsCount = await ordersQuery.CountAsync();

            var pager = new Paginate(recsCount, pg, pageSize);
            int recSkip = (pg - 1) * pageSize;

            // Áp dụng phân trang
            var data = await ordersQuery
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();

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
