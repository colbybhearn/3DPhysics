using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using JigLibX.Collision;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;
using Helper.Physics.PhysicsObjects;
using Helper.Collections;

namespace Helper.Physics
{
    public class PhysicsManager
    {
        public PhysicsSystem PhysicsSystem { get; private set; }
        private System.Timers.Timer tmrPhysicsUpdate;
        private Stopwatch tmrPhysicsElapsed;
        private double lastPhysicsElapsed;
        private SortedList<int, Gobject> gameObjects; // This member is accessed from multiple threads and needs to be locked
        private SortedList<int, Gobject> newObjects;
        public bool DebugPhysics { get; set; }
        public bool PhysicsEnabled { get; set; }
        double TIME_STEP = .01; // Recommended timestep
        float SimFactor = 1.0f;

        public PhysicsManager(ref SortedList<int, Gobject> gObjects, ref SortedList<int, Gobject> nObjects, double updateInterval = 10)
        {
            gameObjects = gObjects;
            newObjects= nObjects;
            InitializePhysics(updateInterval);
        }

        private void InitializePhysics(double updateInterval)
        {
            PhysicsSystem = new PhysicsSystem();
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();
            PhysicsSystem.EnableFreezing = true;
            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;

            PhysicsSystem.CollisionSystem.UseSweepTests = true;
            PhysicsSystem.Gravity = new Vector3(0, -10, 0);
            // CollisionTOllerance and Allowed Penetration
            // changed because our objects were "too small"
            PhysicsSystem.CollisionTollerance = 0.01f;
            PhysicsSystem.AllowedPenetration = 0.001f;

            //PhysicsSystem.NumCollisionIterations = 8;
            //PhysicsSystem.NumContactIterations = 8;
            PhysicsSystem.NumPenetrationRelaxtionTimesteps = 15;

            tmrPhysicsElapsed = new Stopwatch();
            tmrPhysicsUpdate = new System.Timers.Timer();
            tmrPhysicsUpdate.AutoReset = false;
            tmrPhysicsUpdate.Enabled = false;
            tmrPhysicsUpdate.Interval = updateInterval;
            tmrPhysicsUpdate.Elapsed += new System.Timers.ElapsedEventHandler(tmrPhysicsUpdate_Elapsed);
            tmrPhysicsUpdate.Start();
            PhysicsEnabled = true;
        }

        void tmrPhysicsUpdate_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Counter.AddTick("pups");
            Counter.AddTick("average_pups", Counter.GetAverageValue("pups"));
            //Add our new objects
            FinalizeNewObjects();

            CallPreIntegrate();
            // Should use a variable timerate to keep up a steady "feel" if we bog down?
            if (PhysicsEnabled)
            {
                float step = (float)TIME_STEP * SimFactor;
                PhysicsSystem.CurrentPhysicsSystem.Integrate(step);
            }
            CallPostIntegrate();

            lastPhysicsElapsed = tmrPhysicsElapsed.ElapsedMilliseconds;

            ResetTimer();
        }
        public event Helper.Handlers.voidEH PostIntegrate;
        private void CallPostIntegrate()
        {
            if (PostIntegrate == null)
                return;
            PostIntegrate();
        }
        public event Helper.Handlers.voidEH PreIntegrate;
        private void CallPreIntegrate()
        {
            if (PreIntegrate == null)
                return;
            PreIntegrate();
        }

        private void FinalizeNewObjects()
        {
            lock (gameObjects)
            {
                lock (newObjects)
                {
                    while (newObjects.Count > 0)
                    {
                        // Remove from end of list so no shuffling occurs? (maybe)
                        int id = newObjects.Values[0].ID;
                        newObjects[id].FinalizeBody();
                        gameObjects.Add(newObjects[id].ID, newObjects[id]);
                        newObjects.Remove(id);
                    }
                }
            }
        }
        public void ResetTimer()
        {
            tmrPhysicsElapsed.Restart();

            tmrPhysicsUpdate.Stop();
            tmrPhysicsUpdate.Start();
        }

        public Aircraft GetAircraft(Vector3 pos, Model model, Vector3 size, Matrix orient)
        {
            Box boxPrimitive = new Box(-.5f * size, orient, size); // relative to the body, the position is the top left-ish corner instead of the center, so subtract from the center, half of all sides to get that point.
            Box leftWing = new Box(new Vector3(-6, -.6f, -2.30f), orient, new Vector3(5, .1f, 1.2f));
            Box rightWing = new Box(new Vector3(.7f, -.6f, -2.30f), orient, new Vector3(5, .1f, 1.2f));
            Box fuse = new Box(new Vector3(-.9f, -.7f, -2.30f), orient, new Vector3(1.9f, 1f, 6f));
            
            //leftWing.Position
            List<MaterialProperties> props = new List<MaterialProperties>();
            props.Add(new MaterialProperties(.3f,.3f,.3f));
            props.Add(new MaterialProperties(.3f, .3f, .3f));
            props.Add(new MaterialProperties(.3f, .3f, .3f));
            List<Primitive> prims = new List<Primitive>();
            prims.Add(leftWing);
            prims.Add(rightWing);
            prims.Add(fuse);
            Aircraft a = new Aircraft(new Vector3(0, -14, 0), new Vector3(1, 1, 1), prims, props, model, "Airplane");
            return a;
        }

