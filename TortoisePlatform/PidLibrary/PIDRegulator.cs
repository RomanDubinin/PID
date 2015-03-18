using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TortoisePlatform
{
	public delegate double GetDouble();
	public delegate void SetDouble(double value);

	public class PIDRegulator
	{
		#region Fields

		//Gains
		private double Kp;
		private double Ki;
		private double Kd;

		//Running Values
		private double LastPv;
		private double ErrSum;

		//Reading/Writing Values
		private readonly GetDouble ReadPv;
		private readonly GetDouble ReadSp;
		private readonly SetDouble WriteOv;

		//Max/Min Calculation
		private double PvMax;
		private double PvMin;
		private double outMax;
		private double outMin;

		//Threading and Timing
		private readonly TimeSpan SleepTime = TimeSpan.FromMilliseconds(100);
		//private Thread RunThread;

		#endregion

		#region Properties

		public double PGain
		{
			get { return Kp; }
			set { Kp = value; }
		}

		public double IGain
		{
			get { return Ki; }
			set { Ki = value; }
		}

		public double DGain
		{
			get { return Kd; }
			set { Kd = value; }
		}

		public double PVMin
		{
			get { return PvMin; }
			set { PvMin = value; }
		}

		public double PVMax
		{
			get { return PvMax; }
			set { PvMax = value; }
		}

		public double OutMin
		{
			get { return outMin; }
			set { outMin = value; }
		}

		public double OutMax
		{
			get { return outMax; }
			set { outMax = value; }
		}

		//public bool PIDOK
		//{
		//	get { return RunThread != null; }
		//}

		#endregion

		#region Construction / Deconstruction

		public PIDRegulator(double pG, double iG, double dG,
			double pMax, double pMin, double oMax, double oMin,
			GetDouble pvFunc, GetDouble spFunc, SetDouble outFunc)
		{
			Kp = pG;
			Ki = iG;
			Kd = dG;
			PvMax = pMax;
			PvMin = pMin;
			outMax = oMax;
			outMin = oMin;
			ReadPv = pvFunc;
			ReadSp = spFunc;
			WriteOv = outFunc;
		}

		#endregion

		#region Public Methods

		//public void Enable()
		//{
		//	if (RunThread != null)
		//		return;

		//	Reset();

		//	RunThread = new Thread(Run) {IsBackground = true, Name = "PID Processor"};
		//	RunThread.Start();
		//}

		//public void Disable()
		//{
		//	if (RunThread == null)
		//		return;

		//	RunThread.Abort();
		//	RunThread = null;
		//}

		public void Reset()
		{
			ErrSum = 0.0f;
		}

		#endregion

		#region Private Methods

		private double ScaleValue(double value, double valuemin, double valuemax, 
			double scalemin, double scalemax)
		{
			var vPerc = (value - valuemin)/(valuemax - valuemin);
			var bigSpan = vPerc*(scalemax - scalemin);

			var retVal = scalemin + bigSpan;

			return retVal;
		}

		private double Clamp(double value, double min, double max)
		{
			if (value > max)
				return max;
			if (value < min)
				return min;
			return value;
		}

		public void Compute(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (ReadPv == null || ReadSp == null || WriteOv == null)
				return;

			var pv = ReadPv();
			var sp = ReadSp();

			//We need to scale the pv to +/- 100%, but first clamp it
			pv = Clamp(pv, PvMin, PvMax);
			pv = ScaleValue(pv, PvMin, PvMax, -1.0f, 1.0f);

			//We also need to scale the setpoint
			sp = Clamp(sp, PvMin, PvMax);
			sp = ScaleValue(sp, PvMin, PvMax, -1.0f, 1.0f);

			//Now the error is in percent...
			var err = sp - pv;

			var pTerm = err*Kp;
			double iTerm = 0.0f;
			double dTerm = 0.0f;

			double partialSum = 0.0f;

			var dT = 1.0;

			partialSum = ErrSum + dT*err;
			iTerm = Ki*partialSum;


			if (dT != 0.0f)
				dTerm = Kd*(pv - LastPv)/dT;

			ErrSum = partialSum;
			LastPv = pv;

			//Now we have to scale the output value to match the requested scale
			var outReal = pTerm + iTerm + dTerm;

			outReal = Clamp(outReal, -1.0f, 1.0f);
			outReal = ScaleValue(outReal, -1.0f, 1.0f, outMin, outMax);

			//Write it out to the world
			WriteOv(outReal);
		}

		#endregion

		#region Threading

		public void Run()
		{
			var timer = new Timer(SleepTime.TotalMilliseconds);
			timer.Elapsed += Compute;
			timer.Start();
		}

		#endregion
	}
}