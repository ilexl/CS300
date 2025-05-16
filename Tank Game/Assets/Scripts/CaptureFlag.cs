using Unity.Netcode;
using UnityEngine;

public class CaptureFlag : NetworkBehaviour
{
    [SerializeField] Vector3 flagUpPos;
    [SerializeField] Vector3 flagDownPos;
    [SerializeField] GameObject flagObj;
    [SerializeField] Material whiteMat, teamOrange, teamBlue;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ResetFlag()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
