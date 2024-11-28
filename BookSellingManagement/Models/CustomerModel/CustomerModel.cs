using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Models.CustomerModel
{
    [Keyless]
    public class CustomerModel
    {
        public string CustomerId { get; set; } = null!;
        public string CustomerCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public bool Gender { get; set; }
        public string? Address { get; set; }
     
    }
}
