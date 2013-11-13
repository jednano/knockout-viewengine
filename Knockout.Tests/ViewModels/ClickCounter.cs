namespace Knockout.Tests.ViewModels
{
	public class ClickCounter
	{
		public int NumberOfClicks { get; private set; }
		public bool HasClickedTooManyTimes
		{
			get { return NumberOfClicks >= 3; }
		}

		public ClickCounter()
		{
			NumberOfClicks = 0;
		}

		public void RegisterClick()
		{
			NumberOfClicks++;
		}

		public void ResetClicks()
		{
			NumberOfClicks = 0;
		}
	}
}