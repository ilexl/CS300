using UnityEngine;

public class AlwaysLookAtCamera : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if(Camera.main is null) { return; }
        transform.rotation = Camera.main.transform.rotation;
    }
}
