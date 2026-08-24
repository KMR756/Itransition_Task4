using Itransition_Task4.Data;
using Itransition_Task4.Dto;
using Itransition_Task4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Controllers
{
    public class AuthController(AppDbContext _context, IPasswordHasher<User> _passwordHasher) : Controller
    {  
        public IActionResult Login()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public async Task<IActionResult> CreateUser(UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", dto);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "User with this email already exists.";
                return View("Register", dto);
            }

            var newUser = new User
            {
                Email = dto.Email,
                FullName = dto.FullName
            };

          
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.PasswordHash);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User created successfully. Please log in.";
            return RedirectToAction("Login");
        }

      
        public async Task<IActionResult> LoginUser(UserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login", dto);
            }

          
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.PasswordHash);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View("Login", dto);
            }

            
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.PasswordHash);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}