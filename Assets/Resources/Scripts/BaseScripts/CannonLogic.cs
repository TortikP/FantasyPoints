using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class CannonLogic : BaseLogic
{
    public float fireInterval;
    public int firePower;
    public float bulletSpeed;
    public BulletLogic bulletPrefab;
    public GameObject canonTop;
    public float rotationSpeed;
    public float rotationModifier;
    private BaseLogic targetBase;
    public SpriteRenderer baseCannon;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //baseSprite.enabled = true;
        baseCannon.sortingOrder = Mathf.RoundToInt(-transform.position.y+1.5f);
        baseOwner.OnValueChanged += CannonBaseOwner_OnValueChanged;
        if (!IsServer)
        {
            return;
        }
        StartCoroutine(FireTimer());
    }
    public override void Update()
    {
        if(targetBase != null)
        {
            //���������� �������� �����
            Vector3 vectorToTarget = targetBase.transform.position - canonTop.transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - rotationModifier;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            canonTop.transform.rotation = Quaternion.Slerp(canonTop.transform.rotation, q, Time.deltaTime * rotationSpeed);
        }
        base.Update();
    }

    private void CannonBaseOwner_OnValueChanged(PlayerNumber previousValue, PlayerNumber currentValue)
    {
        if (IsClient)
        {
            if (GameManager.Instance.myPlayerObject.playerId.Value == (int)currentValue)
            {
                baseCannon.material = GameManager.Instance.materials[1];
            }
            else
            {
                baseCannon.material = GameManager.Instance.materials[2];
            }
            //baseCannon.SetPropertyBlock(propertyBlock);
        }
    }

    public IEnumerator FireTimer()
    {
        System.Random rand = new System.Random();
        while (true)
        {
            yield return new WaitUntil(() => GetBaseOwner() != PlayerNumber.neutral);
            targetBase = GameManager.Instance.BaseList().Where(b => b.GetBaseIndex() != baseIndex && b.GetBaseOwner() != baseOwner.Value).
                OrderBy(r => rand.Next()).FirstOrDefault();
            yield return new WaitForSeconds(fireInterval);
            yield return new WaitUntil(() => !isFrozen);
            if (targetBase.GetBaseOwner() != GetBaseOwner())
            {
                Fire(targetBase);
            }
        }
    }

    public void Fire(BaseLogic targetBase)
    {
        BulletLogic a = Instantiate(bulletPrefab, canonTop.transform.position, canonTop.transform.rotation);    
        a.SetTargetBase(targetBase);
        a.SetOwnerBase(this);   
        a.SetBulletSpeed(bulletSpeed);
        a.SetBulletPower(firePower);
        a.GetComponent<NetworkObject>().Spawn();
        a.bulletFaction.Value = baseFaction.Value;
    }


}
