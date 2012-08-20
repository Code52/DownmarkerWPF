using System.Web.Mvc;

namespace MetaWebLogSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}