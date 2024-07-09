using NavMeshPlus.Components;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Teleport : Ability
{

    private float timeAlive;
    public Vector2 firstPoint;
    public Vector2 lastPoint;
    public NavMeshLink link;
    public GameObject firstPortal;
    public GameObject lastPortal;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            link.area = NavMesh.GetAreaFromName("Portal") + (int) playerOwner.Value + 1;
            link.startPoint = firstPoint;
            link.endPoint = lastPoint;
            firstPortal.transform.position = firstPoint;
            lastPortal.transform.position = lastPoint;
            foreach (UnitLogic unit in FindObjectsOfType<UnitLogic>())
            {
                if(unit.GetUnitOwner() == playerOwner.Value)
                {
                    unit.UnitDestination();
                }
            }
            SetPortalsClientRpc(firstPoint, lastPoint);
        }
    }
    [ClientRpc]
    private void SetPortalsClientRpc(Vector2 firstPoint, Vector2 lastPoint)
    {
        link.startPoint = firstPoint;
        link.endPoint = lastPoint;
        firstPortal.transform.position = firstPoint;
        lastPortal.transform.position = lastPoint;
        firstPortal.GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(-firstPortal.transform.position.y);
        lastPortal.GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(-lastPortal.transform.position.y);
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        if (timeAlive >= abilityDuration)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }

}
