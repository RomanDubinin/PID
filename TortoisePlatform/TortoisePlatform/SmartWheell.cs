using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PidLibrary;

namespace TortoisePlatform
{
    class SmartWheell
    {
        public PIDRegulator Regulator { get; private set; }
        public int CurrentTics {get; set; }
        public int ExpectedTics { get; set; }
        public int TicsPerPeriod {get; set; }

        public SmartWheell(PIDRegulator regulator, int ticsPerPeriod)
        {
            Regulator = regulator;
            TicsPerPeriod = ticsPerPeriod;
            CurrentTics = 0;
            ExpectedTics = 0;
        }


    }
}
