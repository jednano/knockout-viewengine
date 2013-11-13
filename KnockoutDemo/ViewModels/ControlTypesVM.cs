using System.Collections.Generic;

namespace KnockoutDemo.ViewModels
{
	public class ControlTypesVm
	{
		public string StringValue { get; private set; }
		public string PasswordValue { get; private set; }
		public bool BooleanValue { get; private set; }
		public IEnumerable<string> OptionValues { get; private set; }
		public string SelectedOptionValue { get; private set; }
		public IEnumerable<string> MultipleSelectedOptionValues { get; private set; }
		public string RadioSelectedOptionValue { get; private set; }

		public ControlTypesVm()
		{
			StringValue = "Hello";
			PasswordValue = "mypass";
			BooleanValue = true;
			OptionValues = new [] {"Alpha", "Beta", "Gamma"};
			SelectedOptionValue = "Gamma";
			MultipleSelectedOptionValues = new [] {"Alpha"};
			RadioSelectedOptionValue = "Beta";
		}
	}
}