using UnityEngine;

namespace Ballistics
{
    public class ProjectileSpawner : MonoBehaviour
    {

        private ProjectileKey _previousProjectileKey;
        [SerializeField] 
        private ProjectileKey projectileType;
        public ProjectileKey ProjectileType
        {
            get => projectileType;
            set
            {
                projectileType = value;
                var projectile = ProjectileDatabase.GetProjectile(projectileType);
                materialType = projectile.MaterialKey;
                projectileDiameterMm = projectile.DiameterMm;
                projectileLengthMm = projectile.LengthMm;
                projectileVelocityMS = projectile.VelocityMs;
            }
        }
    
        [SerializeField]
        private MaterialKey materialType;
        public float projectileDiameterMm;

        public float projectileLengthMm;
        public float projectileVelocityMS;


        private float _diameterM;
        private float _lengthM;
        private float _volume;
    

        private int _frame = 0;
        private int fireDelay = 60;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void FireProjectile()
        {
            // Convert diameter and length from mm to meters
            _diameterM = projectileDiameterMm / 1000f;  // Convert mm to m
            _lengthM = projectileLengthMm / 1000f;      // Convert mm to m
        

            // Calculate the volume of the cylinder (projectile)
            _volume = Mathf.PI * Mathf.Pow(_diameterM / 2f, 2) * _lengthM;
        
        
            // var projectileInstance = Instantiate(projectilePrefab, transform);
            // projectileInstance.SetProjectileProperties(projectileVelocityMS * transform.forward, _diameterM, _lengthM, MaterialType);
            Projectile.Create(transform.position, projectileVelocityMS * transform.forward, 0, _diameterM,
                _lengthM, materialType, Projectile.ProjectileType.Bullet);
        }

        private void FixedUpdate()
        {
            if (++_frame % fireDelay == 0)
            {
                _frame = 0;
                FireProjectile();
            }
        }

    
    
        // This method is called in the editor when a value is changed in the Inspector
        private void OnValidate()
        {
            
            if (_previousProjectileKey != projectileType)
            {
                var projectile = ProjectileDatabase.GetProjectile(projectileType);
                materialType = projectile.MaterialKey;
                projectileDiameterMm = projectile.DiameterMm;
                projectileLengthMm = projectile.LengthMm;
                projectileVelocityMS = projectile.VelocityMs;
                _previousProjectileKey = projectileType;
            }
            

        }
    }
}
