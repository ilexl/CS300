using UnityEngine;

namespace Ballistics
{
    // An interface masquerading as an abstract class. Hindsight is 20/20
    public abstract class DamageableTankModule : MaterialObject
    {
        
        /// <summary>
        /// Called when a projectile fully penetrates the object hit. Expected to be overridden for sound events, spall generation, etc.
        /// </summary>
        /// <param name="entryPoint">The place of first impact of the projectile.</param>
        /// <param name="exitPoint">The place where the projectile exited the object.</param>
        /// <param name="thickness">The distance between the entry and the exit.</param>
        /// <param name="projectileVelocity">How fast the projectile is going after penetration.</param>
        /// <param name="previousVelocity">How fast the projectile was going prior to penetration.</param>
        /// <param name="projectileDiameter"></param>
        /// <param name="rng">Network deterministic random number generator.</param>
        public virtual void PostPenetration(Vector3 entryPoint,
            Vector3 exitPoint,
            float thickness,
            Vector3 projectileVelocity,
            Vector3 previousVelocity, 
            float projectileDiameter, System.Random rng)
        {

        }
        /// <summary>
        /// Called when a projectile is stopped by an object it hits. Expected to be overridden for sound events, hp subtraction, etc.
        /// </summary>
        /// <param name="entryPoint">The place of first impact of the projectile.</param>
        /// <param name="exitPoint">The place where the projectile exited the object.</param>
        /// <param name="thickness">The distance between the entry and the exit.</param>
        /// <param name="projectileVelocity">How fast the projectile is going after penetration.</param>
        /// <param name="previousVelocity">How fast the projectile was going prior to penetration.</param>
        /// <param name="projectileDiameter"></param>
        /// <param name="rng">Network deterministic random number generator.</param>
        public virtual void NonPenetration(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, float projectileDiameter, System.Random rng)
        {
            
        }
        /// <summary>
        /// Called when a projectile ricochets off an object it hits. Currently not implemented anywhere, but fully functional.
        /// </summary>
        /// <param name="entryPoint">The place of first impact of the projectile.</param>
        /// <param name="exitPoint">The place where the projectile exited the object.</param>
        /// <param name="thickness">The distance between the entry and the exit.</param>
        /// <param name="projectileVelocity">How fast the projectile is going after penetration.</param>
        /// <param name="previousVelocity">How fast the projectile was going prior to penetration.</param>
        /// <param name="projectileDiameter"></param>
        /// <param name="rng">Network deterministic random number generator.</param>
        public virtual void Deflection(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, float projectileDiameter, System.Random rng)
        {
            
        }
    }
}