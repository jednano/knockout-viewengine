namespace KnockoutDemo.ViewModels
{
	public class HelloWorld
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string FullName
		{
			get { return string.Concat(FirstName, " ", LastName); }
		}
		
		public HelloWorld(string first, string last)
		{
			FirstName = first;
			LastName = last;
		}
	}
}