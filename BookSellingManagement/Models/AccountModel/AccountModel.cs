using Microsoft.EntityFrameworkCore;

namespace BookSellingManagement.Models.AccountModel
{
    [Keyless]
    public class AccountModel
    {
            public string Email { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public bool Gender { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string Address { get; set; } = null!;
            public string PhoneNumber { get; set; } = null!;
        }
    }

