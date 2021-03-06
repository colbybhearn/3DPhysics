﻿#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using JigLibX.Collision;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Math;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Utils;
#endregion

namespace Physics.PhysicsObjects
{
    public class HeightmapObject : Gobject
    {
        public HeightmapObject(Model model,Vector2 shift, Vector3 position)
            : base()
        {
            Body = new Body(); // just a dummy. The PhysicObject uses its position to get the draw pos
            Skin = new CollisionSkin(null);

            HeightMapInfo heightMapInfo = model.Tag as HeightMapInfo;
			Array2D field = new Array2D(heightMapInfo.heights.GetLength(0), heightMapInfo.heights.GetLength(1));

			for (int x = 0; x < heightMapInfo.heights.GetLength(0); x++)
            {
				for (int z = 0; z < heightMapInfo.heights.GetLength(1); z++)
                {
                    field.SetAt(x,z,heightMapInfo.heights[x,z]);  
                }
            }

            // move the body. The body (because its not connected to the collision
            // skin) is just a dummy. But the base class shoudl know where to
            // draw the model.
            Body.MoveTo(new Vector3(shift.X,0,shift.Y), Matrix.Identity);

            Skin.AddPrimitive(new Heightmap(field, shift.X, shift.Y, heightMapInfo.terrainScale, heightMapInfo.terrainScale), new MaterialProperties(0.7f, 0.7f, 0.6f));

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.AddCollisionSkin(Skin);
            CommonInit(position, new Vector3(1,1,1), model, false, "");
        }
        /*
        public override void ApplyEffects(BasicEffect effect)
        {
            effect.PreferPerPixelLighting = true;
        }*/

    }
}
