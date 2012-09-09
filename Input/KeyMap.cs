using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Input
{
    /// <summary>
    /// Stores a set of keys, mapped to a set of possible bindings
    /// </summary>
    public class KeyMap
    {
        /*
         * Keymap Has
         *  - A game that it is tied to
         *  - A list of bindings
         *  
         * A binding has
         *  - an alias
         *  - a default setting (key + modifier)
         *  - a keyEvent type (Down, Up, PressDown, PressUp)
         */


        //Todo
        // add a form for showing, editing bindings.
        // add a class to read and write a keymap file, per user and per game name.
    }
}
