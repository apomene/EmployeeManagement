using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
