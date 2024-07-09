using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainBaseLogic : BaseLogic
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        baseOwner.OnValueChanged += MainBase_Captured;
    }

    public void MainBase_Captured(PlayerNumber previousOwner,PlayerNumber newOwner)
    {
        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            if (player.playerId.Value == (int)newOwner)
            {
                player.mainBases.Value++;
            }
            else if (player.playerId.Value == (int)previousOwner)
            {
                player.mainBases.Value--;
            }
        }
    }
}
