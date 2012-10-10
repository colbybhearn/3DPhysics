using System;
using JigLibX.Vehicles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Geometry;
using JigLibX.Collision;

namespace Helper.Physics.PhysicsObjects
{
    public class RoverObject : Gobject
    {
        private Rover rover;
        private Model wheel;
        private Model Radar;
        private Model Laser;
        public bool hasRadar = false;
        public bool hasLaser = false;
        public float energy = 100.0f;        

        public RoverObject(string asset,
            Vector3 pos,
            Model model, 
            Model wheels,
            Model radar,
            Model laser,
            float maxSteerAngle,
            float steerRate,
            float wheelSideFriction,
            float wheelFwdFriction,
            float wheelTravel,
            float wheelRadius,
            float wheelZOffset,
            float wheelRestingFrac,
            float wheelDampingFrac,
            int wheelNumRays,
            float driveTorque,
            float gravity)
            : base()
        {
            Radar = radar;
            Laser = laser;
            rover = new Rover(true, true, maxSteerAngle, steerRate,
                wheelSideFriction, wheelFwdFriction, wheelTravel, wheelRadius,
                wheelZOffset, wheelRestingFrac, wheelDampingFrac,
                wheelNumRays, driveTorque, gravity);

            this.Body = rover.Chassis.Body;
            this.Skin = rover.Chassis.Skin;
            Body.CollisionSkin = Skin;
            Body.ExternalData = this;
            this.wheel = wheels;
            CommonInit(pos, new Vector3(1, 1, 1), model, true, asset);
            SetCarMass(400.1f);

            actionManager.AddBinding((int)Actions.Acceleration, new Helper.Input.ActionBindingDelegate(SimulateAcceleration), 1);
            actionManager.AddBinding((int)Actions.Steering, new Helper.Input.ActionBindingDelegate(SimulateSteering), 1);
            actionManager.AddBinding((int)Actions.Laser, new Helper.Input.ActionBindingDelegate(SimulateLaser), 1);
        }

        public enum Actions
        {
            Acceleration,
            Steering,
            Laser
        }

