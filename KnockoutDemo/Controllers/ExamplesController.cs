using System.Collections.Generic;
using System.Web.Mvc;
using KnockoutDemo.ViewModels;

namespace KnockoutDemo.Controllers
{
	public class ExamplesController : Controller
	{
		[ActionName("hello-world")]
		public ActionResult HelloWorld()
		{
			ViewData["item"] = new HelloWorld("Planet", "Earth");
			return View("HelloWorld");
		}

		[ActionName("click-counter")]
		public ActionResult ClickCounter()
		{
			ViewData["item"] = new ClickCounter();
			return View("ClickCounter");
		}

		[ActionName("simple-list")]
		public ActionResult SimpleList()
		{
			ViewData["item"] = new SimpleList {"Alpha", "Beta", "Gamma"};
			return View("SimpleList");
		}

		[ActionName("better-list")]
		public ActionResult BetterList()
		{
			ViewData["item"] = new BetterList();
			return View("BetterList");
		}

		[ActionName("control-types")]
		public ActionResult ControlTypes()
		{
			ViewData["item"] = new ControlTypesVm();
			return View("ControlTypes");
		}

		[ActionName("collections")]
		public ActionResult Collections()
		{
			ViewData["item"] = new CollectionsVm();
			return View("Collections");
		}

		public ActionResult Index()
		{
			ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
			return View();
		}
	}
}
