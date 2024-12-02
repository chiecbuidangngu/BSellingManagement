using System.Net.Mail;
using System.Net;

namespace BookSellingManagement.Areas.Admin.Reponsitory
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("buihuyenhd2k2@gmail.com", "tilnfkicxcdllxsw")
            };

            return client.SendMailAsync(
                new MailMessage(from: "buihuyenhd2k2@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }


}
