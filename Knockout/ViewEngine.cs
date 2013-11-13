using System.Web.Mvc;

namespace Knockout
{
	public class ViewEngine : VirtualPathProviderViewEngine
	{
		public ViewEngine()
		{
			ViewLocationFormats = new[] {"~/Views/{1}/{0}.cshtml"};
			PartialViewLocationFormats = ViewLocationFormats;
		}

		protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
		{
			return new View(viewPath);
		}

		protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
		{
			return new View(partialPath);
		}
	}
}
