using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FiftyOne.Foundation.Mobile.Detection;
using MVC.Models;

namespace MVC.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            // Sets the title for the Home page.
            ViewBag.Title = "ASP.NET MVC Device Detection Example";

            return View();
        }

        public ActionResult About()
        {
            // Sets title and message for the About page.
            ViewBag.Title = "About This Example";
            ViewBag.Message = "51Degrees MVC example description page.";

            return View();
        }
    }
}
