using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Physics;
using JigLibX.Collision;
using System.Diagnostics;
using Physics.PhysicsObjects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using JigLibX.Geometry;

namespace Physics
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
            //PhysicsSystem.Gravity = new Vector3(0, -2f, 0);
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
            //Add our new objects
            lock (gameObjects)
            {
                FinalizeNewObjects();

                // Should use a variable timerate to keep up a steady "feel" if we bog down?
                if (PhysicsEnabled)
                {
                    float step = (float)TIME_STEP * SimFactor;
                    PhysicsSystem.CurrentPhysicsSystem.Integrate(step);
                }
            }

            //TODO: Add to the Camera Manager a way to 
            //cam.UpdatePosition(); // keep the camera moving towards its target position
            //objectCam.UpdatePosition(); // keep the camera moving towards its target position
            lastPhysicsElapsed = tmrPhysicsElapsed.ElapsedMilliseconds;

            ResetTimer();
        }

        private void FinalizeNewObjects()
        {
            while (newObjects.Count > 0)
            {
                // Remove from end of list so no shuffling occurs? (maybe)
                int i = newObjects.Count - 1;
                newObjects[i].FinalizeBody();
                gameObjects.Add(newObjects[i].ID, newObjects[i]);
                if (newObjects[i] is CarObject)
                {
                    Debug.WriteLine("Car object moved from new to main");
                }
                newObjects.RemoveAt(i);
            }
        }
        public void ResetTimer()
        {
            tmrPhysicsElapsed.Restart();

            tmrPhysicsUpdate.Stop();
            tmrPhysicsUpdate.Start();
        }

        public CarObject GetCar(Model carModel, Model wheelModel)
        {
            CarObject carObject = null;
            try
            {
                carObject = new CarObject(
                    //new Vector3(-60, 0.5f, 8), // camera's left
                    new Vector3(0, 2.5f, 0),
                    carModel, wheelModel, true, true, 30.0f, 5.0f, 4.7f, 5.0f, 0.20f, 0.4f, 0.05f, 0.45f, 0.3f, 1, 520.0f, PhysicsSystem.Gravity.Length());
                carObject.Car.EnableCar();
                carObject.Car.Chassis.Body.AllowFreezing = false;
                
                
            }
            catch (Exception E)
            {
            }
            return carObject;
        }

        public bool AddNewObject(Gobject gob)
        {
            if (gameObjects.ContainsKey(gob.ID) ||
                newObjects.ContainsKey(gob.ID))
                return false;
            newObjects.Add(gob.ID, gob);
            return true;
        }

        private Gobject GetBox(Vector3 pos, Vector3 size, Matrix orient, Model model, bool moveable)
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
                moveable
                );

            newObjects.Add(box.ID, box);
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
                moveable);

            newObjects.Add(sphere.ID, sphere);
            return sphere;
        }
        public LunarVehicle GetLunarLander(Vector3 pos, Vector3 size, Matrix orient, Model model)
        {
            Box boxPrimitive = new Box(-.5f * size, orient, size); // this is relative to the Body!
            LunarVehicle lander = new LunarVehicle(
                pos,
                size / 2,
                boxPrimitive,
                model
                );

            newObjects.Add(lander.ID, lander);
            return lander;
        }

        public void SetSimFactor(float value)
        {
            SimFactor = value;
        }
    }
}
