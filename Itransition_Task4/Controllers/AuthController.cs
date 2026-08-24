using System.Security.Claims;
using Itransition_Task4.Data;
using Itransition_Task4.Dto;
using Itransition_Task4.Models;
using Itransition_Task4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Controllers
{
    public class AuthController(
        AppDbContext _context,
        IPasswordHasher<User> _passwordHasher,
        IServiceScopeFactory _scopeFactory) : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PasswordHash))
            {
                ViewBag.ErrorMessage = "Password cannot be empty.";
                return View("Register", dto);
            }

            var token = Guid.NewGuid().ToString("N");
            var newUser = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Status = UserStatus.Unverified,
                RegistrationTime = DateTime.UtcNow,
                VerificationToken = token
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.PasswordHash);

            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ViewBag.ErrorMessage = "User with this email already exists.";
                return View("Register", dto);
            }

            var verifyLink = Url.Action("VerifyEmail", "Auth", new { token }, Request.Scheme);

            
            _ = Task.Run(async () =>
            {
                if (verifyLink != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendVerificationEmailAsync(newUser.Email, verifyLink);
                }
            });

            TempData["SuccessMessage"] = "Registration successful! Verification email sent. You may log in right away.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification token.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Verification token is invalid or expired.";
                return RedirectToAction("Login");
            }

            if (user.Status == UserStatus.Unverified)
            {
                user.Status = UserStatus.Active;
                user.VerificationToken = null; // Clear token after activation
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Email verified successfully! Status is now Active.";
            }
            else if (user.Status == UserStatus.Blocked)
            {
                TempData["ErrorMessage"] = "Email verified, but your account is currently blocked.";
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginUser(UserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login", dto);
            }

            if (user.Status == UserStatus.Blocked)
            {
                ViewBag.ErrorMessage = "Your account is blocked.";
                return View("Login", dto);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.PasswordHash ?? "");
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login", dto);
            }

            user.LastLoginTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}