using System;
namespace Nibriboard
{
	public static class Env
	{
		public static readonly DateTime ServerStartTime = DateTime.Now;

		public static double SecondsSinceStart {
			get {
				TimeSpan timeSinceStart = DateTime.Now - ServerStartTime;
				return Math.Round(timeSinceStart.TotalSeconds, 4);
			}
		}
	}
}
