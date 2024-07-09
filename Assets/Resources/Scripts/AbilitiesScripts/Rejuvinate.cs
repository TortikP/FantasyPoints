using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Rejuvinate : Ability
{
    public ulong targetBaseIndex;
    private BaseLogic targetBase;
    public GameObject rejuvinateEffect;
    private float timeAlive = 0;
    private GameObject effect;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            targetBase = GameManager.Instance.BaseList().Where(b => b.GetBaseIndex() == targetBaseIndex).FirstOrDefault();
            targetBase.SetIncomeDelay(targetBase.GetIncomeDelay() / 2);
            targetBase.SetAmountOfUnits(targetBase.GetAmountOfUnits() + abilityPower);
            effect = Instantiate(rejuvinateEffect);
            effect.transform.position = targetBase.transform.position;
            effect.GetComponent<NetworkObject>().Spawn(true);
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
            targetBase.SetIncomeDelay(targetBase.GetIncomeDelay() * 2);
            effect.GetComponent<NetworkObject>().Despawn(true);
            NetworkObject.Despawn(true);
        }
        else
        {
            timeAlive += Time.deltaTime;
            if(playerOwner.Value != PlayerNumber.neutral)
            {
                if(playerOwner.Value != targetBase.GetBaseOwner())
                {
                    targetBase.SetIncomeDelay(targetBase.GetIncomeDelay() * 2);
                    effect.GetComponent<NetworkObject>().Despawn(true);
                    NetworkObject.Despawn(true);
                }
            }
        }
    }
}
