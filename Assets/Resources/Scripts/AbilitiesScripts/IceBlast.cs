using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class IceBlast : Ability
{

    public List<GameObject> affectedObjects;
    public float timeAlive = 0;
    public void Start()
    {
        affectedObjects = new List<GameObject>();
    }
    public void Update()
    {
        if(timeAlive >= abilityDuration)
        {
            foreach(GameObject obj in affectedObjects)
            {
                if (obj != null)
                {
                    if (obj.CompareTag("Unit"))
                            obj.GetComponent<UnitLogic>().SetCurrentSpeed(obj.GetComponent<UnitLogic>().GetSpeed());

                    if (obj.CompareTag("Base"))
                            obj.GetComponent<BaseLogic>().isFrozen = false;
                }
            }
            if (IsServer) NetworkObject.Despawn(true);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.CompareTag("Unit")) {
            if (collider.GetComponent<UnitLogic>().GetUnitOwner() != playerOwner.Value) {
                collider.gameObject.GetComponent<UnitLogic>().SetCurrentSpeed(0f);
                affectedObjects.Add(collider.gameObject);
            }
        }

        if(collider.CompareTag("Base")) {
            if (collider.GetComponent<BaseLogic>().GetBaseOwner() != playerOwner.Value) {
                collider.gameObject.GetComponent<BaseLogic>().isFrozen = true;
                affectedObjects.Add(collider.gameObject);
            }
        }
    }

}
