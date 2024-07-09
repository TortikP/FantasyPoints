using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FountainBaseLogic : BaseLogic
{

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        baseOwner.OnValueChanged += ManaFountain_Captured;
    }
    public void ManaFountain_Captured(PlayerNumber previousValue, PlayerNumber currentValue)
    {
        foreach(PlayerController playerController in FindObjectsOfType<PlayerController>())
        {
            if(playerController.playerId.Value == (int) currentValue)
            {
                playerController.playerManaFountains++;
            }

            if(playerController.playerId.Value == (int) previousValue)
            {
                playerController.playerManaFountains--;
            }
        }
    }

}
