﻿
using Microsoft.Xna.Framework;
using Helper.Physics;

namespace Helper.Camera.Cameras
{
    public class WatchCamera : BaseCamera
    {
        public WatchCamera()
        {

        }

        public override Matrix GetViewMatrix()
        {
            return RhsLevelViewMatrix;
        }

        public override Matrix GetProjectionMatrix()
        {
            return _projection;
        }

        public override void Update()
        {
            base.Update();
            Gobject gob = GetFirstGobject();
            if (gob == null) return;

            LookAtLocation(gob.BodyPosition());
            
        }
    }
}