using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Collections
{
    public class ThreadQueue : Queue<object>
    {
        public ThreadQueue()
            : base ()
        {
        }
        
        public void EnQ(object o)
        {
            lock(this)
            {
                this.Enqueue(o);
            }
        }

        public object DeQ()
        {
            object o = null;
            lock (this)
            {
                o =this.Dequeue();
            }
            return o;
        }

    }
}
