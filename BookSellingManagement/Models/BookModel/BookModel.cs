using BookSellingManagement.Models.Categories;
using BookSellingManagement.Models.Authors;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BookSellingManagement.Reponsitory.Validation;

namespace BookSellingManagement.Models.Book
{
    public class BookModel
    {
        [Key]
        public string BookId { get; set; } = null!;
        public string BookCode { get; set; } = null!;
        [Required(ErrorMessage ="Yêu cầu nhập tên sách")]
        public string BookName { get; set; } = null!;
        public string BookSlug { get; set; } = null!;
        
   
        [Required(ErrorMessage = "Yêu cầu nhập giá")]
        public int Price { get; set; }
        
        public string? Description { get; set; }


        [Required(ErrorMessage = "Yêu cầu nhập số lượng sách")]
        public int ImportedQuantity { get; set; } 
       
        public int SoldQuantity { get; set; }
        
        public int RemainingQuantity 
        { 
            get { return ImportedQuantity - SoldQuantity; } 
        }
        
        public bool IsActive { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập vị trí để sách")]

        public string Location { get; set; }

        public string Image { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn tác giả")]
        public string AuthorId { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn thể loại")]
        public string CategoryId { get; set; }
  
        public AuthorModel Author { get; set; }

        public CategoryModel Category { get; set; }
        [NotMapped]
        [FileExtension]
        [Required(ErrorMessage = "Yêu cầu tải ảnh sách")]

        public IFormFile? ImageUpload {  get; set; }
    }
}
