using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Fireball : Ability
{

    public void TurnOffCollider()
    {
        GetComponent<Collider2D>().enabled = false;
    }

    public void DespawnFireball()
    {
        if(IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }
    public void OnTriggerEnter2D(Collider2D collider) {
        if (IsServer)
        {
            if (collider.gameObject.tag == "Unit")
            {
                if (collider.GetComponent<UnitLogic>().GetUnitOwner() != playerOwner.Value)
                {
                    collider.GetComponent<NetworkObject>().Despawn(true);
                }
            }
            if (collider.gameObject.tag == "Base")
            {
                if (collider.GetComponent<BaseLogic>().GetBaseOwner() != playerOwner.Value)
                {
                    collider.GetComponent<BaseLogic>().SetAmountOfUnits(collider.GetComponent<BaseLogic>().GetAmountOfUnits() - 10);
                }
            }
        }
    }

}
