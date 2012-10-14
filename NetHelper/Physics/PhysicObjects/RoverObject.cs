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
        #region Properties and Fields
        private Rover rover;
        private Model wheel;
        private Model Radar;
        private Model Laser;
        private Model Arm;
        private Model Cam;
        private Model Pole;
        public bool hasRadar = false;
        public bool hasLaser = false;
        public float Energy = 100.0f;   // Should be reduced by server and value passed to client        
        public Rover Rover
        {
            get { return this.rover; }
        }

        public float rotCamYaw = 0.0f;
        public float rotCamPitch = 0.0f;

        Vector3 CamArmAPointOfRotationFromRover = new Vector3(1, 1, 0);
        private Matrix CamArmARotation
        {
            get
            {
                return Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(rotCamYaw, 0, 0));
            }
        }
        Vector3 CamArmAOriginCorrection = new Vector3(0, 0, -.1f);

        Vector3 CamArmBPointOfRotationFromArmA = new Vector3(.07f, .07f, -.19f);
        private Matrix CamArmBRotation
        {
            get
            {
                return Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, rotCamPitch, -(float)Math.PI / 2.0f));
            }
        }
        Vector3 CamArmBOriginCorrection = new Vector3(0, 0, -.1f);

        Vector3 CamBoxPointOfRotationFromArmB = new Vector3(0, 0, 0);
        Matrix CamBoxRotation = Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, 0, 0));
        Matrix CamBoxOrientCorrection = Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, (float)Math.PI, -(float)Math.PI / 2.0f));
        Vector3 CamBoxOriginCorrection = new Vector3(0, 0, .13f);

        Vector3 CamPoleLocation = new Vector3(1, 0, 0);
        Vector3 CamPoleScaleCorrection = new Vector3(.75f, .5f, .75f);

        float radarRotation = 0;
        DateTime lastDraw;
        #endregion

        // These must be actions like dropRadar, not properties like hasRadar.
        public enum Actions
        {
            Acceleration,
            Steering,
            DropLaser,
            DropRadar,
            ShootLaser,
            RotatedCamX,
            RotatedCamY
        }

        public RoverObject(int asset,
            Vector3 pos,
            Model model, 
            Model wheels,
            Model radar,
            Model laser,
            Model rotArm,
            Model cam,
            Model pole,
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
            Arm = rotArm;
            Cam = cam;
            Pole = pole;
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
            SetRoverMass(400.1f);

            // allow different types of bindings. bool, int, float
            actionManager.AddBinding((int)Actions.Acceleration, new Helper.Input.ActionBindingDelegate(SimulateAcceleration), 1);
            actionManager.AddBinding((int)Actions.Steering, new Helper.Input.ActionBindingDelegate(SimulateSteering), 1);
            actionManager.AddBinding((int)Actions.ShootLaser, new Helper.Input.ActionBindingDelegate(SimulateShootLaser), 1);
            actionManager.AddBinding((int)Actions.DropLaser, new Helper.Input.ActionBindingDelegate(SimulateDropLaser), 1);
            actionManager.AddBinding((int)Actions.DropRadar, new Helper.Input.ActionBindingDelegate(SimulateDropRadar), 1);
            actionManager.AddBinding((int)Actions.RotatedCamX, new Helper.Input.ActionBindingDelegate(SimulateDropLaser), 1);
            actionManager.AddBinding((int)Actions.RotatedCamY, new Helper.Input.ActionBindingDelegate(SimulateDropRadar), 1);
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

        #region Draw
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
                DrawRadar(View, Projection);

            if (hasLaser)
                DrawLaser(View, Projection);

            DrawCameraRig(View, Projection);
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
        private void DrawRadar(Matrix View, Matrix Projection)
        {
            float rotationsPerSecond = .3f;
            float stepsPerRotation = 20;
            float radiansPerStep = 2.0f * (float)Math.PI / stepsPerRotation;

            //float rotPosition = DateTime.Now.Millisecond * 
            Vector3 offsetPos = new Vector3(-.9f, .7f, .5f);
            DateTime now = DateTime.Now;
            float diffSeconds = (float)(now-lastDraw).TotalSeconds;
            radarRotation += diffSeconds * rotationsPerSecond * stepsPerRotation * radiansPerStep;
            radarRotation %= (2.0f*(float)Math.PI);

            Matrix world =  Matrix.CreateScale(Scale * 1.4f) *
                            Matrix.CreateRotationY(radarRotation) * // rotate the dish
                            Matrix.CreateTranslation(offsetPos) * 
                            rover.Chassis.Body.Orientation * 
                            Matrix.CreateTranslation(rover.Chassis.Body.Position);

            Matrix[] transforms = new Matrix[Radar.Bones.Count];
            Radar.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Radar.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.DarkGray.ToVector3();
                    effect.World =  transforms[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
            lastDraw = now;
        }
        private void DrawLaser(Matrix View, Matrix Projection)
        {
            /* Old Location
            Vector3 offsetPos = new Vector3(1, .2f, -1.0f);
            
            Matrix world = Matrix.CreateScale(Scale * .2f) *
                            Matrix.CreateTranslation(offsetPos) *
                            rover.Chassis.Body.Orientation *
                            Matrix.CreateTranslation(rover.Chassis.Body.Position);*/

            // New Location (On Top Ya Head)
            Vector3 offsetFromCam = new Vector3(-.47f, -.1f, .06f);

            Matrix world = 
                    Matrix.CreateScale(new Vector3(1,1,2))*
                    Matrix.CreateScale(Scale * .05f) *
                    // Laser
                    Matrix.CreateTranslation(offsetFromCam) * //
                    // Cam
                    Matrix.CreateTranslation(CamBoxOriginCorrection) * //
                    CamBoxRotation *
                    CamBoxOrientCorrection *
                    Matrix.CreateTranslation(CamBoxPointOfRotationFromArmB) * // move to the point of rotation for the arm (relative to the parent)

                    // Arm B
                    Matrix.CreateTranslation(CamArmBOriginCorrection) * //
                    CamArmBRotation *                                            // (Step 7)
                    Matrix.CreateTranslation(CamArmBPointOfRotationFromArmA) * // move to the point of rotation for the arm (relative to the parent)
                // Arm A
                    Matrix.CreateTranslation(CamArmAOriginCorrection) * //
                    CamArmARotation *
                    Matrix.CreateTranslation(CamArmAPointOfRotationFromRover) * // move to the point of rotation for the arm
                    rover.Chassis.Body.Orientation * // orient with the rover
                    Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover (Step 1)

            
            Matrix[] ltransforms = new Matrix[Laser.Bones.Count];
            Laser.CopyAbsoluteBoneTransformsTo(ltransforms);
            foreach (ModelMesh mesh in Laser.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.Gold.ToVector3();
                    effect.World = ltransforms[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
        }
        private void DrawCameraRig(Matrix View, Matrix Projection)
        {
            //Rover Chasis -> Arm A -> Arm B / Camera

            #region Pole
            Matrix world = //Matrix.CreateScale(PoleScaleCorrection) * // scale it in addition to the scale of this entire gobject
                            Matrix.CreateScale(Scale * CamPoleScaleCorrection) * // scale it in addition to the scale of this entire gobject
                            Matrix.CreateTranslation(CamPoleLocation) * // move to the location of the pole
                            rover.Chassis.Body.Orientation * // orient with the rover
                            Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover

            Matrix[] modtrans = new Matrix[Pole.Bones.Count];
            Pole.CopyAbsoluteBoneTransformsTo(modtrans);
            foreach (ModelMesh mesh in Pole.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.World = modtrans[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
            #endregion

            #region Rotation Arm A
            world =  Matrix.CreateScale(Scale * 1.4f) *
                            Matrix.CreateTranslation(CamArmAOriginCorrection) * //
                            CamArmARotation *
                            Matrix.CreateTranslation(CamArmAPointOfRotationFromRover) * // move to the point of rotation for the arm
                            rover.Chassis.Body.Orientation * // orient with the rover
                            Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover

            modtrans = new Matrix[Arm.Bones.Count];
            Arm.CopyAbsoluteBoneTransformsTo(modtrans);
            foreach (ModelMesh mesh in Arm.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.World = modtrans[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
            #endregion

            #region Rotation Arm B
            world = Matrix.CreateScale(Scale * 1.4f) *

                    Matrix.CreateTranslation(CamArmBOriginCorrection) * //
                    CamArmBRotation *
                    Matrix.CreateTranslation(CamArmBPointOfRotationFromArmA) * // move to the point of rotation for the arm (relative to the parent)

                    Matrix.CreateTranslation(CamArmAOriginCorrection) * //
                    CamArmARotation *
                    Matrix.CreateTranslation(CamArmAPointOfRotationFromRover) * // move to the point of rotation for the arm
                    rover.Chassis.Body.Orientation * // orient with the rover
                    Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover (Step 1)

            foreach (ModelMesh mesh in Arm.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.World = modtrans[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
            #endregion

            #region Camera

            world = Matrix.CreateScale(Scale * 1.4f) *

                    // Cam
                    Matrix.CreateTranslation(CamBoxOriginCorrection) * //
                    CamBoxRotation *
                    CamBoxOrientCorrection *
                    Matrix.CreateTranslation(CamBoxPointOfRotationFromArmB) * // move to the point of rotation for the arm (relative to the parent)
                    
                    // Arm B
                    Matrix.CreateTranslation(CamArmBOriginCorrection) * //
                    CamArmBRotation *                                            // (Step 7)
                    Matrix.CreateTranslation(CamArmBPointOfRotationFromArmA) * // move to the point of rotation for the arm (relative to the parent)
                    // Arm A
                    Matrix.CreateTranslation(CamArmAOriginCorrection) * //
                    CamArmARotation *
                    Matrix.CreateTranslation(CamArmAPointOfRotationFromRover) * // move to the point of rotation for the arm
                    rover.Chassis.Body.Orientation * // orient with the rover
                    Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover (Step 1)

            modtrans = new Matrix[Cam.Bones.Count];
            Cam.CopyAbsoluteBoneTransformsTo(modtrans);
            foreach (ModelMesh mesh in Cam.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.AmbientLightColor = Color.Gray.ToVector3();
                    effect.World = modtrans[mesh.ParentBone.Index] * world;
                    effect.View = View;
                    effect.Projection = Projection;
                }
                mesh.Draw();
            }
            #endregion
        }
        #endregion

        public Matrix GetRoverCamWorldMatrix()
        {
            return Matrix.CreateScale(Scale * 1.4f) *
                    // Cam                    
                    Matrix.CreateTranslation(CamBoxOriginCorrection) * //
                    CamBoxRotation *
                    CamBoxOrientCorrection *
                    Matrix.CreateTranslation(CamBoxPointOfRotationFromArmB) * // move to the point of rotation for the arm (relative to the parent)
                    // Arm B
                    Matrix.CreateTranslation(CamArmBOriginCorrection) * //
                    CamArmBRotation *                                            // (Step 7)
                    Matrix.CreateTranslation(CamArmBPointOfRotationFromArmA) * // move to the point of rotation for the arm (relative to the parent)
                    // Arm A
                    Matrix.CreateTranslation(CamArmAOriginCorrection) * //
                    CamArmARotation *
                    Matrix.CreateTranslation(CamArmAPointOfRotationFromRover) * // move to the point of rotation for the arm
                    rover.Chassis.Body.Orientation * // orient with the rover
                    Matrix.CreateTranslation(rover.Chassis.Body.Position); // move to the rover (Step 1)
        }
        public Vector3 GetCamPosition()
        {
            Vector3 CamLensRelativeToBoxOrigin = new Vector3(-.1f, 0, .3f);
            return Vector3.Transform(CamLensRelativeToBoxOrigin, GetRoverCamWorldMatrix());
        }

        public Matrix GetCameraOrientation()
        {
            return //Matrix.CreateScale(Scale * 1.4f) *
                // Cam
                Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(0, 0, (float)Math.PI / 2.0f)) *
                    CamBoxRotation *
                // Arm B
                    CamArmBRotation *                                            // (Step 7)
                // Arm A
                    CamArmARotation *
                    rover.Chassis.Body.Orientation; // orient with the rover
        }

        public Vector3 GetCameraDirection()
        {
            return GetRoverCamWorldMatrix().Backward;
        }
        public Vector3 GetCameraUp()
        {
            return GetRoverCamWorldMatrix().Down;
        }

        private void SetRoverMass(float mass)
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
        public override void SetNominalInput()
        {
            SetAcceleration(0);
            SetSteering(0);
            SetShootLaser(0);
        }

        public void SimulateAcceleration(object[] vals)
        {
            SetAcceleration((float)vals[0]);
        }
        public void SimulateSteering(object[] vals)
        {
            SetSteering((float)vals[0]);
        }
        public void SimulateShootLaser(object[] vals)
        {
            SetShootLaser((float)vals[0]);
        }
        public void SimulateDropLaser(object[] vals)
        {
            // we need more generic ActionValues.
            // different types, like ObjectAttributes so that this next procedure is cleaner
            if ((float)vals[0] > .5f)
                SetLaser(false);
        }
        public void SimulateDropRadar(object[] vals)
        {
            if ((float)vals[0] > .5f)
                SetRadar(false);
        }
        public void SimulateRotateCamX(object[] vals)
        {
            AdjustCamYaw((float)vals[0]);
        }
        public void SimulateRotateCamY(object[] vals)
        {
            AdjustCamPitch((float)vals[0]);
        }
        
        public void SetAcceleration(float p)
        {
            rover.Accelerate = p;
            actionManager.SetActionValues((int)Actions.Acceleration, new object[] { p });
        }
        public void SetSteering(float p)
        {
            rover.Steer = p;
            actionManager.SetActionValues((int)Actions.Steering, new object[] { p });
        }
        public void SetShootLaser(float p)
        {
            actionManager.SetActionValues((int)Actions.DropLaser, new object[] { p });
        }
        public void DropRadar()
        {
            actionManager.SetActionValues((int)Actions.DropRadar, new object[] { 1 });
        }
        public void DropLaser()
        {
            actionManager.SetActionValues((int)Actions.DropLaser, new object[] { 1 });
        }
        public void AdjustCamPitch(float v)
        {
            rotCamPitch += v;
            rotCamPitch = MathHelper.Clamp(rotCamPitch, -1.5f, 1.5f);
            actionManager.SetActionValues((int)Actions.RotatedCamY, new object[] { v });
        }
        public void AdjustCamYaw(float v)
        {
            rotCamYaw += v;
            actionManager.SetActionValues((int)Actions.RotatedCamX, new object[] { v });
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
        #endregion

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

        /// <summary>
        /// Used by the client to apply authoritative attributes described by the server
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="iv"></param>
        /// <param name="fv"></param>
        public override void SetObjectAttributes(bool[] bv, int[] iv, float[] fv)
        {
            // user presses forward and coasts into a radar
            // server detects and calls SetRadar(true)
            // server now hasAttributeChanged = true;
            // during PostIntegrate, server will send objectattribute update to client with radar=true;
            // client receives and calls SetObjectAttributes.
            // if hasRadar is just set to true, the next ObjectActionPacket sent to the server says radar=false. // it should say dropRadar = false;


            int index = -1;
            if (bv != null && bv.Length>=2)
            {
                SetRadar(bv[++index]);
                SetLaser(bv[++index]);
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