        public override void FinalizeBody()
        {
            try
            {
                //Vector3 com = SetMass(2.0f);
                //SetMass(2.0f);
                //Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
                Body.MoveTo(Position, Matrix.Identity);
                Body.EnableBody(); // adds to CurrentPhysicsSystem
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
        }

        private void DrawWheel(Wheel wh, bool rotated, Matrix View, Matrix Projection)
        {
            float steer = wh.SteerAngle;

            Matrix rot;
            if (rotated) rot = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
            else rot = Matrix.Identity;

            Matrix world = rot * Matrix.CreateRotationZ(MathHelper.ToRadians(-wh.AxisAngle)) * // rotate the wheels
                        Matrix.CreateRotationY(MathHelper.ToRadians(steer)) *
                        Matrix.CreateTranslation(wh.Pos + wh.Displacement * wh.LocalAxisUp) * rover.Chassis.Body.Orientation * // oritentation of wheels
                        Matrix.CreateTranslation(rover.Chassis.Body.Position);
            foreach (ModelMesh mesh in wheel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world; // translation

                    effect.View = View;
                    effect.Projection = Projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        public override void Draw(ref Matrix View, ref Matrix Projection)
        {
            DrawWheel(rover.Wheels[0], true, View, Projection);
            DrawWheel(rover.Wheels[1], true, View, Projection);
            DrawWheel(rover.Wheels[2], true, View, Projection);
            DrawWheel(rover.Wheels[3], false, View, Projection);
            DrawWheel(rover.Wheels[4], false, View, Projection);
            DrawWheel(rover.Wheels[5], false, View, Projection);

            if (Model == null)
                return;
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    if (Selected)
                        effect.AmbientLightColor = Color.Red.ToVector3();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }

            if (hasRadar)
            {
                foreach (ModelMesh mesh in Radar.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.AmbientLightColor = Color.Gray.ToVector3();
                        effect.World = GetRadarWorldMatrix();
                        effect.View = View;
                        effect.Projection = Projection;
                    }
                    mesh.Draw();
                }
            }

            if (hasLaser)
            {
                Matrix[] ltransforms = new Matrix[Laser.Bones.Count];
                Laser.CopyAbsoluteBoneTransformsTo(ltransforms);
                foreach (ModelMesh mesh in Laser.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.AmbientLightColor = Color.Gold.ToVector3();
                        effect.World = ltransforms[mesh.ParentBone.Index] * GetLaserWorldMatrix();
                        effect.View = View;
                        effect.Projection = Projection;
                    }
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// only used for the model
        /// </summary>
        /// <returns></returns>
        public Matrix GetLaserWorldMatrix()
        {
            //return Matrix.CreateScale(Scale*.3f) * Body.Orientation * Matrix.CreateTranslation(Body.Position + new Vector3(1, .2f, 0));  WRONG
            return Matrix.CreateScale(Scale * .3f) * Matrix.CreateTranslation(new Vector3(1, .2f, -.5f)) * rover.Chassis.Body.Orientation * Matrix.CreateTranslation(rover.Chassis.Body.Position);

        }

        /// <summary>
        /// only used for the model
        /// </summary>
        /// <returns></returns>
        public Matrix GetRadarWorldMatrix()
        {
            
            return Matrix.CreateScale(Scale * .4f) * Matrix.CreateTranslation(new Vector3(-1, .4f, .5f)) * rover.Chassis.Body.Orientation * Matrix.CreateTranslation(rover.Chassis.Body.Position);
        }
        
        public Rover Rover
        {
            get { return this.rover; }
        }

        private void SetCarMass(float mass)
        {
            Body.Mass = mass;
            Vector3 min, max;
            rover.Chassis.GetDims(out min, out max);
            Vector3 sides = max - min;

            float Ixx = (1.0f / 12.0f) * mass * (sides.Y * sides.Y + sides.Z * sides.Z);
            float Iyy = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Z * sides.Z);
            float Izz = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Y * sides.Y);

            Matrix inertia = Matrix.Identity;
            inertia.M11 = Ixx; inertia.M22 = Iyy; inertia.M33 = Izz;
            rover.Chassis.Body.BodyInertia = inertia;
            rover.SetupDefaultWheels();
        }

        public override Vector3 GetPositionAbove()
        {
            return Body.Position + Vector3.UnitY * 4;
        }

        #region Input
        public void SimulateAcceleration(object[] vals)
        {
            SetAcceleration((float)vals[0]);
        }
        public void SetAcceleration(float p)
        {
            rover.Accelerate = p;
            actionManager.SetActionValues((int)Actions.Acceleration, new object[] { p });
        }

        public void SimulateSteering(object[] vals)
        {
            SetSteering((float)vals[0]);
        }
        public void SetSteering(float p)
        {
            rover.Steer = p;
            actionManager.SetActionValues((int)Actions.Steering, new object[] { p });
        }

        public void SimulateLaser(object[] vals)
        {
            setLaser((float)vals[0]);
        }
        public void setLaser(float p)
        {
            // Anything physical to do here?

            actionManager.SetActionValues((int)Actions.Laser, new object[] { p });
        }
        #endregion

        public override void SetNominalInput()
        {
            SetAcceleration(0);
            SetSteering(0);
            setLaser(0);
        }

        public void SetRadar(bool hasit)
        {
            hasAttributeChanged = hasRadar != hasit;
            hasRadar = hasit;
        }

        public void SetLaser(bool hasit)
        {
            hasAttributeChanged = hasLaser != hasit;
            hasLaser = hasit;
        }

        /// <summary>
        /// Used by the server to gather up info to distribute to all clients
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="iv"></param>
        /// <param name="fv"></param>
        public override void GetObjectAttributes(out bool[] bv, out int[] iv, out float[] fv)
        {
            bv = new bool[] {hasRadar, hasLaser};
            iv = null;
            fv = null;
        }

        public override void SetObjectAttributes(bool[] bv, int[] iv, float[] fv)
        {
            int index = -1;
            if (bv != null && bv.Length>=2)
            {
                hasRadar = bv[++index];
                hasLaser = bv[++index];
            }
            index = -1;
        }
    }

    public class RoverChassis : Chassis
    {
        public RoverChassis(RollingVehicle veh)
             : base(veh)
        {
        }

        public override void SetDims(Vector3 min, Vector3 max)
        {
            dimsMin = min;
            dimsMax = max;
            Vector3 sides = max - min;
            Box box1 = new Box(min, Matrix.Identity, sides);
            
            collisionSkin.RemoveAllPrimitives();
            collisionSkin.AddPrimitive(box1, new MaterialProperties(0.3f, 0.5f, 0.3f));

            body.Vehicle.SetupDefaultWheels();
        }
    }
}
