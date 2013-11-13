using System.Collections.Generic;
using System.IO;
using HtmlAgilityPack;
using Knockout.Tests.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Knockout.Tests
{
	[TestClass]
	public class ExamplesTest
	{
		private const string Location = "../../{0}/{1}.html";

		private static string GetHtml(string folderName, string fileName, object data = null)
		{
			var path = string.Format(Location, folderName, fileName);
			using (var sr = new StreamReader(path))
				return new BoundView(sr.ReadToEnd(), data).DocumentNode.InnerHtml;
		}

		private static void TestDataBind(object data)
		{
			var fileName = data.GetType().Name;

			// Force closing of option tags.
			// TODO: might need to enforce more of these later.
			HtmlNode.ElementsFlags.Remove("option");

			var liveBound = GetHtml("Views", fileName, data);
			var preBound = GetHtml("BoundViews", fileName);

			Assert.AreEqual(liveBound, preBound);
		}

		[TestMethod]
		public void TestHelloWorld()
		{
			TestDataBind(new HelloWorld("Planet", "Earth"));
		}

		[TestMethod]
		public void TestClickCounter()
		{
			TestDataBind(new ClickCounter());
		}

		[TestMethod]
		public void TestSimpleList()
		{
			TestDataBind(new SimpleList {"Alpha", "Beta", "Gamma"});
		}

		[TestMethod]
		public void TestBetterList()
		{
			TestDataBind(new BetterList());
		}

		[TestMethod]
		public void TestControlTypes()
		{
			TestDataBind(new ControlTypes());
		}

		[TestMethod]
		public void TestCollections()
		{
			TestDataBind(new Collections());
		}

		[TestMethod]
		public void TestPagedGrid()
		{
			var initialData = new List<PagedGrid.Item>
			                  	{
			                  		new PagedGrid.Item {Name = "Well-Travelled Kitten", Sales = 352, Price = 75.95},
			                  		new PagedGrid.Item {Name = "Speedy Coyote", Sales = 89, Price = 190},
			                  		new PagedGrid.Item {Name = "Furious Lizard", Sales = 152, Price = 25},
			                  		new PagedGrid.Item {Name = "Indifferent Monkey", Sales = 1, Price = 99.95},
			                  		new PagedGrid.Item {Name = "Brooding Dragon", Sales = 0, Price = 6350},
			                  		new PagedGrid.Item {Name = "Ingenious Tadpole", Sales = 39450, Price = .35},
			                  		new PagedGrid.Item {Name = "Optimistic Snail", Sales = 420, Price = 1.5}
			                  	};
			TestDataBind(new PagedGrid(initialData));
		}
	}
}
