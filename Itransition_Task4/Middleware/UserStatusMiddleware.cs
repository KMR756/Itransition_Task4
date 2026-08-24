using System.Security.Claims;
using Itransition_Task4.Data;
using Itransition_Task4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Middleware
{
    public class UserStatusMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            
            if (path.StartsWith("/auth/login") ||
                path.StartsWith("/auth/register") ||
                path.StartsWith("/auth/createuser") ||
                path.StartsWith("/auth/loginuser") ||
                path.StartsWith("/auth/verifyemail"))
            {
                await next(context);
                return;
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

                    // Redirect if user was deleted or blocked
                    if (user == null || user.Status == UserStatus.Blocked)
                    {
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/Auth/Login");
                        return;
                    }
                }
            }
            else
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            await next(context);
        }
    }
}