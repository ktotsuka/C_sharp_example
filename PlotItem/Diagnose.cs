using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PlotItemSpace
{
    public class Diagnose : Stopwatch
    {
        static private Stopwatch myStopWatch = new Stopwatch();

        static public void StartTimer()
        {
            myStopWatch.Reset();
            myStopWatch.Start();
        }

        static public void StopTimer()
        {
            float elapsed_time;

            elapsed_time = (float)myStopWatch.ElapsedTicks / (float)Stopwatch.Frequency;
            Console.WriteLine("Elapsed time = " + elapsed_time + " s");
            myStopWatch.Stop();
        }
    }
}
