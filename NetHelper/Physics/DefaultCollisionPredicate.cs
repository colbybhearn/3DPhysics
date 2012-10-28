using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JigLibX.Collision;

namespace Helper.Physics
{
    public class DefaultCollisionPredicate : CollisionSkinPredicate1
    {
        public override bool ConsiderSkin(CollisionSkin skin0)
        {
            return true;
        }
    }
}
