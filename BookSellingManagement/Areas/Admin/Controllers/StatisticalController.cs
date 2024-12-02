
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class StatisticalController : Controller
    {
        private readonly DataContext _dataContext;

        public StatisticalController(DataContext context)
        {
            _dataContext = context;
        }

      public async Task<IActionResult> Index()
        {
            // Số lượng hàng tồn
            var count_stock = await _dataContext.Books.SumAsync(b => b.ImportedQuantity - b.SoldQuantity);
            ViewBag.CountStock = count_stock;

            // Số lượng bán ra (bỏ qua các đơn hàng bị hủy)
            var count_sold = await _dataContext.OrderDetails
                                               .Where(od => _dataContext.Orders
                                                                        .Any(o => o.OrderCode == od.OrderCode && o.Status != 3))
                                               .SumAsync(od => od.Quantity);
            ViewBag.CountSold = count_sold;

            // Tổng doanh thu (bỏ qua các đơn hàng bị hủy)
            var total_revenue = await _dataContext.OrderDetails
                                                  .Where(od => _dataContext.Orders
                                                                           .Any(o => o.OrderCode == od.OrderCode && o.Status != 3))
                                                  .SumAsync(od => od.Quantity * od.Price);
            ViewBag.TotalRevenue = total_revenue;

            var count_orders = _dataContext.Orders.Select(od => od.OrderId).Distinct().Count();
            ViewBag.CountOrders = count_orders;

            // Lấy top 10 sách bán chạy (bỏ qua các đơn hàng bị hủy)
            var topBooks = _dataContext.OrderDetails
                .Where(od => _dataContext.Orders
                                         .Any(o => o.OrderCode == od.OrderCode && o.Status != 3)) 
                .GroupBy(od => od.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .Join(_dataContext.Books,
                      od => od.BookId,
                      b => b.BookId,
                      (od, b) => new
                      {
                          BookName = b.BookName,
                          TotalSold = od.TotalSold
                      })
                .OrderByDescending(b => b.TotalSold)
                .Take(10) 
                .ToList();

            // Truyền dữ liệu vào View dưới dạng JSON để sử dụng trong Chart.js
            ViewBag.TopBooks = topBooks.Select(b => b.BookName).ToList();
            ViewBag.TopBooksSold = topBooks.Select(b => b.TotalSold).ToList();
            // Lấy doanh thu và số lượng bán theo tháng (bỏ qua các đơn bị hủy)
            var monthlyData = await _dataContext.Orders
                .Where(o => o.CreateDate != null && o.Status != 3)
                .GroupBy(o => new { o.CreateDate.Month, o.CreateDate.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            ViewBag.MonthlyRevenue = monthlyData.Select(m => m.TotalRevenue).ToList();
            ViewBag.MonthlySales = monthlyData.Select(m => new
            {
                Month = m.Month,
                Year = m.Year
            }).ToList();


            // Thống kê số lượng sách theo thể loại
            var categoryStats = _dataContext.Books
                .GroupBy(b => b.Category.CategoryName)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalBooks = g.Sum(b => b.ImportedQuantity - b.SoldQuantity) // Tổng số sách còn lại
                })
                .ToList();

            ViewBag.CategoryNames = categoryStats.Select(cs => cs.CategoryName).ToList();
            ViewBag.CategoryCounts = categoryStats.Select(cs => cs.TotalBooks).ToList();

            // Thống kê số lượng trạng thái đơn hàng
            var orderStatusStats = _dataContext.Orders
    .GroupBy(o => o.Status) // Nhóm theo trạng thái đơn hàng (int)
    .Select(g => new
    {
        Status = g.Key, // Giá trị int của trạng thái
        TotalOrders = g.Count() // Số lượng đơn hàng trong trạng thái đó
    })
    .ToList();

            // Ánh xạ từ int sang tên trạng thái
            var statusMapping = new Dictionary<int, string>
{
            { 1, "Đơn hàng mới" },
            { 2, "Đã thanh toán" },
            { 3, "Hủy đơn hàng" },
            { 4, "Đang giao hàng" },
            { 5, "Giao thành công" }
};

            // Chuyển đổi status từ int sang tên
            ViewBag.OrderStatuses = orderStatusStats
                .Select(os => statusMapping.ContainsKey(os.Status) ? statusMapping[os.Status] : "Không xác định")
                .ToList();

            ViewBag.OrderCounts = orderStatusStats.Select(os => os.TotalOrders).ToList();

            return View();


  

      
        }

    }
}
