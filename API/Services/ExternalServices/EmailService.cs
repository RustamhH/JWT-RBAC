using API.Models;
using System.Net;
using System.Net.Mail;
namespace API.Services.ExternalServices
{
    public class EmailService
    {
        public MailAddress to { get; set; }
        public MailMessage email { get; set; }
        public MailAddress from { get; set; }



        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            from = new MailAddress(_configuration["Smtp:Gmail"]!);
        }

        // Methods

        public async Task<Result> sendMailAsync(string mail, string title, string text)
        {
            try
            {
                to = new MailAddress(mail);
                email = new MailMessage(from, to);
            }
            catch (Exception ex)
            {
                return new Error("SMTP_ERROR", ex.Message);
            }

            email.IsBodyHtml = true;
            email.Subject = title;
            email.Body = text;

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 25;
            smtp.Credentials = new NetworkCredential(from.Address, _configuration["Smtp:Password"]);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = true;

            try
            {
                await smtp.SendMailAsync(email);
            }
            catch (SmtpException ex)
            {
                return new Error("SMTP_ERROR", ex.Message);
            }
            return Result.Success();
        }
    }
}
