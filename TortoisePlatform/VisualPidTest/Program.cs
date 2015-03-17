using System;

namespace TortoisePlatform
{
	class Program
	{
		private static Random Rnd;
		public static double GetVal()
		{
			return double.Parse(Console.ReadLine());
		}


		static void Main(string[] args)
		{
			Rnd = new Random();
			var regulator = new PIDRegulator(1.0, 1.0, 0.15, 10.0, -10.0, 10.0, -10.0, GetVal, GetVal, Console.WriteLine);
			regulator.Run();
		}
	}
}
