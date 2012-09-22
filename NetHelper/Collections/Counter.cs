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

    public static class Counter
    {
        static SortedList<string, ThreadQueue<Tick>> Counters = new SortedList<string, ThreadQueue<Tick>>();
        static int MaxSize = 1000;
        static Stopwatch Watch = Stopwatch.StartNew();


        public static void AddTick(string alias, double value)
        {
            alias = alias.ToLower();
            lock (Counters)
            {
                int index = Counters.IndexOfKey(alias);
                if (index == -1)
                {
                    Counters.Add(alias, new ThreadQueue<Tick>());
                    index = Counters.IndexOfKey(alias);
                }

                Counters.Values[index].EnQ(new Tick(Watch.ElapsedTicks, value));
                if (Counters.Values[index].Count > MaxSize)
                    Counters.Values[index].DeQ();
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
                    long min = Counters.Values[index].Min(t => t.Time);
                    time = Counters.Values[index].Sum(t => (t.Time - min));
                    value = Counters.Values[index].Sum(t => t.Value);
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
                    long min = Counters.Values[index].Min(a => a.Time);
                    long max = Counters.Values[index].Max(a => a.Time);
                    /*        Ticks Per Second             Ticks Per Second * Total
                     * ----------------------------       ---------------------------
                     *     Time Elapse * Average     ==     Time Elapsed * Average
                     *      -----------------
                     *            Total
                     */
                    ret = (Stopwatch.Frequency * Counters.Values[index].Count) / (double)((max - min) * v);
                }
            }

            return ret;
        }

    }
}
