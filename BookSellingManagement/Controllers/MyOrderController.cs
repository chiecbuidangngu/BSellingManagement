using BookSellingManagement.Models;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookSellingManagement.Controllers
{
    public class MyOrderController : Controller
    {


        private readonly DataContext _dataContext;
        public MyOrderController(DataContext context)
        {

            _dataContext = context;

        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            const int pageSize = 10;
            if (pg < 1)
            {
                pg = 1;
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Lấy danh sách đơn hàng của người dùng, đã sắp xếp theo ngày tạo (mới nhất đầu tiên)
            var ordersQuery = _dataContext.Orders
                .Where(od => od.Username == userEmail)
                .OrderByDescending(od => od.CreateDate);

            // Tính tổng số bản ghi
            int recsCount = await ordersQuery.CountAsync();

            // Tính số bản ghi cần bỏ qua dựa trên trang hiện tại
            int recSkip = (pg - 1) * pageSize;

            // Lấy dữ liệu của trang hiện tại
            var orders = await ordersQuery
                .Skip(recSkip)
                .Take(pageSize)
                .ToListAsync();

            // Tạo đối tượng phân trang
            var pager = new Paginate(recsCount, pg, pageSize);

            // Truyền pager và danh sách đơn hàng vào ViewBag
            ViewBag.Pager = pager;
            ViewBag.UserEmail = userEmail;

            return View(orders);
        }

        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var order = await _dataContext.Orders
                                          .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == 1)
            {
                order.Status = 3;
                await _dataContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            return RedirectToAction("MyOrders");
        }
        public async Task<IActionResult> ConfirmReceipt(string orderId)
        {
            // Tìm đơn hàng theo orderId
            var order = await _dataContext.Orders
                                           .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == 4) // Đang giao hàng
            {
                // Cập nhật trạng thái sang "Giao thành công"
                order.Status = 5;

                await _dataContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xác nhận đã nhận đơn hàng thành công";
            }
            return RedirectToAction("MyOrders");
        }

        public async Task<IActionResult> OrderDetail()
        {  
            return View("OrderDetail");
        }





    }
}
