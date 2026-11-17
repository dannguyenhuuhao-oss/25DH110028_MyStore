using System.Linq;
using System.Web.Mvc;
using _25DH110028_MyStore.Models;

namespace _25DH110028_MyStore.Areas.Admin.Controllers
{
    public class StockController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index()
        {
            ViewBag.InStock = db.Products.Where(p => p.Quantity > 0).OrderBy(p => p.ProductName).ToList();
            ViewBag.NeedRestock = db.Products.Where(p => p.Quantity == 0).OrderBy(p => p.ProductName).ToList();
            return View();
        }
    }
}