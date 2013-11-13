using System;
using System.Collections.Generic;
using System.Globalization;
using Knockout.ViewModels;

namespace Knockout.Tests.ViewModels
{
	class PagedGrid : SimpleGrid
	{
		private readonly List<Item> _items;

		public PagedGrid(List<Item> items)
		{
			_items = items;
			Data = items;
			Columns = new IGridColumn[]
			          	{
			          		new ItemNameColumn {HeaderText = "Item Name"},
			          		new SalesCountColumn {HeaderText = "Sales Count"},
			          		new PriceColumn {HeaderText = "Price"}
			          	};
			PageSize = 4;
		}

		public void AddItem()
		{
			_items.Add(new Item{Name = "New Item", Sales = 0, Price = 100});
		}

		public void SortByName()
		{
			_items.Sort((x, y) => String.Compare(x.Name, y.Name,
				StringComparison.Ordinal));
		}

		public void JumpToFirstPage()
		{
			CurrentPageIndex = 0;
		}

		public class Item
		{
			public string Name { get; set; }
			public int Sales { get; set; }
			public double Price { get; set; }
		}

		private class ItemNameColumn : GridColumn, IGridColumn
		{
			public string RowText(object item)
			{
				return ((Item)item).Name;
			}
		}

		private class SalesCountColumn : GridColumn, IGridColumn
		{
			public string RowText(object item)
			{
				return ((Item)item).Sales.ToString(CultureInfo.InvariantCulture);
			}
		}

		private class PriceColumn : GridColumn, IGridColumn
		{
			public string RowText(object item)
			{
				return string.Format("${0:C}", ((Item)item).Price);
			}
		}
	}
}
