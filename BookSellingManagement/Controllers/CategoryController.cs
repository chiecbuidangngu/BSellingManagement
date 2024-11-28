
using BookSellingManagement.Models.Categories;
using BookSellingManagement.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BookSellingManagement.Controllers
{

    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string categoryId)
        {
       
            if (string.IsNullOrEmpty(categoryId))
            {
                return RedirectToAction("Index");
            }

            // Tìm danh mục dựa trên CategoryId
            CategoryModel category = await _dataContext.Categories.FindAsync(categoryId);
            if (category == null) return RedirectToAction("Index");

            // Lấy danh sách sách theo CategoryId
            var BooksByCategory = await _dataContext.Books
                .Where(c => c.CategoryId == category.CategoryId)
                .OrderByDescending(c => c.CategoryId) // Sắp xếp ở đây
                .ToListAsync(); // Chỉ gọi ToListAsync một lần

            // Truyền dữ liệu vào view
            ViewBag.BooksByCategory = BooksByCategory;
            ViewBag.Category = category;

            return View(BooksByCategory); // Trả về danh sách sách đã sắp xếp
        }

    }
}


  

