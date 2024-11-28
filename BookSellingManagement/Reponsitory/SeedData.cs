using BookSellingManagement.Models.Categories;
using BookSellingManagement.Models.Authors;
using BookSellingManagement.Models.Book;
using BookSellingManagement.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;

public static class SeedData
{
    public static void SeedingData(DataContext _context)

    {
        _context.Database.Migrate();
        // Khởi tạo danh mục
        
                _context.SaveChanges();
           }
        
    }

                