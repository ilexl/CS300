using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Transforms")]
    [SerializeField] Transform hullTransform;
    [SerializeField] Transform turretTransform;
    [SerializeField] Transform barrelTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return; // Only the local player controls movement

        MovePlayer();
        SyncTransforms();
    }

    private void MovePlayer()
    {
        // Get input axis for movement and rotation
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Move player
            hullTransform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // Rotate player
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            hullTransform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Sync all relevant transforms to the server and other clients
    private void SyncTransforms()
    {
        // Sync transforms
        SyncTransformServerRpc(hullTransform.position, hullTransform.rotation, "hull");
        SyncTransformServerRpc(turretTransform.position, turretTransform.rotation, "turret");
        SyncTransformServerRpc(barrelTransform.position, barrelTransform.rotation, "barrel");

    }

    // ServerRPC to sync transforms with the server and then broadcast to clients
    [ServerRpc(RequireOwnership = false)]
    private void SyncTransformServerRpc(Vector3 position, Quaternion rotation, string transformName)
    {
        // Call the ClientRpc to sync on all clients
        SyncTransformClientRpc(position, rotation, transformName);
    }

    // ClientRPC to sync transforms on all clients
    [ClientRpc]
    private void SyncTransformClientRpc(Vector3 position, Quaternion rotation, string transformName)
    {
        // Only update other clients, not the owner's own transforms
        if (IsOwner) return;

        // Depending on the transformName, sync the appropriate transform
        switch (transformName)
        {
            case "hull":
                hullTransform.position = position;
                hullTransform.rotation = rotation;
                break;
            case "turret":
                turretTransform.position = position;
                turretTransform.rotation = rotation;
                break;
            case "barrel":
                barrelTransform.position = position;
                barrelTransform.rotation = rotation;
                break;

        }
    }
}