using System;

namespace PidLibrary
{
	public class PIDRegulator
	{
		private double LastPprocessVal;
		private double ErrorSum;

		public double ProportionalCoef { get; set; }
		public double IntegralCoef { get; set; }
		public double DifferentialCoef { get; set; }
		public double PprocessValMin { get; set; }
		public double PprocessValMax { get;set; }
		public double OutMin { get; set; }
		public double OutMax { get; set; }


		public PIDRegulator(double proportionalCoef, double integralCoef, double differentialCoef,
			double inputMax, double inputMin, double outputMax, double outputMin)
		{
			ProportionalCoef = proportionalCoef;
			IntegralCoef = integralCoef;
			DifferentialCoef = differentialCoef;
			PprocessValMax = inputMax;
			PprocessValMin = inputMin;
			OutMax = outputMax;
			OutMin = outputMin;
		}

		#region Public Methods

		public void Reset()
		{
			ErrorSum = 0.0f;
		}

		#endregion

		private double ScaleValue(double value, double valuemin, double valuemax, double scalemin, double scalemax)
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

		public double Compute(double processVal, double setPoint, TimeSpan deltaTime)
		{
			//We need to scale the pv to +/- 100%, but first clamp it
			processVal = Clamp(processVal, PprocessValMin, PprocessValMax);
			processVal = ScaleValue(processVal, PprocessValMin, PprocessValMax, -1.0f, 1.0f);

			//We also need to scale the setpoint
			setPoint = Clamp(setPoint, PprocessValMin, PprocessValMax);
			setPoint = ScaleValue(setPoint, PprocessValMin, PprocessValMax, -1.0f, 1.0f);

			//Now the error is in percent...
			var err = setPoint - processVal;

			var pTerm = err*ProportionalCoef;
			double iTerm = 0.0f;
			double dTerm = 0.0f;

			double partialSum = 0.0f;

			var dT = deltaTime.TotalSeconds;

			partialSum = ErrorSum + dT*err;
			iTerm = IntegralCoef*partialSum;


			if (dT != 0.0f)
				dTerm = DifferentialCoef*(processVal - LastPprocessVal)/dT;

			ErrorSum = partialSum;
			LastPprocessVal = processVal;

			//Now we have to scale the output value to match the requested scale
			var outReal = pTerm + iTerm + dTerm;

			outReal = Clamp(outReal, -1.0f, 1.0f);
			outReal = ScaleValue(outReal, -1.0f, 1.0f, OutMin, OutMax);

			return outReal;
		}
	}
}