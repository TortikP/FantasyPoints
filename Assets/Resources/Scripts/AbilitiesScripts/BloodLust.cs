using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BloodLust : Ability
{
    private float timeAlive = 0;
    public List<UnitLogic> affectedUnits;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            foreach (UnitLogic unit in FindObjectsOfType<UnitLogic>())
            {
                if (unit.GetUnitOwner() == playerOwner.Value)
                {
                    unit.isAccelerated.Value = true;
                    affectedUnits.Add(unit);
                }
            }
        }
    }

    public void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (timeAlive >= abilityDuration)
        {
            foreach (UnitLogic unit in affectedUnits)
            {
                if (unit != null)
                {
                    unit.isAccelerated.Value = false;
                }
            }
            NetworkObject.Despawn(true);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }

}
