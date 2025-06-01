using UnityEngine;
using UnityEngine.Serialization;

namespace Ballistics
{
    public class FunctionalTankModule : DamageableTankModule
    {

        public bool destroyOnHealthGone = true;
        [SerializeField]
        private float initialHealth;
        [SerializeField]
        private float health;
        
        private Renderer _renderer;
        private bool _colourNeedsUpdating = false;
        
        
        #region Properties
        public float Health { 
            get => health;
            set
            {
                float oldHealth = health;
                
                health = value;
                ComponentDamaged(oldHealth);
                if (health <= 0)
                {
                    ComponentDestroyed();
                }
            }
            
        }
        public float HealthRatio
        {
            get => health / initialHealth;
            set => health = initialHealth * value;
        }
        #endregion
       

        
        #region penetrationEffects
        public override void PostPenetration(Vector3 entryPoint, Vector3 exitPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, 
            float projectileDiameter)
        {
            DebugUtils.DebugDrawSphere(entryPoint, 0.1f, Color.red, 1);
            base.PostPenetration(entryPoint, exitPoint, thickness, projectileVelocity, previousVelocity, projectileDiameter);
            
            float damage = (projectileDiameter * 1000) * thickness * previousVelocity.magnitude;
            Health -= damage;
        }
        public override void NonPenetration(Vector3 entryPoint, Vector3 exisPoint, float thickness, Vector3 projectileVelocity, Vector3 previousVelocity, 
            float projectileDiameter)
        {
            DebugUtils.DebugDrawSphere(entryPoint, 0.1f, Color.red, 1);
            base.NonPenetration(entryPoint, exisPoint, thickness, projectileVelocity, previousVelocity, projectileDiameter);
            
            float damage = (projectileDiameter * 1000) * thickness * previousVelocity.magnitude;
            Health -= damage;
        }
        #endregion
        
        #region Events
        public void ComponentDestroyed()
        {
            if (destroyOnHealthGone)
                Destroy(gameObject);
        }
        
        private void ComponentDamaged(float oldHealth)
        {
            _colourNeedsUpdating = true;
        }
        #endregion
        

        #region UnityLogic
        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            
            Health = initialHealth;
        }
        private void Update()
        {

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