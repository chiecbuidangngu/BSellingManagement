using BookSellingManagement.Models.Book;
using BookSellingManagement.Reponsitory.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSellingManagement.Models.Authors
{
    
    public class AuthorModel
    {
        [Key]
        public string AuthorId { get; set; } = null!;
        public string AuthorCode { set; get; } = null!;
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Tên tác giả")] 
        public string AuthorName { set; get; } = null!;
        public string AuthorSlug { set; get; } = null!;
        public string? Information { set; get; }
        public string Image { get; set; } = "noimage.jpg";

        public bool IsActive { get; set; }

        [NotMapped]
        [FileExtension]
        [Required(ErrorMessage = "Yêu cầu tải ảnh sách")]

        public IFormFile ImageUpload { get; set; }
        public int SumBook { set; get; }
        public IEnumerable<BookModel>? Books { get; set; }
    }
}
