using System.Collections.Generic;
using System.Linq;

namespace Knockout
{
	/// <summary>
	/// Singleton stand-in for ko Javascript library.
	/// </summary>
	public sealed class Ko
	{
		// ReSharper disable InconsistentNaming
		private static readonly Ko instance = new Ko();
		// ReSharper restore InconsistentNaming
		
		public KoUtils Utils;

		private Ko()
		{
			Utils = new KoUtils();
		}

		public static Ko Instance
		{
			get { return instance; }
		}

		public class KoUtils
		{
			public IEnumerable<object> Range(int min, int max)
			{
				return Enumerable.Range(min, max - min + 1).Cast<object>();
			}
		}
	}
}
