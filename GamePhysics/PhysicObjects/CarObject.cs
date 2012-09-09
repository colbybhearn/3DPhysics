using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Vehicles;
using JigLibX.Collision;

namespace Physics.PhysicsObjects
{
    public class CarObject : Gobject
    {

        private Car car;
        private Model wheel;

        public CarObject(Vector3 pos,
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
            CommonInit(pos, new Vector3(1, 1, 1), model, true);
            SetCarMass(100.1f);
        }

        public override void FinalizeBody()
        {
            try
            {
                //Vector3 com = SetMass(2.0f);
                //Skin.ApplyLocalTransform(new JigLibX.Math.Transform(-com, Matrix.Identity));
                Body.MoveTo(Position, Matrix.Identity);
                Body.EnableBody(); // adds to CurrentPhysicsSystem
            }
            catch (Exception E)
            {
            }
        }

        private void DrawWheel(Wheel wh, bool rotated, Matrix View, Matrix Projection)
        {
            foreach (ModelMesh mesh in wheel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    float steer = wh.SteerAngle;

                    Matrix rot;
                    if (rotated) rot = Matrix.CreateRotationY(MathHelper.ToRadians(180.0f));
                    else rot = Matrix.Identity;

                    effect.World = rot * Matrix.CreateRotationZ(MathHelper.ToRadians(-wh.AxisAngle)) * // rotate the wheels
                        Matrix.CreateRotationY(MathHelper.ToRadians(steer)) *
                        Matrix.CreateTranslation(wh.Pos + wh.Displacement * wh.LocalAxisUp) * car.Chassis.Body.Orientation * // oritentation of wheels
                        Matrix.CreateTranslation(car.Chassis.Body.Position); // translation

                    effect.View = View;
                    effect.Projection = Projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        public override void Draw(Matrix View, Matrix Projection)
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

        public void SetAcceleration(float p)
        {
            car.Accelerate = p;
        }

        public void SetSteering(float p)
        {
            car.Steer = p;
        }

        public void setHandbrake(float p)
        {
            car.HBrake = p;
        }
    }
}
