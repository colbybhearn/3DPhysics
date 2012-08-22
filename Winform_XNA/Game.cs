using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX;
using JigLibX.Physics;
using JigLibX.Collision;
using System.Collections.Generic;
using JigLibX.Geometry;
using JigLibX.Math;
namespace Winform_XNA
{
    public class Game 
    {
        /* Class should delegate most Game processing such as:
         * content loading
         * handling input
         * updating physics
         * drawing
         */

        Vector2 GRAVITY = new Vector2(0, 800);

        public static Game Instance { get; private set; }

        public Game()
        {
            Instance = this;
        }



    }
}
