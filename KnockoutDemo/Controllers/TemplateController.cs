using System.Web.Mvc;
using KnockoutDemo.ViewModels;

namespace KnockoutDemo.Controllers
{
    public class TemplateController : Controller
    {
        //
        // GET: /Template/

		public PartialViewResult HelloWorld()
		{
			return PartialView("~/Views/Shared/Templates/HelloWorld.cshtml");
		}

    }
}
