using BookSellingManagement.Models.Book;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookSellingManagement.Models.Categories
{
    public class CategoryModel
    {
        [Key]
        public string CategoryId { get; set; } = null!;
        public string CategoryCode { get; set; } = null!;
        [Required,MinLength(4,ErrorMessage ="Yêu cầu nhập Tên thể loại")]
        public string CategoryName { get; set; } = null!;
      
        public string CategorySlug { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<BookModel>? Books { get; set; }

    }
}