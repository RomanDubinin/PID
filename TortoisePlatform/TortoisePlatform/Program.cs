using PidLibrary;
using System;
using System.IO.Ports;
using System.Timers;
using System.Threading.Tasks;

namespace TortoisePlatform
{
	public class Program
	{
		private static SerialPort Com;
		private static Timer SendTimer;
		private static Timer ReadTimer;
        private static int ExpectedTics = 200;

        private static int MaxInput = 1000;
        private static int MinInput = -1000;

        private static int MaxOutput = 255;
        private static int MinOutput = -255;

        private static TimeSpan Period = TimeSpan.FromMilliseconds(100);

        private static int[] Tics;
        private static PIDRegulator[] Regulators;

        private static void WriteCommand(int driveId, int mills, int speed)
        {
            var command = String.Format("M 0 {0} {1} {2}", driveId, mills, speed);
            Com.WriteLine(command + String.Format(" {0}", command.Length - 4));
        }

        private static string ReadCommand()
        {
            while (Com.BytesToRead < 20)
            { }
            return Com.ReadLine();
        }

		public static void Main(string[] args)
		{
            Tics = new int[4];
            Regulators = new PIDRegulator[4];
            for (var i = 0; i < 4; i++)
            {
                Regulators[i] = new PIDRegulator(2, 1, 0.3, MaxInput, MinInput, MaxOutput, MinOutput);
            }

            Com = new SerialPort("COM29", 115200);
			Com.Open();
            Com.WriteLine("SE 100");

            //SendTimer = new Timer(2000);
            //SendTimer.Elapsed += Send;
            //SendTimer.Start();

            //ReadTimer = new Timer(5);
            //ReadTimer.Elapsed += Read;
            //ReadTimer.Start();
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
                    if (id % 2 == 0)
                        currentTics *= -1;

                    Tics[id] += currentTics;
                    if (command[0].CompareTo("DR") == 0)
                    {
                        Console.WriteLine("id: {0}, Time: {1}, tics: {2}", id, command[2], Tics[id]);
                        var t = Tics[id];
                        Task.Factory.StartNew(() => DoMath(id, t));
                        Tics[id] = 0;
                    }
                }
			Console.ReadLine();
			Com.Close();
		}

        private static void DoMath(int id, int actualTics)
        {
            var speed = Regulators[id].Compute(actualTics, ExpectedTics, Period);

            Console.WriteLine("{0}: {1}", id, speed);

            //todo это надо менять
            if (id % 2 == 0)
                speed *= -1;

            WriteCommand(id, Period.Milliseconds, (int)speed);
        }

        //public static void Send(object sender, ElapsedEventArgs elapsedEventArgs)
        //{
        //    WriteCommand(0, 1000, 100);
        //}

        //public static void Read(object sender, ElapsedEventArgs elapsedEventArgs)
        //{
        //    if(Com.BytesToRead > 20)
        //        Console.WriteLine(Com.ReadLine());
        //    //Console.ReadKey();
        //    //Console.WriteLine("Read");
        //}
	}
}
