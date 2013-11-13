using System.Collections.Generic;

namespace Knockout.Tests.ViewModels
{
	public class Person
	{
		public string Name { get; private set; }
		public List<string> Children { get; private set; }
		
		public Person(string name, IEnumerable<string> children = null)
		{
			Name = name;
			Children = new List<string>();
			if (children == null) return;
			foreach (var child in children)
				AddChild(child);
		}

		public void AddChild(string name = "New child")
		{
			Children.Add(name);
		}
	}

	public class Collections
	{
		public IEnumerable<Person> People { get; private set; } 
		public bool ShowRenderTimes { get; private set; }

		public Collections()
		{
			People = new[]
			         	{
			         		new Person("Annabelle", new[] {"Arnie", "Anders", "Apple"}),
			         		new Person("Bertie", new[] {"Boutros-Boutros", "Brianna", "Barbie", "Bee-bop"}),
			         		new Person("Charles", new[] {"Cayenne", "Cleopatra"})
			         	};
			ShowRenderTimes = false;
		}
	}
}