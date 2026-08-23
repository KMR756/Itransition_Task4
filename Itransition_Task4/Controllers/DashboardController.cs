using Microsoft.AspNetCore.Mvc;

namespace Itransition_Task4.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
