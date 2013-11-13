using System.Collections.Generic;

namespace KnockoutDemo.ViewModels
{
	public class BetterList : List<string>
	{
		public string ItemToAdd { get; private set; }
		public List<string> AllItems { get { return this; } }
		public List<string> SelectedItems { get; private set; }

		public BetterList()
			: base(new [] { "Fries", "Eggs Benedict", "Ham", "Cheese" })
		{
			ItemToAdd = string.Empty;
			SelectedItems = new List<string> {"Ham"}; // Initial selection
		}

		public void AddItem()
		{
			// Prevent blanks and duplicates
			if (ItemToAdd != string.Empty && IndexOf(ItemToAdd) < 0)
				AllItems.Add(ItemToAdd);
			ItemToAdd = string.Empty; // Clear the text box
		}

		public void RemoveSelected()
		{
			foreach (var item in SelectedItems)
				Remove(item);
			SelectedItems.Clear();
		}

		public void SortItems()
		{
			Sort();
		}
	}
}