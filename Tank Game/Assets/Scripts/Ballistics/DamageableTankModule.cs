using UnityEngine;

namespace Ballistics
{
    public abstract class DamageableTankModule : MaterialObject
    {
        public virtual void PostPenetration(Vector3 entryPoint,
            Vector3 exitPoint,
            float thickness,
            Vector3 projectileVelocity,
            Vector3 previousVelocity, 
            float projectileDiameter, System.Random rng)
        {
            
        }

        public virtual void NonPenetration(Vector3 entryPoint, Vector3 exisPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, float projectileDiameter, System.Random rng)
        {
            
        }

        public virtual void Deflection(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, float projectileDiameter, System.Random rng)
        {
            
        }
    }
}