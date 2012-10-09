using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JigLibX.Vehicles
{
    // Colby Added this.
    // It's all his fault. =/

    public class RollingVehicle
    {
        public Chassis chassis;
        public List<Wheel> wheels;
        /// <summary>
        /// Allow access to all the wheels
        /// </summary>
        public List<JigLibX.Vehicles.Wheel> Wheels
        {
            get { return wheels; }
        }

        /// <summary>
        /// AddExternalForces
        /// </summary>
        /// <param name="dt"></param>
        public virtual void AddExternalForces(float dt)
        {
            for (int i = 0; i < wheels.Count; i++)
                wheels[i].AddForcesToCar(dt);
        }

         /// <summary>
        /// Update stuff at the end of physics
        /// </summary>
        /// <param name="dt"></param>
        public virtual void PostPhysics(float dt)
        {
        }

        /// <summary>
        /// Sets up some sensible wheels based on the chassis
        /// </summary>
        public virtual void SetupDefaultWheels()
        {
        }
    }
}
