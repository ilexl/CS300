using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField]TankVarients tempTankRespawn;
    public void RespawnTEMP()
    {
        Debug.LogWarning("TEMP respawn being used...");
        Player player = FindFirstObjectByType<Player>();
        player.ChangeTank(tempTankRespawn);
    }
}
