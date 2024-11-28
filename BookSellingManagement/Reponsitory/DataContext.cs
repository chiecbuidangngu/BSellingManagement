using BookSellingManagement.Models.Authors;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Models.Categories;
using BookSellingManagement.Models.OrderModel;
using BookSellingManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BookSellingManagement.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<AuthorModel> Authors { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<WishlistModel> Wishlists { get; set; }

    }
}
