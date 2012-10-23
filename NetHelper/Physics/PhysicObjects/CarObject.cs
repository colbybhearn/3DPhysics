using System;
using JigLibX.Vehicles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Helper.Objects;

namespace Helper.Physics.PhysicsObjects
{
    public class CarObject : Gobject
    {
        private Car car;
        private Model wheel;

        public CarObject(int asset,
            Vector3 pos,
            Model model,Model wheels, bool FWDrive,
                       bool RWDrive,
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
            car = new Car(FWDrive, RWDrive, maxSteerAngle, steerRate,
                wheelSideFriction, wheelFwdFriction, wheelTravel, wheelRadius,
                wheelZOffset, wheelRestingFrac, wheelDampingFrac,
                wheelNumRays, driveTorque, gravity);

            this.Body = car.Chassis.Body;
            this.Skin= car.Chassis.Skin;
            Body.CollisionSkin = Skin;
            Body.ExternalData = this;
            this.wheel = wheels;
            CommonInit(pos, new Vector3(1, 1, 1), model, true, asset);
            SetCarMass(100.1f);

            actionManager.AddBinding((int)Actions.Acceleration, new Helper.Input.ActionBindingDelegate(SimulateAcceleration), 1);
            actionManager.AddBinding((int)Actions.Steering, new Helper.Input.ActionBindingDelegate(SimulateSteering), 1);
            actionManager.AddBinding((int)Actions.Handbrake, new Helper.Input.ActionBindingDelegate(SimulateHandbrake), 1);
        }

        public enum Actions
        {
            Acceleration,
            Steering,
            Handbrake
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
                        Matrix.CreateTranslation(wh.Pos + wh.Displacement * wh.LocalAxisUp) * car.Chassis.Body.Orientation * // oritentation of wheels
                        Matrix.CreateTranslation(car.Chassis.Body.Position);
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
            DrawWheel(car.Wheels[0], true, View, Projection);
            DrawWheel(car.Wheels[1], true, View, Projection);
            DrawWheel(car.Wheels[2], false, View, Projection);
            DrawWheel(car.Wheels[3], false, View, Projection);

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
        }

        public Car Car
        {
            get { return this.car; }
        }

        private void SetCarMass(float mass)
        {
            Body.Mass = mass;
            Vector3 min, max;
            car.Chassis.GetDims(out min, out max);
            Vector3 sides = max - min;

            float Ixx = (1.0f / 12.0f) * mass * (sides.Y * sides.Y + sides.Z * sides.Z);
            float Iyy = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Z * sides.Z);
            float Izz = (1.0f / 12.0f) * mass * (sides.X * sides.X + sides.Y * sides.Y);

            Matrix inertia = Matrix.Identity;
            inertia.M11 = Ixx; inertia.M22 = Iyy; inertia.M33 = Izz;
            car.Chassis.Body.BodyInertia = inertia;
            car.SetupDefaultWheels();
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
            car.Accelerate = p;
            actionManager.SetActionValues((int)Actions.Acceleration, new object[] {p});
        }

        public void SimulateSteering(object[] vals)
        {
            SetSteering((float)vals[0]);
        }
        public void SetSteering(float p)
        {
            car.Steer = p;
            actionManager.SetActionValues((int)Actions.Steering, new object[] { p });
        }

        public void SimulateHandbrake(object[] vals)
        {
            setHandbrake((float)vals[0]);
        }
        public void setHandbrake(float p)
        {
            car.HBrake = p;
            actionManager.SetActionValues((int)Actions.Handbrake, new object[] { p });
        }
        #endregion

        public override void SetNominalInput()
        {
            SetAcceleration(0);
            SetSteering(0);
            setHandbrake(0);
        }
    }
}
