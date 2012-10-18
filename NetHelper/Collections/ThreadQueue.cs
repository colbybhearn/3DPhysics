using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Collections
{
    public class ThreadQueue<T> : Queue<T>
    {
        public ThreadQueue()
            : base ()
        {
        }

        public int myCount
        {
            get
            {
                int c = 0;
                lock (this)
                {
                    c= this.Count;
                }
                return c;
            }
        }

        public void EnQ(T o)
        {
            lock(this)
            {
                this.Enqueue(o);
            }
        }

        public T DeQ()
        {
            T o;
            lock (this)
            {
                o = this.Dequeue();
            }
            return o;
        }

    }
}
