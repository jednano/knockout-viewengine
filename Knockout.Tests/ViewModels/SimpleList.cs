using System.Collections.Generic;

namespace Knockout.Tests.ViewModels
{
	public class SimpleList : List<string>
	{
		public List<string> Items { get { return this; } } 
		public string ItemToAdd { get; private set; }

		public SimpleList()
		{
			ItemToAdd = string.Empty;
		}

		/// <summary>
		/// Adds the item. Writing to the "Items" property causes any associated UI to update.
		/// </summary>
		public void AddItem()
		{
			if (ItemToAdd == string.Empty) return;
			Items.Add(ItemToAdd);
			// The text box is bound to the ItemToAdd property, so this clears the text box.
			ItemToAdd = string.Empty;
		}
	}
}