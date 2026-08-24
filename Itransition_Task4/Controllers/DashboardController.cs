using Itransition_Task4.Data;
using Itransition_Task4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Controllers
{
    public class DashboardController(AppDbContext _context) : Controller
    {
        
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.LastLoginTime.HasValue)
                .ThenByDescending(u => u.LastLoginTime)
                .ThenByDescending(u => u.RegistrationTime)
                .ToListAsync();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> BlockUsers([FromForm] List<int> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    user.Status = UserStatus.Blocked;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{users.Count} user(s) blocked successfully.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUsers([FromForm] List<int> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                foreach (var user in users)
                {
                    if (user.Status == UserStatus.Blocked)
                    {
                        user.Status = string.IsNullOrEmpty(user.VerificationToken) ? UserStatus.Active : UserStatus.Unverified;
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{users.Count} user(s) unblocked successfully.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUsers([FromForm] List<int> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{users.Count} user(s) deleted successfully.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnverified()
        {
            var unverifiedUsers = await _context.Users.Where(u => u.Status == UserStatus.Unverified).ToListAsync();
            if (unverifiedUsers.Any())
            {
                _context.Users.RemoveRange(unverifiedUsers);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{unverifiedUsers.Count} unverified user(s) deleted.";
            }
            return RedirectToAction("Index");
        }
    }
}