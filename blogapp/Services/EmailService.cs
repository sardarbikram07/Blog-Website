using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace blogapp.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string to, string subject, string html)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("BlogHub", "your-gmail@gmail.com")); // your Gmail address
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = html };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("suman15sep2004", "oxkc augu dpwt sjbj"); // Use App Password, not Gmail password
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
