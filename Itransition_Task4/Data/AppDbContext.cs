using Itransition_Task4.Models;
using Microsoft.EntityFrameworkCore;

namespace Itransition_Task4.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
       
        public DbSet<User> Users { get; set; }
    }

}
