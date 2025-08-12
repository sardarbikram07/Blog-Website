using BCrypt.Net;
using blogapp.Data;
using blogapp.Models;
using blogapp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


namespace blogapp.Controllers
{
    public class AuthController : Controller
    {
        private readonly BlogDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmailService _emailService;

        public AuthController(BlogDBContext context, EmailService emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(UserRegistrationViewModel model)
        {
            // Additional null checks
            if (model == null)
            {
                ModelState.AddModelError("", "Invalid registration data.");
                return View(new UserRegistrationViewModel());
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Name) || 
                string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Create new user
                var user = new User
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    DateTime = DateTime.Now
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Your account is pending approval. You will be notified once an administrator reviews your request.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Message = "Invalid email or password.";
                return View();
            }

            // Check if user is approved
            if (user.BlogAccessStatus == AccessStatus.Pending)
            {
                ViewBag.Message = "Your account is pending approval. Please contact administrator.";
                return View();
            }
            
            if (user.BlogAccessStatus == AccessStatus.Rejected)
            {
                ViewBag.Message = "Your account has been rejected. Please contact administrator for more information.";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("ProfileImagePath", user.ProfileImagePath ?? "");
            return RedirectToAction("Index", "Feed");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email not found");
                return View();
            }

            // Generate a token (secure random GUID or JWT or Identity token)
            var token = Guid.NewGuid().ToString();

            // Save token and expiry to DB (or use built-in Identity if available)
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            // Create reset link
            var resetLink = Url.Action("ResetPassword", "Auth", new { token, email = user.Email }, Request.Scheme);

            // Send email
            await _emailService.SendEmailAsync(user.Email, "Reset Your Password",
                $"Click this link to reset your password: <a href='{resetLink}'>Reset Password</a>");

            ViewBag.Message = "Reset link sent. Check your email.";
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid password reset request.");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model); // this will load the ResetPassword.cshtml page
        }
        [HttpPost]
        
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ResetMessage"] = "User not found.";
                return RedirectToAction("Login", "Auth");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["ResetMessage"] = "✅ Password updated successfully!";
                return RedirectToAction("Login", "Auth");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult CheckAuthStatus()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            
            var isAuthenticated = userId.HasValue && !string.IsNullOrEmpty(userName);
            
            return Json(new { 
                isAuthenticated = isAuthenticated,
                userId = userId,
                userName = userName
            });
        }


    }
}
