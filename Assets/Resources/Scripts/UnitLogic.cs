using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using NavMeshPlus.Components;

public class UnitLogic : NetworkBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private PlayerNumber unitOwner;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private RuntimeAnimatorController[] animatorArray;
    private Vector2 destination;
    private float currentSpeed;
    private BaseLogic originBase;
    private BaseLogic destinationBase;
    private Vector2 previousPosition;
    private const float POSITION_WAIT_TIME = 0.1f;
    private float positionWaitTime = POSITION_WAIT_TIME;


    public NetworkVariable<ulong> originBaseId;
    public NetworkVariable<ulong> destinationBaseId;
    public NetworkVariable<bool> isAccelerated = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isDisabled = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isFlipped = new NetworkVariable<bool>(false);
    public SpriteRenderer unitSprite;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        unitSprite.sortingOrder = Mathf.RoundToInt(-transform.position.y);
        previousPosition = transform.position;
        
        agent = GetComponent<NavMeshAgent>();
        agent.speed = maxSpeed;
        agent.updateUpAxis = false;
        agent.updateRotation = false;

        isAccelerated.OnValueChanged += IsAccelerated_OnValueChanged;
        isDisabled.OnValueChanged += IsDisabled_OnValueChanged;
        isFlipped.OnValueChanged += IsFlipped_OnValueChanged;
        originBaseId.OnValueChanged += OriginBaseId_OnValueChanged;
        destinationBaseId.OnValueChanged += DestinationBaseId_OnValueChanged;

    }

    private void IsFlipped_OnValueChanged(bool previousValue, bool currentValue)
    {
        if (IsClient)
        {
            unitSprite.flipX = currentValue;
        }
    }

    private void OriginBaseId_OnValueChanged(ulong previousValue, ulong currentValue)
    {
        //�������� ����
        originBase = NetworkManager.Singleton.SpawnManager.SpawnedObjects[originBaseId.Value].GetComponent<BaseLogic>();
        //����������� ��������� �����
        unitOwner = originBase.GetBaseOwner();
        GetComponent<Animator>().runtimeAnimatorController = animatorArray[(int)originBase.GetBaseFaction()];
        //���������� ������ ��������� ����� ������������ ����
        agent.areaMask += 1 << NavMesh.GetAreaFromName("Portal") + (int)unitOwner + 1;
        //�������� �� ������� ����������� "����� �����" �� �����
        //��������� �����, ���� ����������� ����������� ��������� ����� 
        if(FindObjectOfType<BloodLust>() != null)
        {
            foreach(BloodLust bloodLust in FindObjectsOfType<BloodLust>())
            {
                if(bloodLust.playerOwner.Value == unitOwner)
                {
                    if (IsServer)
                    {
                        bloodLust.affectedUnits.Add(this);
                        isAccelerated.Value = true;
                    }
                }
            }
        }

        if (IsClient)
        {
            if (GameManager.Instance.myPlayerObject.playerId.Value == (int)unitOwner)
            {
                unitSprite.material = GameManager.Instance.materials[1];
            }
            else
            {
                unitSprite.material = GameManager.Instance.materials[2];
            }
        }
    }

    private void DestinationBaseId_OnValueChanged(ulong previousValue, ulong currentValue)
    {
        //���� ����������
        destinationBase = NetworkManager.Singleton.SpawnManager.SpawnedObjects[currentValue].GetComponent<BaseLogic>();
        //����� ����������
        destination = destinationBase.transform.position;
        //������� ����� � ������� ����
        if(destination.x < originBase.transform.position.x)
        {
            unitSprite.flipX = true;
        }
        //���������� ���� �� �������
        if (IsServer)
        {
            UnitDestination(destination);
        }
    }

    private void IsAccelerated_OnValueChanged(bool previousValue, bool currentValue)
    {
        print(name + " " + currentValue);
        if (IsServer)
        {
            if (currentValue)
            {
                SetSpeed(maxSpeed * 2);
                agent.acceleration = agent.acceleration * 2;
            }
            else
            {
                SetSpeed(maxSpeed / 2);
                agent.acceleration = agent.acceleration / 2;
            }
        }
        if (IsClient)
        {
            transform.GetChild(0).gameObject.SetActive(currentValue);
            print(currentValue);
        }

    }

    private void IsDisabled_OnValueChanged(bool previousValue, bool currentValue)
    {
        if (currentValue)
        {
            SetCurrentSpeed(0);
        }
        else
        {
            SetCurrentSpeed(maxSpeed);
        }
    }
    // Update is called once per frame
    void Update()
    {
       /* if (IsClient)
        {
            unitSprite.sortingOrder = Mathf.RoundToInt(-transform.position.y);
            if(positionWaitTime < 0)
            {
                if ((previousPosition.x < transform.position.x && unitSprite.flipX) || (previousPosition.x > transform.position.x && !unitSprite.flipX))
                {
                    unitSprite.flipX = !unitSprite.flipX;
                }
                positionWaitTime = POSITION_WAIT_TIME;
                previousPosition = transform.position;

            }
            else
            {
                positionWaitTime -= Time.deltaTime;
            }
        }*/
        if (IsServer)
         {
             if(agent.velocity.x - 0.1f >= 0 && isFlipped.Value)
             {
                 isFlipped.Value = false;
             }
             else if(agent.velocity.x + 0.1f < 0 && !isFlipped.Value)
             {
                 isFlipped.Value = true;
             }
         }

    }

    public void InitializeUnit(ulong originId, ulong destinationId)
    {
        originBaseId.Value = originId;
        destinationBaseId.Value = destinationId;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsServer)
        {
            if (collision.CompareTag("Unit"))
            {
                if (collision.gameObject.GetComponent<UnitLogic>().GetUnitOwner() != unitOwner)
                {
                    NetworkObject.Despawn(true);
                }
            }
            if (collision.CompareTag("Base") && collision.gameObject.GetComponent<BaseLogic>() == destinationBase)
            {
                if (destinationBase.GetBaseOwner() == originBase.GetBaseOwner())
                {
                    destinationBase.SetAmountOfUnits(destinationBase.GetAmountOfUnits() + 1);
                }
                else if (destinationBase.GetBaseOwner() != originBase.GetBaseOwner())
                {
                    destinationBase.SetAmountOfUnits(destinationBase.GetAmountOfUnits() - 1);
                    if (destinationBase.GetAmountOfUnits() <= 0)
                    {
                        destinationBase.SetAmountOfUnits(0);
                        destinationBase.SetBaseFaction(originBase.GetBaseFaction());
                        destinationBase.SetBaseOwner(originBase.GetBaseOwner());
                    }
                
                }
                GetComponent<NetworkObject>().Despawn(true);
            }
        }
    }

    public void UnitDestination(Vector2 pos)
    {
        agent.SetDestination(pos);
    }

    public void UnitDestination()
    {
        agent.SetDestination(destination);
    }

    public void SetSpeed(float maxSpeed)
    {
        this.maxSpeed = maxSpeed;
        SetCurrentSpeed(maxSpeed);
    }

    public void SetCurrentSpeed(float currentSpeed)
    {
        this.currentSpeed = currentSpeed;
        agent.speed = currentSpeed;
        if(currentSpeed <= 0)
        {
            GetComponent<Animator>().speed = 0;
        }
        else
        {
            GetComponent<Animator>().speed = 1;
        }
    }

    public float GetSpeed()
    {
        return maxSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public NavMeshAgent GetUnitAgent()
    {
        return agent;
    }

    public PlayerNumber GetUnitOwner()
    {
        return unitOwner;
    }

}

