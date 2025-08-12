using blogapp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Linq;

public class AccountController : Controller
{
    private readonly BlogDBContext _context;
    public AccountController(BlogDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            // Send Email (replace with your real SMTP config)
            var message = new MailMessage();
            message.To.Add(new MailAddress(email));
            message.Subject = "Your Password Recovery";
            message.Body = $"Hello {user.Name},\n\nYour password is: {user.PasswordHash}";
            message.IsBodyHtml = false;

            using (var smtp = new SmtpClient())
            {
                smtp.Host = "smtp.gmail.com"; // e.g., Gmail SMTP
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("your@gmail.com", "your-app-password");
                smtp.EnableSsl = true;
                smtp.Send(message);
            }

            ViewBag.Message = "Password sent to your email.";
        }
        else
        {
            ViewBag.Message = "Email not found.";
        }

        return View();
    }
}
