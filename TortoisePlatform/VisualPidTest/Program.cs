using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using PidLibrary;
using Timer = System.Timers.Timer;

namespace VisualPidTest
{
	class Program
	{
		private const int WindowSize = 500;

		private const int MaxInput = 100;
		private const int MinInput = 0;

		private const int MaxOutput = 255;
		private const int MinOutput = -255;

		private static TrackBar InVal;
		private static TrackBar ExpectedVal;
		private static TrackBar OutVal;

		private static PIDRegulator Regulator;
		private static TimeSpan SleepTime = TimeSpan.FromMilliseconds(10);
		private static Timer Timer;

		public static double GetActual()
		{
			return InVal.Value;
		}

		public static double GetExpected()
		{
			return ExpectedVal.Value;
		}

		public static void SetVal(double val)
		{
			OutVal.Value = (int)val;
		}

		static void Main(string[] args)
		{
			InVal = new TrackBar {Maximum = MaxInput, Minimum = MinInput, Width = WindowSize, Location = new Point(0, 0)};

			OutVal = new TrackBar {Maximum = MaxOutput, Minimum = MinOutput, Width = WindowSize, Location = new Point(0, 60)};

			ExpectedVal = new TrackBar {Maximum = MaxInput, Minimum = MinInput, Width = WindowSize, Location = new Point(0, 120)};

			Regulator = new PIDRegulator(1.0, 1.0, 1, MaxInput, MinInput, MaxOutput, MinOutput);
			

			var form = new Form
			{
				ClientSize = new Size(WindowSize, WindowSize),
			};
			form.Controls.AddRange(new Control[]{InVal, OutVal, ExpectedVal});

			Run();
			form.ShowDialog();
		}

		public static void Run()
		{
			Timer = new Timer(SleepTime.TotalMilliseconds);
			Timer.Elapsed += Compute;
			Timer.Start();
		}

		public static void Compute(object state, ElapsedEventArgs elapsedEventArgs)
		{
			SetVal(Regulator.Compute(GetActual(), GetExpected(), TimeSpan.FromMilliseconds(Timer.Interval)));
		}
	}
}
