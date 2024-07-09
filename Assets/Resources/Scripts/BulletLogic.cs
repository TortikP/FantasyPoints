using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletLogic : NetworkBehaviour
{
    private BaseLogic ownerBase;
    private BaseLogic targetBase;
    private float bulletSpeed;
    private int bulletPower;
    public NetworkVariable<Faction> bulletFaction = new NetworkVariable<Faction>(Faction.neutral);

    public override void OnNetworkSpawn()
    {
        bulletFaction.OnValueChanged = BulletFaction_OnValueChanged;
    }

    private void BulletFaction_OnValueChanged(Faction previousFaction, Faction currentFaction)
    {
        switch (currentFaction)
        {
            case Faction.druid:
                GetComponentInChildren<SpriteRenderer>().color = new Color(215,255,0);
                break;
            case Faction.chaos:
                GetComponentInChildren<SpriteRenderer>().color = Color.red;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetBase.transform.position, Time.deltaTime * bulletSpeed);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
        {
            return;
        }
        if (collider.gameObject.GetComponent<BaseLogic>() == targetBase)
        {
            targetBase.SetAmountOfUnits(targetBase.GetAmountOfUnits() - bulletPower);
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public void SetBulletSpeed(float bulletSpeed)
    {
        this.bulletSpeed = bulletSpeed;
    }

    public float GetBulletSpeed()
    {
        return bulletSpeed;
    }

    public void SetTargetBase(BaseLogic targetBase)
    { 
        this.targetBase = targetBase; 
    }
    public BaseLogic GetTargetBase()
    {
        return targetBase;
    }

    public void SetOwnerBase(BaseLogic ownerBase)
    {
        this.ownerBase = ownerBase;
    }
    public BaseLogic GetOwnerBase()
    {
        return ownerBase;
    }
    public void SetBulletPower(int bulletPower)
    { 
        this.bulletPower = bulletPower; 
    }
}
