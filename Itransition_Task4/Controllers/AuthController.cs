using Itransition_Task4.Data;
using Itransition_Task4.Dto;
using Itransition_Task4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Controllers
{
    public class AuthController(AppDbContext _context) : Controller
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
            var exitingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if(exitingUser == null)
            {
                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = dto.PasswordHash,
                    FullName = dto.FullName
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

            }
            else
            {
                ViewBag.ErrorMessage = "User with this email already exits.";
                return View("Register");
            }
            TempData["SuccessMessage"] = "User created successfully. Please log in.";

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> LoginUser(UserDto dto)
        {
            var isUserExisit = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if(isUserExisit == null)
            {
                ViewBag.ErrorMessage = "User with this email does not Exits.";
                return View("Login");
            }
            else
            {
                if(isUserExisit.PasswordHash == dto.PasswordHash)
                {

                    return RedirectToAction("Index","Dashboard");
                }
                else
                {
                    ViewBag.ErrorMessage = "Incorrect Password";
                    return View("Login");
                }
            }
        }
    }
}
