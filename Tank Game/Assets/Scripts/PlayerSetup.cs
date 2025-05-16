using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("Player setting up!");

        // all need to be able to see all scripts
        var tm = gameObject.AddComponent<TankMovement>(); // tank movement first
        var pt = gameObject.AddComponent<PlayerTeam>(); // player team first
        var player = gameObject.AddComponent<Player>(); // player before tank visuals
        var tv = gameObject.AddComponent<TankVisuals>(); // tank visuals last
        player.Setup(tm, pt, tv, IsOwner);

        if (IsOwner)
        {
            // only target tank if client is the player of said tank
            Camera.main.GetComponent<CameraControl>().target = transform;
        }

        this.enabled = false; // finish setup
    }
}
