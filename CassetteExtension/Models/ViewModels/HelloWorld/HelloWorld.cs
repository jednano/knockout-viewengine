namespace KnockoutDemo.ViewModels
{
	public class HelloWorld
	{
        /*
         * var ViewModel = function(first, last) {
         * this.firstName = ko.observable(first);
         * this.lastName = ko.observable(last);
         * 
         * this.fullName = ko.computed(function() {
         * return this.firstName() + " " + this.lastName();
         * }, this);
         * };
         * 
         * ko.applyBindings(new ViewModel("Planet", "Earth")); // separate js file using ajax
         */

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