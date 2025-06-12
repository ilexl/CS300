using UnityEngine;
using UnityEngine.Serialization;

namespace Ballistics
{
    public class FunctionalTankModule : DamageableTankModule
    {
        public Type CurrentType;
        [SerializeField] bool healthInit = false;
        public bool destroyOnHealthGone = true;
        [SerializeField]
        private float initialHealth;
        [SerializeField]
        private float health;
        
        private Renderer _renderer;
        private bool _colourNeedsUpdating = false;
        bool alreadyDestroyed = false;

        #region Enum
        public enum Type
        {
            None, 
            // parts
            Engine,
            Transmission,
            Track,
            Wheel,
            Barrel,
            Breach,
            Ammo,
            // crew
            Commander,
            Driver,
            Gunner,
            Loader
        }

        #endregion


        #region Properties
        public float Health { 
            get => health;
            set
            {
                if(health > 0)
                {
                    alreadyDestroyed = false;
                }
                float oldHealth = health;
                health = value;
                ComponentDamaged(oldHealth);
                if (health <= 0)
                {
                    ComponentDestroyed();
                    alreadyDestroyed = true;
                }
            }
        }

        public void ServerSetHealth(float h)
        {
            health = h;
            _colourNeedsUpdating = true;
        }

        public float HealthRatio
        {
            get => health / initialHealth;
            set => health = initialHealth * value;
        }
        #endregion
       
        public float GetInitialHealth() { return initialHealth; }
        
        #region penetrationEffects
        public override void PostPenetration(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, 
            float projectileDiameter, System.Random rng)
        {
            DebugUtils.DebugDrawSphere(entryPoint, 0.1f, Color.red, 1);
            base.PostPenetration(entryPoint, exitPoint, thickness, projectileVelocity, previousVelocity, projectileDiameter, rng);
            
            float damage = (projectileDiameter * 1000) * thickness * previousVelocity.magnitude;
            Health -= damage;
        }
        public override void NonPenetration(Vector3 entryPoint, Vector3 exisPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, 
            float projectileDiameter, System.Random rng)
        {
            DebugUtils.DebugDrawSphere(entryPoint, 0.1f, Color.red, 1);
            base.NonPenetration(entryPoint, exisPoint, thickness, projectileVelocity, previousVelocity, projectileDiameter, rng);
            
            float damage = (projectileDiameter * 1000) * thickness * previousVelocity.magnitude;
            Health -= damage;
        }
        #endregion
        
        #region Events
        public void ComponentDestroyed()
        {
            if (alreadyDestroyed) { return; }
            if (transform.root.GetComponent<TankCombat>() != null && healthInit == true)
            {
                transform.root.GetComponent<TankCombat>().ComponentHealthUpdate(this);
            }
            Debug.Log($"Component Destroyed | {gameObject} | {CurrentType}");
            if (destroyOnHealthGone)
                Destroy(gameObject);
        }
        
        private void ComponentDamaged(float oldHealth)
        {
            if (alreadyDestroyed) { return; }
            if (transform.root.GetComponent<TankCombat>() != null && healthInit == true)
            {
                transform.root.GetComponent<TankCombat>().ComponentHealthUpdate(this);
            }
            _colourNeedsUpdating = true;
        }
        #endregion
        

        #region UnityLogic
        private void Start()
        {
            healthInit = false;
            _renderer = GetComponent<Renderer>();
            
            if(initialHealth <= 0) 
            {
                Debug.LogWarning($"init health for {gameObject.name} was <= 0... Set to 100 as a fail over...");
                initialHealth = 100; 
            } // default to 100
            Health = initialHealth;
            healthInit = true;
        }
        private void Update()
        {
            if (_renderer == null)
            {
                return; // no renderer on server
            }

            if (_colourNeedsUpdating)
            {
                
                float ratio = HealthRatio;

                if (ratio <= 0)
                {
                    _renderer.material.color = Color.black;
                    return;
                }
                // Maybe should do this once a tick instead
                Color interpolatedColor = ratio < 0.5f 
                    ? Color.Lerp(Color.red, Color.yellow, ratio * 2) 
                    : Color.Lerp(Color.yellow, Color.white, (ratio - 0.5f) * 2);
                
                _renderer.material.color = interpolatedColor;
                _colourNeedsUpdating = false;
            }
            
        }
        #endregion
        
    }
}