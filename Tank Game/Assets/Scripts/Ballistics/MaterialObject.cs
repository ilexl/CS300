using UnityEngine;

namespace Ballistics
{
    public abstract class MaterialObject : MonoBehaviour
    {
        [SerializeField] 
        private MaterialKey materialType; // Backing field, directly set in unity editor

        private Material _material;

        public Material Material => _material;

        public MaterialKey MaterialType
        {
            get => materialType;
            set
            {
                materialType = value;
                _material = MaterialDatabase.GetMaterial(materialType);
            }
        }
    
        // This method is called in the editor when a value is changed in the Inspector
        private void OnValidate()
        {
            // Ensure your material database lookup is valid.
            _material = MaterialDatabase.GetMaterial(materialType);
        }

        private void Awake()
        {
            // Ensure your material database lookup is valid.
            _material = MaterialDatabase.GetMaterial(materialType);
        }
        
    }
}