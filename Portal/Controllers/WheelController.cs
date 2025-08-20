using Microsoft.AspNetCore.Mvc;

namespace Portal.Controllers
{
    public class WheelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
