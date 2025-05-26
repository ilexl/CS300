using UnityEngine;
using UnityEngine.Serialization;

namespace Ballistics
{
    public class FunctionalTankModule : DamageableTankModule
    {
        [SerializeField]
        private float initialHealth;
        private float health;
        public float Health { 
            get => health;
            set
            {
                health = value;
                if (health <= 0)
                {
                    ComponentDestroyed();
                }
            }
            
        }

        private void Start()
        {
            Health = initialHealth;
        }
        
        public override void PostPenetration(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity,
            float projectileDiameter)
        {
            base.PostPenetration(entryPoint, exitPoint, thickness, projectileVelocity, projectileDiameter);
            
            float damage = projectileDiameter * thickness * projectileVelocity.magnitude;
            Debug.Log("Damage: " + damage + "");
            Health -= damage;
        }
        public override void NonPenetration(Vector3 entryPoint, Vector3 exisPoint, float thickness, Vector3 projectileVelocity,
            float projectileDiameter)
        {
            float damage = projectileDiameter * thickness * projectileVelocity.magnitude;
            Debug.Log("Damage: " + damage + "");
            Health -= damage;
        }

        public void ComponentDestroyed()
        {
            Destroy(gameObject);
        }
    }
}