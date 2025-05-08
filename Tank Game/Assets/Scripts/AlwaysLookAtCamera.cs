using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
