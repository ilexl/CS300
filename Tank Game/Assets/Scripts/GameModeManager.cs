using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { FreeForAll, TeamDeathmatch, CaptureTheFlag }
public class GameModeManager : NetworkBehaviour
{
    [SerializeField] GameMode currentGameMode;
    public GameMode GetCurrentGamemode()
    {
        return currentGameMode;
    }


    // more to follow

}
