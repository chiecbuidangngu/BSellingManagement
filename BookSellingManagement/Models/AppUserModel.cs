using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;





namespace BookSellingManagement.Models
{
    public class AppUserModel : IdentityUser
    {
     
        public string FullName { get; set; }
        public string Address { get; set; }
        public string RoleId { get; set; }

    
    }
}


