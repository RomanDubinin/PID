using System;
using System.IO.Ports;
using System.Threading.Tasks;
using PidLibrary;

namespace TortoisePlatform
{
	public class Program
	{
		private static SerialPort Com;

		private const int MaxInput = 1000;
		private const int MinInput = -1000;

		private const int MaxOutput = 255;
		private const int MinOutput = -255;

		private static SmartWheell[] Wheels;

		private static TimeSpan Period = TimeSpan.FromMilliseconds(50);

		private static void WriteCommand(int driveId, int mills, int speed)
		{
			var command = String.Format("M 0 {0} {1} {2}", driveId, mills, speed);
			Com.WriteLine(command + String.Format(" {0}", command.Length - 4));
		}

		private static string ReadCommand()
		{
			while (Com.BytesToRead < 20)
			{
			}
			return Com.ReadLine();
		}

		public static void Main(string[] args)
		{
			Wheels = new SmartWheell[4];

			for (var i = 0; i < 4; i++)
			{
				var regulator = new PIDRegulator(1, 0.1, 0.01, MaxInput, MinInput, MaxOutput, MinOutput);
				Wheels[i] = new SmartWheell(regulator, 50);
			}
			//Wheels[3].TicsPerPeriod = 100;
			//Wheels[2].TicsPerPeriod = 100;


			Com = new SerialPort("COM29", 115200);
			Com.Open();
			Com.WriteLine("SE 100");

			for (var i = 0; i < 4; i++)
			{
				WriteCommand(i, Period.Milliseconds, 10);
			}

			while (true)
			{
				var command = ReadCommand().Split(' ');
				if (command[0].CompareTo("FAIL") == 0)
					continue;

				if (command.Length != 5)
					continue;

				var id = int.Parse(command[1]);
				var currentTics = int.Parse(command[3]);
				//todo это надо менять
				if (id%2 == 0)
					currentTics *= -1;

				Wheels[id].CurrentTics += currentTics;
				if (command[0].CompareTo("DR") == 0)
				{
					Console.WriteLine("id: {0}, tics: {1}", id, Wheels[id].ExpectedTics - Wheels[id].CurrentTics);
					Wheels[id].ExpectedTics += Wheels[id].TicsPerPeriod;
					Task.Factory.StartNew(() => DoMath(id, Wheels[id].CurrentTics, Wheels[id].ExpectedTics));
					//Tics[id] = 0;
				}
			}
		}

		private static void DoMath(int id, int actualTics, int expectedTics)
		{
			var speed = Wheels[id].Regulator.Compute(0, expectedTics - actualTics, Period);

			//todo это надо менять
			if (id%2 == 0)
				speed *= -1;

			WriteCommand(id, Period.Milliseconds, (int) speed);
		}
	}
}