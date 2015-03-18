using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TortoisePlatform
{
	class Program
	{
		private static int WindowSize = 500;

		private static int MaxInput = 100;
		private static int MinInput = 0;

		private static int MaxOutput = 255;
		private static int MinOutput = -255;
		
		private static TrackBar InVal;
		private static TrackBar ExpectedVal;
		private static TrackBar OutVal;

		private static PIDRegulator Regulator;
		private static TimeSpan SleepTime = TimeSpan.FromMilliseconds(100);

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
			InVal = new TrackBar();
			InVal.Maximum = MaxInput;
			InVal.Minimum = MinInput;
			InVal.Width = WindowSize;
			InVal.Location = new Point(0, 0);

			OutVal = new TrackBar();
			OutVal.Maximum = MaxOutput;
			OutVal.Minimum = MinOutput;
			OutVal.Width = WindowSize;
			OutVal.Location = new Point(0, 60);
			
			ExpectedVal = new TrackBar();
			ExpectedVal.Maximum = MaxInput;
			ExpectedVal.Minimum = MinInput;
			ExpectedVal.Width = WindowSize;
			ExpectedVal.Location = new Point(0, 120);
			
			Regulator = new PIDRegulator(1.0, 1.0, 1, MaxInput, MinInput, MaxOutput, MinOutput, GetActual, GetExpected, SetVal);
			

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
			var timer = new Timer(SleepTime.TotalMilliseconds);
			timer.Elapsed += Compute;
			timer.Start();
		}

		public static void Compute(object state, ElapsedEventArgs elapsedEventArgs)
		{
			Regulator.Compute();
		}
	}
}
