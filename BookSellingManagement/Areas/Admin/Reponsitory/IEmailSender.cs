namespace BookSellingManagement.Areas.Admin.Reponsitory
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
