using Microsoft.AspNetCore.Mvc;

namespace E_BookStoreWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
