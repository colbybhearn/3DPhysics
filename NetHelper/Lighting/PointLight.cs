using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Helper.Lighting
{
    public class PointLight
    {
        public Vector3 lightPos;
        public float lightPower;
        public float ambientPower;

        public PointLight()
        {
            lightPos = new Vector3(-10, -11, -2);
            lightPower = 1.0f;
            ambientPower = 0.2f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="plPow">Point Light power</param>
        /// <param name="ambPow">Ambient Light power</param>
        public PointLight(Vector3 pos, float plPow, float ambPow)
        {

        }
    }
}
