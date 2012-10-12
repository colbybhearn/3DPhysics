using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Utility
{
    public class MultEnum
    {
        int uniqValue = 0;
        SortedList<string, SortedList<int, int>> enums = new SortedList<string, SortedList<int, int>>();

        public MultEnum()
        {
        }

        public void Add(Enum e)
        {
            int val = (int)Convert.ChangeType(e, e.GetTypeCode());
            string t = e.GetType().Name;
            if (!enums.ContainsKey(t))
                enums.Add(t, new SortedList<int, int>());
            SortedList<int, int> values = enums[t];
            values.Add(val, ++uniqValue);
        }

        public int Lookup(Enum e)
        {
            int val = (int)Convert.ChangeType(e, e.GetTypeCode());
            string t = e.GetType().Name;
            if (!enums.ContainsKey(t))
                return -1;
            if (!enums[t].ContainsKey(val))
                return -1;
            return enums[t][val];
        }

    }
}
