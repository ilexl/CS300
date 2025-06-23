using UnityEngine;

namespace Ballistics
{
    /// <summary>
    /// A monobehavior with an attached material component. Useful to inherit from.
    /// </summary>
    public abstract class MaterialObject : MonoBehaviour
    {
        [SerializeField] 
        private MaterialKey materialType; // Backing field, directly set in unity editor

        public Material Material { get; private set; }

        public MaterialKey MaterialType
        {
            get => materialType;
            set
            {
                materialType = value;
                Material = MaterialDatabase.GetMaterial(materialType);
            }
        }
    
        // This method is called in the editor when a value is changed in the Inspector
        private void OnValidate()
        {
            // Ensure your material database lookup is valid.
            Material = MaterialDatabase.GetMaterial(materialType);
        }

        private void Awake()
        {
            // Ensure your material database lookup is valid.
            Material = MaterialDatabase.GetMaterial(materialType);
        }
        
    }
}