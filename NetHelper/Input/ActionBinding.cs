using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helper.Input
{
    public class ActionBinding
    {
        public int ID;
        public ActionBindingDelegate Callback;
        public List<int> Indices;

        public ActionBinding(int id, ActionBindingDelegate cb, List<int> indicies)
        {
            ID = id;
            Callback = cb;
            Indices = indicies;
        }



    }
}