        public CarObject GetCar(Model carModel, Model wheelModel)
        {
            CarObject carObject = null;
            try
            {
                carObject = new CarObject("car",
                    //new Vector3(-60, 0.5f, 8), // camera's left
                    new Vector3(0, 2.5f, 0),
                    carModel, wheelModel, true, true, 30.0f, 5.0f, 4.7f, 5.0f, 0.20f, 0.4f, 0.05f, 0.45f, 0.3f, 1, 520.0f, PhysicsSystem.Gravity.Length());
                carObject.Car.EnableCar();
                carObject.Car.Chassis.Body.AllowFreezing = false;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
            return carObject;
        }
        /// <summary>
        /// Adds a new object to the physics system
        /// </summary>
        /// <param name="gob"></param>
        /// <returns></returns>
        public bool AddNewObject(Gobject gob)
        {
            lock (gameObjects)
            {
                lock (newObjects)
                {
                    if (gameObjects.ContainsKey(gob.ID) ||
                        newObjects.ContainsKey(gob.ID))
                        return false;

                    newObjects.Add(gob.ID, gob);
                }
            }
            return true;
        }


        
        public Gobject GetBox(Model model)
        {
            return GetBox(new Vector3(0, 0, 0), new Vector3(2, 2, 2), Matrix.Identity, model, true);
        }
        public Gobject GetBox(Vector3 pos, Vector3 size, Matrix orient, Model model, bool moveable)
        {
            // position of box was upper leftmost corner
            // body has world position
            // skin is relative to the body
            Box boxPrimitive = new Box(-.5f * size, orient, size); // relative to the body, the position is the top left-ish corner instead of the center, so subtract from the center, half of all sides to get that point.

            Gobject box = new Gobject(
                pos,
                size / 2,
                boxPrimitive,
                model,
                moveable,
                "cube"
                );


            return box;
        }

        public Gobject GetBoxHighFriction(Vector3 pos, Vector3 size, Matrix orient, Model model, bool moveable)
        {
            // position of box was upper leftmost corner
            // body has world position
            // skin is relative to the body
            Box boxPrimitive = new Box(-.5f * size, orient, size); // relative to the body, the position is the top left-ish corner instead of the center, so subtract from the center, half of all sides to get that point.

            Gobject box = new Gobject(
                pos,
                size / 2,
                boxPrimitive, 
                MaterialTable.MaterialID.NotBouncyRough,
                model,
                "cube"
                );

            return box;
        }

        public void AddSpheres(int n, Model s)
        {
            Random r = new Random();
            for (int i = 0; i < n; i++)
            {
                GetSphere(
                    new Vector3(
                        (float)(10 - r.NextDouble() * 20),
                        (float)(40 - r.NextDouble() * 20),
                        (float)(10 - r.NextDouble() * 20)),
                    (float)(.5f + r.NextDouble()), s, true);
            }
        }
        private void AddSphere(Model s)
        {
            GetSphere(new Vector3(0, 3, 0), .5f, s, true);
        }
        public Gobject GetDefaultSphere(Model model)
        {
            return GetSphere(new Vector3(0, 0, 0), 5, model, true);
        }
        public Gobject GetSphere(Vector3 pos, float radius, Model model, bool moveable)
        {
            Sphere spherePrimitive = new Sphere(pos, radius);
            Gobject sphere = new Gobject(
                pos,
                Vector3.One * radius,
                spherePrimitive,
                model,
                moveable,
                "sphere");

            //newObjects.Add(sphere.ID, sphere);
            return sphere;
        }
        public LunarVehicle GetLunarLander(Vector3 pos, Vector3 size, Matrix orient, Model model)
        {
            //Box boxPrimitive = new Box(-.5f * size, orient, size); // this is relative to the Body!
            LunarVehicle lander = new LunarVehicle(
                pos,
                size,
                orient,
                model,
                "lunar lander"
                );

            //newObjects.Add(lander.ID, lander);
            return lander;
        }

        public void SetSimFactor(float value)
        {
            SimFactor = value;
        }

        public void Stop()
        {
            if(tmrPhysicsElapsed!=null)
                tmrPhysicsElapsed.Stop();
            if(tmrPhysicsUpdate!=null)
                tmrPhysicsUpdate.Stop();
        }


        public LunarVehicle GetLunarLander(Model model)
        {
            return GetLunarLander(new Vector3(0, 0, 0), new Vector3(2, 2, 2), Matrix.Identity, model);
        }

        public Aircraft GetAircraft(Model model)
        {
            return GetAircraft(new Vector3(0, 0, 0), model, new Vector3(1,1,1), Matrix.Identity);
        }

        public Gobject GetRover(Model roverModel, Model wheelModel, Model radar, Model laser)
        {
            RoverObject carObject = null;
            try
            {
                carObject = new RoverObject("Rover2",
                    //new Vector3(-60, 0.5f, 8), // camera's left
                    new Vector3(0, 2.5f, 0),
                    roverModel, wheelModel, radar, laser, 30.0f, 5.0f, 4.7f, 5.0f, 0.20f, 0.4f, 0.05f, 0.45f, 0.3f, 1, 80.0f, PhysicsSystem.Gravity.Length());
                carObject.Rover.EnableCar();
                carObject.Rover.Chassis.Body.AllowFreezing = false;
            }
            catch (Exception E)
            {
                System.Diagnostics.Debug.WriteLine(E.StackTrace);
            }
            return carObject;
        }
    }
}
