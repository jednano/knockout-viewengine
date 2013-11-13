using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using HtmlAgilityPack;

namespace Knockout
{
	public class View : IView
	{
		private static readonly Dictionary<string, string> CachedFiles = new Dictionary<string, string>();
		public string ViewPath { get; private set; }

		public View(string viewPath)
		{
			ViewPath = viewPath;
		}

		public void Render(ViewContext viewContext, TextWriter writer)
		{
			#region Handle caching
			// TODO: Modified templates are not being reloaded.
			if (CachedFiles.ContainsKey(ViewPath) == false)
			{
				CachedFiles.Add(ViewPath, File.ReadAllText(
					viewContext.HttpContext.Server.MapPath(ViewPath)));
			}
			var sourceCode = CachedFiles[ViewPath];
			#endregion

			var data = viewContext.ViewData["item"];

			// Force closing of option tags.
			// TODO: might need to enforce more of these later.
			HtmlNode.ElementsFlags.Remove("option");

			new BoundView(sourceCode, data).Save(writer);
		}
	}
}
