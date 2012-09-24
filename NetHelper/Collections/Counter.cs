using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Helper.Collections
{
    class Tick
    {
        public long Time;
        public double Value;

        public Tick(long t, double v)
        {
            Time = t;
            Value = v;
        }
    }

    public class Counter
    {
        static SortedList<string, Counter> Counters = new SortedList<string, Counter>();
        static int MaxSize = 1000;
        static Stopwatch Watch = Stopwatch.StartNew();

        List<Tick> Ticks;
        int TicksStart = 0;
        int TicksEnd = 0;
        long MinTime;
        long MaxTime;
        double TotalValue;
        long TotalTime;

        int Count
        {
            get
            {
                return (TicksStart - TicksEnd + MaxSize) % MaxSize;
            }
        }

        private Counter()
        {
            MinTime = long.MaxValue;
            MaxTime = long.MinValue;

            Ticks = new List<Tick>(MaxSize);
        }

        private void AddTick(double value)
        {
            if(Double.IsInfinity(value))
                value = 0;
            if ((TicksEnd - TicksStart + MaxSize) % MaxSize == 1) // Queue is full
            {
                TotalValue -= Ticks[TicksEnd].Value;
                TotalTime -= Ticks[TicksEnd].Time;
                TicksEnd = (TicksEnd + 1) % MaxSize;

            }


            if (Ticks.Count == TicksStart)
                Ticks.Add(new Tick(Watch.ElapsedTicks, value));
            else
            {
                Ticks[TicksStart].Time = Watch.ElapsedTicks;
                Ticks[TicksStart].Value = value;
            }

            MaxTime = Ticks[TicksStart].Time;
            MinTime = Ticks[TicksEnd].Time;
            TotalValue += value;
            TotalTime += Ticks[TicksStart].Time;
            TicksStart = (TicksStart + 1) % MaxSize;
        }


        public static void AddTick(string alias, double value)
        {
            alias = alias.ToLower();
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                if (index == -1)
                {
                    Counters.Add(alias, new Counter());
                    index = Counters.IndexOfKey(alias);
                }

                Counters.Values[index].AddTick(value);
            }
        }

        public static void AddTick(string alias)
        {
            AddTick(alias, 1);
        }

        private static void GetTotals(string alias, out long time, out double value)
        {
            alias = alias.ToLower();
            time = 0;
            value = 0;
            lock(Counters)
            {
                int index = Counters.IndexOfKey(alias);
                if (index != -1)
                {
                    //long min = Counters.Values[index].MinTime;
                    time = Counters.Values[index].TotalTime;
                    value = Counters.Values[index].TotalValue;
                }
            }
        }
            

        private static void GetAverages(string alias, out double time, out double value)
        {
            alias = alias.ToLower();
            time = 0;
            value = 0;
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                if (index != -1)
                {
                    long t;
                    double v;
                    GetTotals(alias, out t, out v);
                    value = v / (double)Counters.Values[index].Count;
                    time = t / (double)Counters.Values[index].Count;
                }
            }
        }

        public static double GetAverageTime(string alias)
        {
            alias = alias.ToLower();
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                double ret = 0;
                if (index != -1)
                {
                    long t;
                    double v;
                    GetTotals(alias, out t, out v);
                    ret = t / (double)Counters.Values[index].Count;
                }
                return ret;
            }
        }

        public static double GetAverageValue(string alias)
        {
            alias = alias.ToLower();
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                double ret = 0;
                if (index != -1)
                {
                    long t;
                    double v;
                    GetTotals(alias, out t, out v);
                    ret = v / (double)Counters.Values[index].Count;
                }
                return ret;
            }
        }

        public static double GetAveragePerSecond(string alias)
        {
            alias = alias.ToLower();
            double v = GetAverageValue(alias);
            double ret = 0;
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                if (index != -1)
                {
                    long min = Counters.Values[index].MinTime;
                    long max = Counters.Values[index].MaxTime;
                    /*        Ticks Per Second             Ticks Per Second * Total
                     * ----------------------------       ---------------------------
                     *     Time Elapse * Average     ==     Time Elapsed * Average
                     *      -----------------
                     *            Total
                     */
                    ret = (Stopwatch.Frequency * Counters.Values[index].Count) / (double)((Watch.ElapsedTicks - min) * v);
                }
            }

            return ret;
        }

    }
}
