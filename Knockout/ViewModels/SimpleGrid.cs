using System.Collections.Generic;
using System.Linq;

namespace Knockout.ViewModels
{
	public interface IGridColumn
	{
		string HeaderText { get; set; }
		string RowText(object item);
	}

	public abstract class GridColumn
	{
		public string HeaderText { get; set; }
	}

	public class SimpleGrid
	{
		public IEnumerable<object> Data { get; set; }
		public IEnumerable<IGridColumn> Columns { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }

		public IEnumerable<object> ItemsOnCurrentPage
		{
			get { return Data.Skip(PageSize*CurrentPageIndex).Take(PageSize).ToArray(); }
		}

		public int MaxPageIndex
		{
			get { return Data.Count()/PageSize; }
		}
	}
}
