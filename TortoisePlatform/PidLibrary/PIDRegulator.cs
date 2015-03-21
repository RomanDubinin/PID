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


		
		public void Reset()
		{
			ErrorSum = 0.0f;
		}
		
		public double Compute(double processVal, double setPoint, TimeSpan deltaTime)
		{
			processVal = Clamp(processVal, PprocessValMin, PprocessValMax);
			processVal = ScaleValue(processVal, PprocessValMin, PprocessValMax, -1.0, 1.0);

			setPoint = Clamp(setPoint, PprocessValMin, PprocessValMax);
			setPoint = ScaleValue(setPoint, PprocessValMin, PprocessValMax, -1.0, 1.0);

			var err = setPoint - processVal;

			var pTerm = err*ProportionalCoef;

			ErrorSum += deltaTime.TotalSeconds * err;
			var iTerm = IntegralCoef * ErrorSum;

			var dTerm = 0.0;
			if (Math.Abs(deltaTime.TotalSeconds) > 1e-10)
				dTerm = DifferentialCoef * (processVal - LastPprocessVal) / deltaTime.TotalSeconds;

			LastPprocessVal = processVal;

			var outReal = pTerm + iTerm + dTerm;

			outReal = Clamp(outReal, -1.0, 1.0);
			outReal = ScaleValue(outReal, -1.0, 1.0, OutMin, OutMax);

			return outReal;
		}
	}
}