namespace KnockoutDemo.ViewModels
{
	public class HelloContinent
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string FullName
		{
			get { return string.Concat(FirstName, " ", LastName); }
		}

        public HelloContinent(string first, string last)
		{
			FirstName = first;
			LastName = last;
		}
	}
}