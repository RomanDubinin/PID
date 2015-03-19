using System;
using System.IO.Ports;
using System.Timers;

namespace TortoisePlatform
{
	public class Program
	{
		private static SerialPort Com;
		private static Timer SendTimer;
		private static Timer ReadTimer;

		public static void Main(string[] args)
		{
			Com = new SerialPort("COM32", 115200);
			
			Com.Open();
			SendTimer = new Timer(2000);
			SendTimer.Elapsed += Send;
			SendTimer.Start();

			ReadTimer = new Timer(5);
			ReadTimer.Elapsed += Read;
			ReadTimer.Start();
			//while (true)
			//{
			//	Com.WriteLine("SE 1000");
				
				
			//}
			Console.ReadLine();
			SendTimer.Stop();
			ReadTimer.Stop();
			Com.Close();
		}

		public static void Send(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			var command = "M 0 0 1000 100 10";
			Com.WriteLine(command);
			//Console.WriteLine(command);
		}

		public static void Read(object sender, ElapsedEventArgs elapsedEventArgs)
		{
            if(Com.BytesToRead > 20)
			    Console.WriteLine(Com.ReadLine());
			//Console.ReadKey();
			//Console.WriteLine("Read");
		}
	}
}
